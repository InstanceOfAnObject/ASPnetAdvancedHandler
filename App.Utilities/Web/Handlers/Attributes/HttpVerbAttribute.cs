using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Utilities.Web.Handlers
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public abstract class HttpVerbAttribute : Attribute
	{

		public abstract string HttpVerb { get; }

	}
}