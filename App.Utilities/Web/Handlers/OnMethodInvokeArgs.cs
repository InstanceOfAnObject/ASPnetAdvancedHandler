using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Reflection;

namespace App.Utilities.Web.Handlers
{
	public class OnMethodInvokeArgs : CancelEventArgs
	{

		protected internal OnMethodInvokeArgs(MethodInfo method)
		{
			Method = method;
		}

		public MethodInfo Method { get; private set; }

	}
}