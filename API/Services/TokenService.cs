using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser oUser)
        {
            var oClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, oUser.UserName)
            };

            var oCreds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var  oTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject=new ClaimsIdentity(oClaims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = oCreds
            };
            
            var oTokenHandler = new JwtSecurityTokenHandler();
            var oToken = oTokenHandler.CreateToken(oTokenDescriptor);

            return oTokenHandler.WriteToken(oToken);

        }

    }
}