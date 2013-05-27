using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Utilities.Web.Handlers;

namespace CodeProject.GenericHandler
{
	public class PerformanceTestHandler : BaseHandler
	{

		public object SimpleMethod(int idx)
		{
			return idx;
		}

	}
}