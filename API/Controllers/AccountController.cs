using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto){

            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            var user = this.mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();
 
            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
 
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await this.context.Users.Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.UserName == loginDto.username.ToLower());

            if(user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] == user.PasswordHash[i])
                    continue;
                return Unauthorized("Invalid password");
            }

            return new UserDto{
                Username = user.UserName,
                Token = this.tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username){
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}