using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CodeProject.GenericHandler
{
	public partial class CookiesDemo : System.Web.UI.Page
	{
		protected override void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);

			Response.Cookies.Add(new HttpCookie("mycookie", "AlexCode Cookie"));
		}

		protected void Page_Load(object sender, EventArgs e)
		{

		}
	}
}