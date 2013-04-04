using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Utilities.Web.Handlers
{
	public class HttpGetAttribute : HttpVerbAttribute
	{
		public override string HttpVerb
		{
			get { return "GET"; }
		}
	}
}