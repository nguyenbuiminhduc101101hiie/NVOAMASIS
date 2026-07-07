using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace NVOAMASIS.Services;

public class AccountService (IUserService userServices, AppDbContext _context, CustomAuthenticationStateProvider _cusAuth)
{
    
    public async Task<LoginResponse> Authenticate(string userCredential)
    {
        return await userServices.SignInAsync(userCredential);
    }
    public async Task<AuthenticationState> GetAuth()
    {
        return await _cusAuth.GetAuth();

    }

    public AuthUser GetUserDetail()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var user = GetAuth().Result.User.Identity!.Name!;
            var rs = _context.UserList.FirstOrDefault(x => x.Name == user);
            return rs!;
        }
        catch(Exception ex)
        {
            return new AuthUser();
        }
    }
    public AuthUser GetUserDetail_byemail(string email)
    {
        try
        {
            _context.ChangeTracker.Clear();
      
            var rs = _context.UserList.Where(x=>x.Email == email).FirstOrDefault();
            return rs!;
        }
        catch (Exception ex)
        {
            return new AuthUser();
        }
    }
    public async Task<AuthUser> GetUserDetail_byname(string name)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name);
            return rs!;
        }
        catch (Exception ex)
        {
            return new AuthUser();
        }
    }

    public async Task<List<string>> GetList_User_handle()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.UserList.Where(x => x.Name != null)
                .Select(x => x.Usr).Distinct().OrderBy(x => x).ToListAsync();
            rs.Insert(0, "");
            return rs!;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null!;
        }
    }
    public async Task<List<AuthUser>> GetListUser()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var result = await _context.UserList.OrderByDescending(_ => _.Name).ToListAsync();
            result.ForEach(user => user.Pass_viettel = null);
            return result;
        }
        catch (Exception ex)
        {
            return new List<AuthUser>();
        }
    }
  
    public async Task<BoolandMessReponse> RegisterUser(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (user == null) return new BoolandMessReponse(true, "User is null!");
            if (string.IsNullOrWhiteSpace(user.Email))
                return new BoolandMessReponse(false, "Email is required!");
            if (!new EmailAddressAttribute().IsValid(user.Email.Trim()))
                return new BoolandMessReponse(false, "Email không hợp lệ.");
            user.Email = user.Email.Trim();
            //user.Pass_viettel = HashPasswordUsingBcrypt(user.Pass_viettel!);
            user.UsrId = Guid.NewGuid();
            user.Usr = user.Name;
            user.NickName = user.Name;
            user.Manager2 = "";
            _context.UserList.Add(user);

            //create permission
            var ps = await _context.PermissionTemplate.Where(x => x.Dept!.ToLower() == user.Department!.ToLower()).ToListAsync();
            var listpers = new List<Permission_M>();
            foreach (var item in ps)
            {
                var p = new Permission_M();
                p.PermissionId = Guid.NewGuid();
                p.MenuId = item.MenuId;
                p.MenuName = item.MenuName;
                p.Add = item.canAdd;
                p.See = item.canView;
                p.Edit = item.canEdit;
                p.Del = item.canDelete;
                p.Approve = item.canApprove;
                p.UserName = user.Usr;
                listpers.Add(p);
            }

            // permission defalt 
            var perdef = await _context.PermissionTemplate.Where(x => x.Dept == "ALL").FirstOrDefaultAsync();
            if (perdef != null)
            {
                var listmenu = await _context.MenuNames.ToListAsync();
                foreach(var menu in listmenu)
                {
                    var p = new Permission_M();
                    p.PermissionId = Guid.NewGuid();
                    p.MenuId = menu.MenuID;
                    p.MenuName = menu.MenuName;
                    p.Add = perdef.canAdd;
                    p.See = perdef.canView;
                    p.Edit = perdef.canEdit;
                    p.Del = perdef.canDelete;
                    p.Approve = perdef.canApprove;
                    p.UserName = user.Usr;
                    listpers.Add(p);
                }
            }

            _context.Permissions.AddRange(listpers);
            await _context.SaveChangesAsync();

            return new BoolandMessReponse(true, "Register Successfully!");
        }
        catch(Exception ex)
        {
            return new BoolandMessReponse(false, "Register Fail!");
        }
    }
    public async Task<BoolandMessReponse> UpdateUser(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (user == null) return new BoolandMessReponse(true, "User is null!");
            if(string.IsNullOrEmpty(user.Pass_viettel))
            {
                var rs = await GetUserDetail_byname(user.Name);
                user.Pass_viettel = rs.Pass_viettel;
            }
            //else
            //    user.Pass_viettel = HashPasswordUsingBcrypt(user.Pass_viettel!);
            _context.UserList.Update(user);
            await _context.SaveChangesAsync();

            return new BoolandMessReponse(true, "Update Successfully!");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Update Fail!");
        }
    }

    public async Task<BoolandMessReponse> DeleteUser(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (user?.UsrId == null || user?.UsrId == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");
            _context?.UserList.Remove(user!);

            //delete permission
            var pers = await _context?.Permissions.Where(x => x.UserName == user.Usr).ToListAsync()!;
            _context.Permissions.RemoveRange(pers);

            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete User with error code: " + ex.Message);
        }
    }
    public string HashPasswordUsingBcrypt(string pass)
    {
        return BCrypt.Net.BCrypt.HashPassword(pass);
    }

}
