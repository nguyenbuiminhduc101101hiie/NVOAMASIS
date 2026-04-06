using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Components.Account.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using System;

namespace NVOAMASIS.Services
{
    public class UserServicecs(AppDbContext _context) : IUserService
    {
        
        public async Task<AuthUser> FindUserByEmail(string usr) =>
            await _context.UserList.FirstOrDefaultAsync(_ => _.Usr!.ToLower()!.Equals(usr.ToLower()));


        public async Task<LoginResponse> SignInAsync(string userCredential)
        {
            /*f (user is null) return new LoginResponse(false, "Model is empty");
            var applicationUser = await FindUserByEmail(user.Email);
            if (applicationUser is null) return new LoginResponse(false, "User not found");*/

            /*//Verify Password
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Email/Password not valid");

            var getUserRole = await FindUserRole(applicationUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "user role not found");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getRoleName is null) return new LoginResponse(false, "user role not found");

            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            string refreshToken = GenerateRefreshToken();

            //Save the Refresh token to the database
            var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(_ => _.UserId == applicationUser.Id);
            if (findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbContext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new RefreshTokenInfo() { Token = refreshToken, UserId = applicationUser.Id });
            }*/
            if (userCredential is null) return new LoginResponse(false, "Model is empty");

            var parser = userCredential.Split(':');
            string usr = parser[0].Trim();
            string password = parser[1].Trim();
            bool rememberMe = "True".Equals(parser[2]);

            var applicationUser = await FindUserByEmail(usr);
            if (applicationUser is null) return new LoginResponse(false, "User not found");

            if (password != applicationUser.Pass_viettel) return new LoginResponse(false, "User/Password not valid");


            var user = new AuthUser
            {
                Usr = applicationUser.Usr,
                Name = applicationUser.Name,
				UsrId = applicationUser.UsrId,
                Email = applicationUser.Email,
                Manager2 = applicationUser.Manager2,
                Department = applicationUser.Department
				//RememberMe = rememberMe,
			};
            return new LoginResponse(true, "Login successfully", user);
        }
    }
}
