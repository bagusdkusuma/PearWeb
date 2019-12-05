﻿using DSLNG.PEAR.Services.Interfaces;
using DSLNG.PEAR.Services.Requests.User;
using DSLNG.PEAR.Services.Responses.User;
using DSLNG.PEAR.Web.DependencyResolution;
using DSLNG.PEAR.Web.ViewModels;
using DSLNG.PEAR.Web.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DSLNG.PEAR.Common.Extensions;
using System.Web.Script.Serialization;
using DSLNG.PEAR.Common.Helpers;

namespace DSLNG.PEAR.Web.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string message)
        {
            ViewBag.message = message;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(UserLoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(user.Email, user.Password))
                {
                    //FormsAuthentication.SetAuthCookie(user.Username, false);
                    var sessionData = (UserProfileSessionData)this.Session["LoginUser"];
                    var RedirectUrl = sessionData.RedirectUrl;
                    if (RedirectUrl != null)
                    {
                        return Redirect(RedirectUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect Login Data");
                }

            }
            else
            {
                ModelState.AddModelError("", "Incorrect Login Credential");
            }
            return View(user);
        }

        private bool IsValid(string email, string password)
        {
            var hostname = string.Empty;
            if (Request.ServerVariables["REMOTE_ADDR"] != null)
            {
              hostname = DomainHelper.GetComputerName(Request.ServerVariables["REMOTE_ADDR"]);
            }
            var user = _userService.Login(new LoginUserRequest { Email = email, Password = password, IpAddress = Request.UserHostAddress, Browser = Request.UserAgent, HostName = hostname });
            if (user != null && user.IsSuccess)
            {
                /* Try Get Current User Role
                 */
                //this._createRole(user.RoleName);
                //this._userAddToRole(user.Username, user.RoleName);
                var roleName = new List<KeyValuePair<int, string>>();
                //roleName = user.RolePrivileges.ToDictionary(x => x.Id);
                if (user.RolePrivileges != null && user.RolePrivileges.Count() > 0)
                {
                    foreach (var role in user.RolePrivileges)
                    {
                        //this._userAddToRole(user.Username, role.Name);
                        roleName.Add(new KeyValuePair<int, string>(role.Id, role.Name));
                    }
                }
                var profileData = new UserProfileSessionData { UserId = user.Id, Email = user.Email, Name = user.Username, RoleId = user.RoleId, RoleName = user.RoleName, RedirectUrl = user.ChangeModel, IsSuperAdmin = user.IsSuperAdmin, RolePrivilegeName = roleName, LoginId = user.UserLogin.Id };
                this.Session["LoginUser"] = profileData;
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                UserViewModel serializedModel = new UserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleName = user.RoleName,
                    IsActive = user.IsActive,
                    IsSuperAdmin = user.IsSuperAdmin,
                    LoginId = user.UserLogin.Id
                };

                string userData = serializer.Serialize(serializedModel);
                //FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                //    version: 1,
                //    name: user.Username,
                //    issueDate: DateTime.Now,
                //    expiration: DateTime.Now.AddMinutes(30),
                //    isPersistent: false,
                //    userData: userData
                //    );

                CustomPrincipal cp = new CustomPrincipal(serializedModel.Email);
                cp.Id = serializedModel.Id;
                cp.Username = serializedModel.Username;
                cp.RoleName = serializedModel.RoleName;
                cp.IsSuperAdmin = serializedModel.IsSuperAdmin;
                cp.Email = serializedModel.Email;
                cp.LoginId = serializedModel.LoginId;
                HttpContext.User = cp;
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                    1,
                    serializedModel.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(120),
                    false,
                    userData);
                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                //FormsAuthentication.SetAuthCookie(user.Username, false);
                return user.IsSuccess;
            }
            return false;
        }

        private void _userAddToRole(string Username, string RoleName)
        {
            if (!Roles.IsUserInRole(Username, RoleName))
            {
                Roles.AddUserToRole(Username, RoleName);
            }
        }

        private void _createRole(string rolename)
        {
            if (!Roles.RoleExists(rolename))
            {
                Roles.CreateRole(rolename);
            }
        }
        public ActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
        [Authorize]
        public ActionResult ChangePassword()
        {
            var viewModel = new ChangePasswordViewModel();
            var user = _userService.GetUserByName(new GetUserByNameRequest { Name = User.Identity.Name });
            viewModel.Id = user.Id;
            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                //todo call change password service
                var response = _userService.ChangePassword(new ChangePasswordRequest { Id = model.Id, Old_Password = model.Old_Password, New_Password = model.New_Password });

                //ViewBag.Message = response.Message;
                @TempData["Message"] = response.Message;
                return RedirectToAction("Validate", new { Message = response.Message });
            }
            else
            {
                ModelState.AddModelError("", "Incorrect Login Credential");
            }
            return View(model);
        }

        public ActionResult CheckPassword(ChangePasswordViewModel model)
        {
            var response = new UpdateUserResponse();
            response = _userService.CheckPassword(new CheckPasswordRequest { Name = User.Identity.Name, Password = model.Old_Password });
            if (response.IsSuccess)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Validate(string Message)
        {
            ViewBag.Message = Message;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword()
        {
            var viewModel = new ResetPasswordViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (EmailIsValid(model))
                {
                    this.ResetPasswordFactory(model);
                    return RedirectToAction("SendResetPasswordRequest", model);
                }
                else
                {
                    ViewBag.Message = "Email Address Not Found";
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid Email Address");
            }

            return View(model);
        }

        private bool EmailIsValid(ResetPasswordViewModel model)
        {
            var response = new GetUserResponse();
            response = _userService.GetUserByEmail(new GetUserRequest { Email = model.Email });
            return response.IsSuccess;
        }


        public ActionResult SendResetPasswordRequest(ResetPasswordViewModel model)
        {
            return View(model);
        }
        private void ResetPasswordFactory(ResetPasswordViewModel model)
        {
            var response = new ResetPasswordResponseViewModel();
            response = _userService.ResetPassword(new ResetPasswordRequest { Email = model.Email }).MapTo<ResetPasswordResponseViewModel>();


        }

        public ActionResult ValidateEmail(GetUserRequest request)
        {
            var response = new GetUserResponse();
            response = _userService.GetUserByEmail(new GetUserRequest { Email = request.Email });
            if (response.IsSuccess)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Reset(ResetToken request)
        {
            //Todo encode token upon link generation
            var model = new ResetPasswordChangeModel();
            var response = _userService.GetUserByToken(new ResetPasswordTokenRequest { Token = request.Token });
            if (response.IsSuccess)
            {
                model.UserId = response.Profile.Id;
                model.Token = response.Token;
            }
            else
            {
                ViewBag.Message = "Invalid Token";
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Reset(ResetPasswordChangeModel model)
        {
            return View(model);
        }
    }
}