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
            if (user == null) return new BoolandMessReponse(false, "User is null!");
            if (string.IsNullOrWhiteSpace(user.Name))
                return new BoolandMessReponse(false, "User name is required!");
            if (string.IsNullOrWhiteSpace(user.Email))
                return new BoolandMessReponse(false, "Email is required!");
            if (!new EmailAddressAttribute().IsValid(user.Email.Trim()))
                return new BoolandMessReponse(false, "Email không hợp lệ.");
            if (string.IsNullOrWhiteSpace(user.Department))
                return new BoolandMessReponse(false, "Department is required!");

            user.Email = user.Email.Trim();
            user.Name = user.Name.Trim();
            user.UsrId = Guid.NewGuid();
            user.Usr = user.Name;
            user.NickName = user.Name;
            user.Manager2 = "";

            var userExists = await _context.UserList.AsNoTracking()
                .AnyAsync(x => x.Usr == user.Usr || x.Name == user.Name);
            if (userExists)
                return new BoolandMessReponse(false, $"User '{user.Usr}' đã tồn tại. Vui lòng chọn tên khác.");

            _context.UserList.Add(user);

            // Quyền đã có sẵn cho UserName này (tránh PK trùng nếu còn rác dữ liệu cũ).
            var existingMenuKeys = await _context.Permissions.AsNoTracking()
                .Where(x => x.UserName == user.Usr)
                .Select(x => new { x.MenuId, x.MenuName })
                .ToListAsync();

            var usedMenuIds = existingMenuKeys
                .Where(x => x.MenuId != null)
                .Select(x => x.MenuId!.Value)
                .ToHashSet();
            var usedMenuNames = existingMenuKeys
                .Where(x => !string.IsNullOrWhiteSpace(x.MenuName))
                .Select(x => x.MenuName!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var listpers = new List<Permission_M>();

            void TryAddPermission(Guid? menuId, string? menuName, bool add, bool see, bool edit, bool del, bool approve)
            {
                if (menuId.HasValue && usedMenuIds.Contains(menuId.Value))
                    return;
                if (!string.IsNullOrWhiteSpace(menuName) && usedMenuNames.Contains(menuName))
                    return;

                listpers.Add(new Permission_M
                {
                    PermissionId = Guid.NewGuid(),
                    MenuId = menuId,
                    MenuName = menuName,
                    Add = add,
                    See = see,
                    Edit = edit,
                    Del = del,
                    Approve = approve,
                    UserName = user.Usr
                });

                if (menuId.HasValue)
                    usedMenuIds.Add(menuId.Value);
                if (!string.IsNullOrWhiteSpace(menuName))
                    usedMenuNames.Add(menuName);
            }

            // 1) Template theo phòng ban (ưu tiên)
            var dept = user.Department.Trim().ToLower();
            var deptTemplates = await _context.PermissionTemplate
                .AsNoTracking()
                .Where(x => x.Dept != null && x.Dept.ToLower() == dept)
                .ToListAsync();

            foreach (var item in deptTemplates)
            {
                TryAddPermission(
                    item.MenuId,
                    item.MenuName,
                    item.canAdd ?? false,
                    item.canView ?? false,
                    item.canEdit ?? false,
                    item.canDelete ?? false,
                    item.canApprove ?? false);
            }

            // 2) Template ALL: chỉ bổ sung menu chưa có từ bước 1
            var perdef = await _context.PermissionTemplate.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Dept == "ALL");
            if (perdef != null)
            {
                var listmenu = await _context.MenuNames.AsNoTracking().ToListAsync();
                foreach (var menu in listmenu)
                {
                    TryAddPermission(
                        menu.MenuID,
                        menu.MenuName,
                        perdef.canAdd ?? false,
                        perdef.canView ?? false,
                        perdef.canEdit ?? false,
                        perdef.canDelete ?? false,
                        perdef.canApprove ?? false);
                }
            }

            if (listpers.Count > 0)
                _context.Permissions.AddRange(listpers);

            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Register Successfully!");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("PK_Permissions", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true)
        {
            return new BoolandMessReponse(false,
                "Register Fail: quyền menu bị trùng (Permissions). Kiểm tra PermissionTemplate hoặc user đã có quyền sẵn.");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Register Fail: " + (ex.InnerException?.Message ?? ex.Message));
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
