using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper) 
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers(){
            var users = await this.userRepository.GetMembersAsync();//await this.userRepository.GetUsersAsync();
            //var usersToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username){
            var user = await this.userRepository.GetMemberAsync(username);
            
            return user;//this.mapper.Map<MemberDto>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){
             var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             var user = await this.userRepository.GetUserByUserNameAsync(username);

             this.mapper.Map(memberUpdateDto, user);   

             this.userRepository.Update(user);

             if(await this.userRepository.SaveAllAsync()) return NoContent();

             return BadRequest("Failed to update user");
        }
    }
}