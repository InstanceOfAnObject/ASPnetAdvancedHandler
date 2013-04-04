using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Utilities.Web.Handlers
{
	public class HttpPostAttribute : HttpVerbAttribute
	{
		public override string HttpVerb
		{
			get { return "POST"; }
		}
	}
}