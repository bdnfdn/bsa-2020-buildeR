﻿using buildeR.BLL.Interfaces;
using buildeR.Common.DTO.Synchronization.Github;
using buildeR.Common.Enums;
using buildeR.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace buildeR.BLL.Services
{
    public class GithubClient : IGithubClient
    {
        private readonly HttpClient _client;
        private readonly BuilderContext _context;
        public GithubClient(IHttpClientFactory factory, BuilderContext context)
        {
            _client = factory.CreateClient("github");
            _context = context;
        }

        public async Task<GithubUser> GetUserFromToken(int userId, string providerToken)
        {
            SetUpHttpClient(providerToken);

            throw new NotImplementedException();
            //todo
        }

        public async Task<IEnumerable<GithubBranch>> GetRepositoryBranches(int userId, string repositoryName, string providerToken)
        {
            SetUpHttpClient(providerToken);

            throw new NotImplementedException();
            //todo
        }

        public async Task<IEnumerable<GithubRepository>> GetUserRepositories(int userId, string providerToken)
        {
            SetUpHttpClient(providerToken);

            var endpoint = $"user/repos?visibility=all&affiliation=owner";
            var response = await _client.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<GithubRepository>>(content);
        }

        private void SetUpHttpClient(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
        }
    }
}