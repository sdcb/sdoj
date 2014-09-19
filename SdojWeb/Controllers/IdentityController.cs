using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(Roles = SystemRoles.UserAdmin, EmailConfirmed = true)]
    public class IdentityController : Controller
    {
        public readonly ApplicationUserManager UserMgr;

        public readonly ApplicationRoleManager RoleMgr;

        public readonly ApplicationDbContext DbContext;

        public IdentityController(
            ApplicationUserManager userManager,
            ApplicationRoleManager roleManager, 
            ApplicationDbContext dbContext)
        {
            UserMgr = userManager;
            RoleMgr = roleManager;
            DbContext = dbContext;
        }

        // GET: Identity
        public ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        // GET: Identity/Users/
        public ActionResult Users(int? page, string orderBy, bool? asc)
        {
            var users = UserMgr.Users.Project().To<UserSummaryViewModel>()
                .ToSortedPagedList(page, orderBy, asc);
            return View(users);
        }

        // GET: Identity/Edit/3
        public ActionResult Edit(int id)
        {
            var user = UserMgr.Users.Where(u => u.Id == id)
                .Project().To<UserSummaryViewModel>()
                .FirstOrDefault();
            if (user == null)
            {
                return RedirectToAction("Users").WithError(
                    string.Format("未找到id为{0}的用户。", id));
            }

            var roles = RoleMgr.Roles.ToArray();
            ViewBag.User = user;
            return View(roles);
        }

        // POST: Identity/DeleteUserRole?userId=3&roleId=4
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteUserRole(int userid, int roleid)
        {
            var role = RoleMgr.FindById(roleid);
            var action = RedirectToAction("Edit", new {id = userid});
            if (role == null)
            {
                return action.WithError(
                    string.Format("未找到id为{0}的角色。", roleid));
            }

            var result = UserMgr.RemoveFromRole(userid, role.Name);
            if (!result.Succeeded)
            {
                return action.WithError(
                    string.Format("角色删除失败，因为{0}", string.Join(",", result.Errors)));
            }

            return action.WithSuccess(
                string.Format("角色{0}删除成功。", role.Name));
        }

        // POST: Identity/AddUserRole?userId=3&roleId=4
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddUserRole(int userid, int roleid)
        {
            var role = RoleMgr.FindById(roleid);
            var action = RedirectToAction("Edit", new { id = userid });
            if (role == null)
            {
                return action.WithError(
                    string.Format("未找到id为{0}的角色。", roleid));
            }

            var result = UserMgr.AddToRole(userid, role.Name);
            if (!result.Succeeded)
            {
                return action.WithError(
                    string.Format("角色添加失败，因为{0}", string.Join(",", result.Errors)));
            }

            return action.WithSuccess(
                string.Format("角色{0}添加成功。", role.Name));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(int id)
        {
            if (await DbContext.Questions.AnyAsync(x => x.CreateUserId == id))
            {
                return RedirectToAction("Users").WithError(
                    "删除失败，因为该用户有关联的题目，必须手动删除。");
            }

            var user = await DbContext.Users.FindAsync(id);

            DbContext.Entry(user).State = EntityState.Deleted;
            await DbContext.SaveChangesAsync();

            return RedirectToAction("Users").WithSuccess(
                string.Format("用户{0}及其所有关联资料删除成功。", user.UserName));
        }
    }
}