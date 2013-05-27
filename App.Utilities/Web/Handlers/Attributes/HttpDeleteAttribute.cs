using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Utilities.Web.Handlers
{
	public class HttpPutAttribute : HttpVerbAttribute
	{
		public override string HttpVerb
		{
			get { return "PUT"; }
		}
	}
}