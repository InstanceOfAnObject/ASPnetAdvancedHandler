using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Utilities.Web.Handlers;

namespace CodeProject.GenericHandler
{
	/// <summary>
	/// This handler shows how to intercept the method invocation
	/// </summary>
	public class InterceptInvoke : BaseHandler
	{

		public override void OnMethodInvoke(OnMethodInvokeArgs e)
		{
			base.OnMethodInvoke(e);

			if (context.Request.RequestType == "GET" && !e.Method.Name.ToUpper().StartsWith("GET"))
			{
				e.Cancel = true;
			}
		}

		public object GetPerson()
		{
			return new { Name = "AlexCode", Country = "Switzerland" };
		}

		public object DoSomething()
		{
			return new { Action = "I did something!" };
		}

	}
}