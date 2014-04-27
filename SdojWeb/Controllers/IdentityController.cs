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

        public ActionResult Users()
        {
            var users = UserMgr.Users.Project().To<UserSummaryViewModel>().ToArray();
            return View(users);
        }

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
    }
}