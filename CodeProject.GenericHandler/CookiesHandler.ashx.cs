using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Utilities.Web.Handlers;

namespace CodeProject.GenericHandler
{
	public class CookiesHandler : BaseHandler
	{

		public object UseCookie(string cookie)
		{
			return context.Request.Cookies[cookie].Value;
		}

	}
}