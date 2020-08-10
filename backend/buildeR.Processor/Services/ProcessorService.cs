﻿using buildeR.Common.DTO.BuildHistory;
using buildeR.RabbitMq.Interfaces;

using Docker.DotNet;
using Docker.DotNet.Models;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

using LibGit2Sharp;

using Newtonsoft.Json;

using Serilog;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace buildeR.Processor.Services
{
    public class ProcessorService : IProcessorService, IDisposable
    {
        #region Properties\fields
        private readonly IConsumer _consumer;
        private readonly DockerClient _dockerClient;
        private readonly string _pathToProjects;
        private string _pathToFile;

        private string _containerId;

        private string DockerApiUri => IsCurrentOsLinux ? "unix:/var/run/docker.sock" : "npipe://./pipe/docker_engine";
        private bool IsCurrentOsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private string PathToWorkDirectory => Path.Combine(_pathToFile, "ClonedRepository");
        #endregion

        public ProcessorService(IConsumer consumer)
        {
            _pathToProjects = Path.Combine(Path.GetTempPath(), "buildeR", "Projects");
            _dockerClient = new DockerClientConfiguration(new Uri(DockerApiUri)).CreateClient();

            _consumer = consumer;
            _consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object sender, RabbitMQ.Client.Events.BasicDeliverEventArgs e)
        {
            var key = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            var executeBuild = JsonConvert.DeserializeObject<ExecutiveBuildDTO>(message);
            BuildProjectAsync(executeBuild);
            _consumer.SetAcknowledge(e.DeliveryTag, true);
        }

        public void Register()
        {
            _consumer.Consume();
        }

        public void Unregister()
        {
            Log.Information(" >>>> Deregister");
        }

        public async Task BuildProjectAsync(ExecutiveBuildDTO build)
        {
            _pathToFile = Path.Combine(_pathToProjects, build.ProjectId.ToString());

            await RunTestContainerAsync();
            //await CreateImageAsync(build.RepositoryUrl);
        }

        #region Docker Test
        private async Task RunTestContainerAsync()
        {
            const string imageName = "hello-world";
            await PullImageByNameAsync(imageName);
            await CreateAndRunContainerAsync(imageName, 8002);
            var logStream = await _dockerClient
                .Containers
                .GetContainerLogsAsync(_containerId, new ContainerLogsParameters() { ShowStderr = true, ShowStdout = true });
            using (var streamReader = new StreamReader(logStream, Encoding.UTF8))
            {
                var logs = await streamReader.ReadToEndAsync();
                Debug.WriteLine(logs);
            }
        }

        private async Task PullImageByNameAsync(string imageName)
        {
            try
            {
                await _dockerClient
                    .Images
                    .CreateImageAsync(new ImagesCreateParameters
                    {
                        FromImage = imageName,
                        Tag = "latest"
                    },
                        new AuthConfig(),
                        new Progress<JSONMessage>());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async Task CreateAndRunContainerAsync(string imageName, int port)
        {
            try
            {
                var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = imageName,
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        {
                            port.ToString(), default
                        }
                    },
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {port.ToString(), new List<PortBinding> {new PortBinding {HostPort = port.ToString()}}}
                        },
                        PublishAllPorts = true
                    }
                });

                _containerId = response.ID;

                await _dockerClient.Containers.StartContainerAsync(_containerId, null);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        #endregion

        #region Docker
        private async Task CreateImageAsync(string repositoryUrl)
        {
            CloneRepository(repositoryUrl);

            var targetFolder = Directory.GetParent(PathToWorkDirectory).FullName;
            CreateTarGzFileFromFolder(PathToWorkDirectory, "ArchToBuild", targetFolder);

            await using (var stream = File.OpenRead(Path.Combine(targetFolder, "ArchToBuild.tar.gz")))
            {
                try
                {
                    var imageStream = await _dockerClient
                        .Images
                        .BuildImageFromDockerfileAsync(
                            stream,
                            new ImageBuildParameters()
                            {
                                Dockerfile = "Dockerfile"
                            });

                    await _dockerClient
                        .Images
                        .CreateImageAsync(
                            new ImagesCreateParameters() { Tag = "latest" },
                            imageStream,
                            new AuthConfig(),
                            new Progress<JSONMessage>());

                    var availableImages = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters() { All = true });
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
        #endregion

        #region Tar.gz
        private string CreateTarGzFileFromFolder(string sourceDirectory, string tgzFileName, string targetDirectory, bool deleteSourceDirectoryUponCompletion = false)
        {
            if (!tgzFileName.EndsWith(".tar.gz"))
            {
                tgzFileName = $"{tgzFileName}.tar.gz";
            }

            using var outStream = File.Create(Path.Combine(targetDirectory, tgzFileName));
            using (var gzoStream = new GZipOutputStream(outStream))
            {
                var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

                tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                {
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
                }

                AddDirectoryFilesToTarGz(tarArchive, sourceDirectory);

                if (deleteSourceDirectoryUponCompletion)
                {
                    File.Delete(sourceDirectory);
                }

                var tgzPath = (tarArchive.RootPath + ".tar.gz").Replace('/', '\\');

                tarArchive.Close();
                return tgzPath;
            }
        }

        private void AddDirectoryFilesToTarGz(TarArchive tarArchive, string sourceDirectory)
        {
            AddDirectoryFilesToTarGz(tarArchive, sourceDirectory, string.Empty);
        }

        private void AddDirectoryFilesToTarGz(TarArchive tarArchive, string sourceDirectory, string currentDirectory)
        {
            var pathToCurrentDirectory = Path.Combine(sourceDirectory, currentDirectory);
            foreach (var filePath in Directory.GetFiles(pathToCurrentDirectory))
            {
                var tarEntry = TarEntry.CreateEntryFromFile(filePath);

                tarEntry.Name = filePath.Replace(sourceDirectory, "");

                if (tarEntry.Name.StartsWith('\\'))
                {
                    tarEntry.Name = tarEntry.Name.Substring(1);
                }
                tarArchive.WriteEntry(tarEntry, true);
            }

            foreach (var directory in Directory.GetDirectories(pathToCurrentDirectory))
            {
                AddDirectoryFilesToTarGz(tarArchive, sourceDirectory, directory);
            }
        }
        #endregion

        #region Directory
        private void CloneRepository(string repositoryUrl)
        {
            try
            {
                EnsureExistDirectory(PathToWorkDirectory);
                Repository.Clone(repositoryUrl, PathToWorkDirectory);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private void EnsureExistDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        private void DeleteClonedRepository(string path)
        {
            foreach (var sub in Directory.EnumerateDirectories(path))
            {
                DeleteClonedRepository(sub);
            }
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var fileInfo = new FileInfo(file) { Attributes = FileAttributes.Normal };
                fileInfo.Delete();
            }
            Directory.Delete(path);
        }
        #endregion

        public void Dispose()
        {
        }
    }
}
