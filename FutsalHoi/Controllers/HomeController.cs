using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace FutsalHoi.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = @"Data Source = PROMETHEUS; Initial Catalog = FutsalDB; Integrated Security=True";
        public ActionResult Index()
        {
            //Session.Remove("message");
            
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ErrorAuth()
        {

            return View();
        }
    }
}