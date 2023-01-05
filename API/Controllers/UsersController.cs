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
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        private readonly IUnitOfWork _uow;

        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService) 
        {
            this._uow = uow;
            this.mapper = mapper;
            this.photoService = photoService;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams){
            var gender = await this._uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female" : "male";

            var users = await this._uow.UserRepository.GetMembersAsync(userParams);//await this._uow.UserRepository.GetUsersAsync();
            //var usersToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, 
            users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username){
            var user = await this._uow.UserRepository.GetMemberAsync(username);
            
            return user;//this.mapper.Map<MemberDto>(user);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto){
             var username = User.GetUsername();
             var user = await this._uow.UserRepository.GetUserByUserNameAsync(username);

             this.mapper.Map(memberUpdateDto, user);   

             this._uow.UserRepository.Update(user);

             if(await this._uow.Complete()) return NoContent();

             return BadRequest("Failed to upda user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file){
            var user = await this._uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());
            var result = await this.photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo{
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0){
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if(await this._uow.Complete()){
                //return this.mapper.Map<PhotoDto>(photo);
                //return CreatedAtRoute("GetUser", this.mapper.Map<PhotoDto>(photo));
                return CreatedAtRoute("GetUser", new {username = user.UserName}, this.mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problme adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId){
            var user = await this._uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await this._uow.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId){
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("cannot delete main");
            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null){
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if(await _uow.Complete()) return Ok();

            return BadRequest("failed to delete");
        }
    }
}