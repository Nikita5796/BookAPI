using BookApp.API.Entities;
using BookApp.API.Helpers;
using BookApp.BLL;
using BookApp.DAL;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookApp.API.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        private static UserServiceBLL UserObj = new UserServiceBLL(new UserContext());

        public UserService()
        {
            //UserObj = UserObj;
        }

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        //private List<User> _users = UserObj.GetAllUserBLL();

        //private List<User> _users = new List<User>
        //{
        //    new User { UserId=1,UserName="niki",EmailId="niki@gmail.com",Password="niki123", Contact=9876543,Gender="F",Address="pune",StateId=101,CityId=201,PostalCode=200400}
        //};

        private List<User> _users = UserObj.GetUsers();

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string email, string password)
        {
            var user = _users.SingleOrDefault(x => x.EmailId == email && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // remove password before returning
            user.Password = null;

            return user;
        }
    }
}
