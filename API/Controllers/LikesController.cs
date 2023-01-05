using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {
            this._uow = uow;

        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username){
            var sourceUserId = User.GetUserId();   
            var likeUser = await _uow.UserRepository.GetUserByUserNameAsync(username);
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likeUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likeUser.Id);

            if (userLike != null) return BadRequest("You already like this user");

            userLike = new Entities.UserLike{
                SourceUserId = sourceUserId,
                LikedUserId = likeUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if(await _uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams){
            likesParams.UserId = User.GetUserId();
            var users = await _uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}