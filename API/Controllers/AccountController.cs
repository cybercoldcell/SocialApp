using System.Security.Cryptography;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using System.Linq;

namespace API.Controllers
{
    public class AccountController : BaseController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO oDTO)
        {
            if (await IsUserExists(oDTO.UserName)) return BadRequest("User name is taken.");

            using var oHmac = new HMACSHA512();
            var oUser = new AppUser
            {
                UserName = oDTO.UserName,
                PasswordHash = oHmac.ComputeHash(Encoding.UTF8.GetBytes(oDTO.Password)),
                PasswordSalt = oHmac.Key
            };

            _context.Users.Add(oUser);
            await _context.SaveChangesAsync();

            return new UserDTO{
                UserName = oUser.UserName,
                Token = _tokenService.CreateToken(oUser)
            };

        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> UserLogin(LogInDTO oDTO)
        {
            var oUser = await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == oDTO.UserName);
            if (oUser == null) return Unauthorized("Invalid user name.");

            using var oHmac = new HMACSHA512(oUser.PasswordSalt);
            var oHash = oHmac.ComputeHash(Encoding.UTF8.GetBytes(oDTO.Password));

            for (int i = 0; i < oHash.Length; i++)
            {
                if (oHash[i] != oUser.PasswordHash[i]) return Unauthorized("Invalid password.");
            }

            return new UserDTO{
                UserName = oUser.UserName,
                Token = _tokenService.CreateToken(oUser),
                PhotoUrl = oUser.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };


        }

        private async Task<bool> IsUserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

    }

}