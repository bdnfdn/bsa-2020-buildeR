﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using buildeR.BLL.Interfaces;
using buildeR.Common.DTO.User;
using buildeR.DAL.Entities;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace buildeR.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ICollection<UserDTO>> Get()
        {
            return await _userService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<UserDTO> Get(int id)
        {
            return await _userService.GetUserById(id);
        }

        [HttpGet("getbyuid/{UId}")]
        public async Task<UserDTO> GetByUId(string UId)
        {
            return await _userService.GetUserByUId(UId);
        }

        [HttpPost]
        public async Task<UserDTO> Create([FromBody] NewUserDTO user)
        {
            return await _userService.Create(user);
        }

        [HttpPut]
        public async Task<UserDTO> Update([FromBody] UserDTO user)
        {
            return await _userService.Update(user);
        }
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _userService.Delete(id);
        }
    }
}