﻿using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SdojWeb.Infrastructure;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Models;
using Microsoft.Web.Mvc;
using SdojWeb.Infrastructure.Alerts;

namespace SdojWeb.Controllers
{
    [SdojAuthorize(EmailConfirmed = false)]
    public class AccountController : Controller
    {
        public readonly ApplicationUserManager UserManager;
        public readonly ApplicationDbContext DbContext;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationDbContext dbContext)
        {
            UserManager = userManager;
            DbContext = dbContext;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (!await UserManager.CheckPasswordAsync(user, model.Password))
                {
                    user = null;
                }


                if (user != null)
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError("", "用户名或密码无效。");
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> LoginAsJudger(string username, string password)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (!await UserManager.CheckPasswordAsync(user, password))
                user = null;

            if (user != null && user.EmailConfirmed && user.Roles.Any(x => x.Name == SystemRoles.Judger))
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToStringInvariant()));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, SystemRoles.Judger));

                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

                return new EmptyResult();
            }

            return new HttpUnauthorizedResult();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.UserName, Email = model.Email };
                using (var tran = TransactionInRequest.BeginTransaction())
                {
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);

                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmUser", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "确认你的账户",
                            "请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来确认你的帐号");

                        tran.Complete();

                        return this.RedirectToAction<HomeController>(x => x.Index())
                            .WithInfo("已经向你的邮箱" + user.Email + "发送了验证邮件，请前往并点击该邮件中的链接以验证您的帐户。");
                    }

                    AddErrors(result);
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // GET: /Account/ConfirmUser
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmUser(int? userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            IdentityResult result = await UserManager.ConfirmEmailAsync(userId.Value, code);
            if (result.Succeeded)
            {
                return View("ConfirmUser");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ReSendConfirmEmail
        public async Task<ActionResult> ReSendConfirmEmail()
        {
            var userId = User.Identity.GetUserId<int>();
            var user = DbContext.Users.Find(userId);
            if (user.EmailConfirmed)
            {
                await SignInAsync(user, false);
                return RedirectToAction("Index", "Home").WithSuccess("你的帐号已通过验证，不需要重发验证邮件。");
            }

            return View();
        }

        //
        // POST: /Account/ReSendConfirmEmail
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ReSendConfirmEmail(string userId)
        {
            if (User.EmailConfirmed())
            {
                return this.RedirectToAction<HomeController>(x => x.Index())
                    .WithInfo("您的用户已经通过邮件验证，不需要再次验证。");
            }

            var userid = User.Identity.GetUserId<int>();
            var email = User.Identity.GetUserName();
            var code = await UserManager.GenerateEmailConfirmationTokenAsync(userid);
            var callbackUrl = Url.Action("ConfirmUser", "Account", new { userId = userid, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(User.Identity.GetUserId<int>(), "确认你的账户",
                "请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来确认你的账号");


            return this.RedirectToAction<HomeController>(x => x.Index())
                .WithInfo("已经向你的邮箱" + email + "发送了验证邮件，请前往并点击该邮件中的链接以验证您的帐户。");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    ModelState.AddModelError("", "用户不存在或未确认。");
                    return View();
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //await SendEmail(user.Email, callbackUrl, "ResetPassword", "请单击此处重置你的密码");
                await UserManager.SendEmailAsync(user.Id, "重置密码",
                    "请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来重置你的密码");

                return this.RedirectToAction(x => x.ForgotPasswordConfirmation());
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (code == null)
            {
                return View("Error");
            }
            return View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "找不到用户。");
                    return View();
                }
                IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return this.RedirectToAction(x => x.ResetPasswordConfirmation());
                }
                else
                {
                    AddErrors(result);
                    return View();
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                await SignInAsync(user, isPersistent: false);
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return this.RedirectToAction(x => x.Manage(message));
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "你的密码已更改。"
                : message == ManageMessageId.SetPasswordSuccess ? "已设置你的密码。"
                : message == ManageMessageId.RemoveLoginSuccess ? "已删除外部登录名。"
                : message == ManageMessageId.Error ? "出现错误。"
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                        await SignInAsync(user, isPersistent: false);
                        return this.RedirectToAction(x => Manage(ManageMessageId.ChangePasswordSuccess));
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // 用户没有密码，因此将删除由于缺少 OldPassword 字段而导致的所有验证错误
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId<int>(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return this.RedirectToAction(x => Manage(ManageMessageId.SetPasswordSuccess));
                    }
                    AddErrors(result);
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 请求重定向到外部登录提供程序
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return this.RedirectToAction(x => x.Login(returnUrl));
            }

            // 如果用户已具有登录名，则使用此外部登录提供程序将该用户登录
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            // 如果用户没有帐户，则提示该用户创建帐户
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // 请求重定向到外部登录提供程序，以链接当前用户的登录名
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return this.RedirectToAction(x => Manage(ManageMessageId.Error));
            }
            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo.Login);
            if (result.Succeeded)
            {
                ManageMessageId? nouse = null;
                return this.RedirectToAction(x => Manage(nouse));
            }

            return this.RedirectToAction(x => x.Manage(ManageMessageId.Error));
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                ManageMessageId? nouse = null;
                return this.RedirectToAction(x => x.Manage(nouse));
            }

            if (ModelState.IsValid)
            {
                // 从外部登录提供程序获取有关用户的信息
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { Email = model.Email, UserName = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);

                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmUser", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "确认你的账户",
                            "请通过单击 <a href=\"" + callbackUrl + "\">此处</a>来确认你的帐号");

                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return this.RedirectToAction<HomeController>(x => x.Index());
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId<int>());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        //
        // POST: /Account/CheckEmail
        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> CheckEmail(string email)
        {
            var exist = await DbContext.Users.AnyAsync(x => x.Email == email);
            return Json(!exist);
        }

        // 
        // POST: /Account/CheckUserName
        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> CheckUserName(string username)
        {
            var exist = await DbContext.Users.AnyAsync(x => x.UserName == username);
            return Json(!exist);
        }

        // 
        // POST: /Account/DeleteMe
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteMe()
        {
            var userId = User.Identity.GetUserId<int>();
            if (await DbContext.Questions.AnyAsync(x => x.CreateUserId == userId))
            {
                return RedirectToAction("Manage", "Account").WithError(
                    "删除失败，因为您有关联的题目，必须手动删除。");
            }

            var user = await DbContext.Users.FindAsync(userId);
            DbContext.Entry(user).State = EntityState.Deleted;
            await DbContext.SaveChangesAsync();

            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home").WithSuccess(
                string.Format("用户{0}及其所有关联资料删除成功。", user.UserName));
        }

        #region 帮助程序
        // 用于在添加外部登录名时提供 XSRF 保护
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(User user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await user.GenerateUserIdentityAsync(UserManager);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return this.RedirectToAction<HomeController>(x => x.Index());
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri, string userId = null)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            private string LoginProvider { get; set; }
            private string RedirectUri { get; set; }
            private string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}