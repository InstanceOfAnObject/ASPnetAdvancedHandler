using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CodeProject.GenericHandler.MVCWebTest.Controllers
{
    public class TestsController : Controller
    {
        //
        // GET: /Tests/

        public ActionResult Index()
        {
            return View();
        }

		public object SimpleMethod(int idx)
		{
			return idx;
		}

		public ActionResult SimpleTest()
		{
			return View();
		}

    }
}
