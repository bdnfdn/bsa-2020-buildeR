﻿using buildeR.Common.DTO.Repository;

namespace buildeR.Common.DTO.Project
{
    public sealed class NewProjectDTO
    {
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public NewRepositoryDTO Repository { get; set; }
    }
}
