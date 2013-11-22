using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Accela.Shared.AuthLibrary;
using System.Configuration;
using AuthLibrarySampleApp.Models;

namespace AuthLibrarySampleApp.Controllers
{
    public class HomeController : Controller
    {
            ApplicationInfo applicationInfo;

            public HomeController()
            {
                applicationInfo = new ApplicationInfo
                {
                    applicationId = ConfigurationManager.AppSettings["ApplicationId"],
                    applicationSecret = ConfigurationManager.AppSettings["ApplicationSecret"],
                    applicationType = ConfigurationManager.AppSettings["ApplicationType"]
                };
            }

            [HttpPost]
            public ActionResult Login(Agency agency)
            {
                try
                {
                    applicationInfo.agencyName = agency.agencyName;
                    applicationInfo.agencyEnvironment = agency.agencyEnv;
                    AuthLibrary.Login(applicationInfo, ConfigurationManager.AppSettings["RedirectUrl"], ConfigurationManager.AppSettings["Scope"]);
                    return new EmptyResult();
                }
                catch (Exception e)
                {
                    // display error
                    ViewBag.Message = e.Message;
                    return View("Index");
                }
            }

            public ActionResult Index()
            {
                try
                {
                    CurrentUserProfile userProfile = AuthLibrary.GetCurrentUserProfile(ConfigurationManager.AppSettings["RedirectUrl"], applicationInfo);
                    if (userProfile != null)
                    {
                        // good to do something here and take to next page
                        return View("Authenticated", userProfile);
                    }
                    else
                        return View("Index");
                }
                catch (Exception e)
                {
                    // display error
                    ViewBag.Message = e.Message;
                    return View("Index");
                }
            }
    }
}
