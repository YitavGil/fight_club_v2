using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] // account/register
    public async Task<ActionResult<UserDTO>> Register(RegisterDto registerDto) 
    {

        if (await UserExist(registerDto.UserName)) return BadRequest("Username is taken");
        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

         return new UserDTO
         {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
         };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDto loginDto) 
    {
        var user = await context.Users.FirstOrDefaultAsync(x => 
            x.UserName == loginDto.UserName.ToLower());

        if(user == null) return Unauthorized("invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("Password is incorrect");
        }

         return new UserDTO
         {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
         };
    }

   private async Task<bool> UserExist(string username)
   {
    return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
   }
}

