using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(Roles = "admin")]
    public class IdentityController : Controller
    {
        public ApplicationUserManager UserMgr { get; set; }

        public ApplicationRoleManager RoleMgr { get; set; }

        public IdentityController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserMgr = userManager;
            RoleMgr = roleManager;
        }

        // GET: Identity
        public ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        // GET: Identity/Users/
        public ActionResult Users()
        {
            var users = UserMgr.Users.Project().To<UserSummaryViewModel>().ToArray();
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
                return RedirectToAction("Users").WithError("未找到id为{0}的用户。", id);
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
                return action.WithError("未找到id为{0}的角色。", roleid);
            }

            var result = UserMgr.RemoveFromRole(userid, role.Name);
            if (!result.Succeeded)
            {
                return action.WithError("角色删除失败，因为{0}", string.Join(",", result.Errors));
            }

            return action.WithSuccess("角色{0}删除成功。", role.Name);
        }

        // POST: Identity/AddUserRole?userId=3&roleId=4
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddUserRole(int userid, int roleid)
        {
            var role = RoleMgr.FindById(roleid);
            var action = RedirectToAction("Edit", new { id = userid });
            if (role == null)
            {
                return action.WithError("未找到id为{0}的角色。", roleid);
            }

            var result = UserMgr.AddToRole(userid, role.Name);
            if (!result.Succeeded)
            {
                return action.WithError("角色添加失败，因为{0}", string.Join(",", result.Errors));
            }

            return action.WithSuccess("角色{0}添加成功。", role.Name);
        }

        // GET: Identity/Roles
        public ActionResult Roles()
        {
            var roles = RoleMgr.Roles.ToArray();
            return View(roles);
        }

        // POST: Identity/AddRole?name=xyz
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddRole(string name)
        {
            var action = RedirectToAction("Roles");
            if (RoleMgr.RoleExists(name))
            {
                return action.WithError("已经存在名为{0}的角色。", name);
            }

            var role = new ApplicationRole(name);
            var result = RoleMgr.Create(role);
            if (!result.Succeeded)
            {
                return action.WithError("新建角色失败，因为{0}", string.Join(",", result.Errors));
            }
            return action.WithSuccess("角色{0}新建成功。", role.Name);
        }

        // POST: Identity/DeleteRole?roleId=3
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteRole(int roleId)
        {
            var action = RedirectToAction("Roles");
            var role = RoleMgr.FindById(roleId);
            if (role == null)
            { 
                return action.WithError("不存在id为{0}的角色。", roleId);
            }

            var result = RoleMgr.Delete(role);
            if (!result.Succeeded)
            {
                return action.WithError("删除角色失败，因为{0}", string.Join(",", result.Errors));
            }
            return action.WithSuccess("角色{0}删除成功。", role.Name);
        }
    }
}