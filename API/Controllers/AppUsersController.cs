using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using API.DTOs;
using System.Security.Claims;
using API.Extensions;
using Microsoft.AspNetCore.Http;

namespace API.Controllers
{

    [Authorize]
    public class AppUsersController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public AppUsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _mapper = mapper;
            _photoService = photoService;
            _userRepository = userRepository;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetAppUsers()
        {
            /* var users = await _userRepository.GetUsersAsync();
            var usersToReturn = _mapper.Map<IEnumerable<MemberDTO>>(users);
            return Ok(usersToReturn); */

            var users = await _userRepository.GetMembersAsync();
            return Ok(users);

        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetAppUserByName(string username)
        {
           /*  var user = await _userRepository.GetUserByNameAsync(username); 
            return _mapper.Map<MemberDTO>(user); */
            
            return await _userRepository.GetMemberByNameAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUserName());
            _mapper.Map(memberUpdateDTO, user);

            _userRepository.Update(user);

            if(await _userRepository.SaveAsync()) return NoContent();

            return BadRequest("Failed to update the user.");  
            
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUserName());
            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PubID = result.PublicId
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await _userRepository.SaveAsync())
            {
                //return _mapper.Map<PhotoDTO>(photo);
                return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDTO>(photo));

            }
                
            
            return BadRequest("Error occurs when adding photo.");
 
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo.IsMain) return BadRequest("This is already your main photo");

            var main = user.Photos.FirstOrDefault(x => x.IsMain);
            if(main != null) main.IsMain = false;
            photo.IsMain = true;

            if(await _userRepository.SaveAsync()) return NoContent();
            return BadRequest("Error occurs when setting main photo.");
            
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            
            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("Sorry, you cannot delete your main photo");

            if(photo.PubID != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PubID);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if(await _userRepository.SaveAsync()) return Ok();

            return BadRequest("Error occurs when deleting your requested photo");
        }

    }

}