using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public abstract class BaseImportTemplate
	{

		public abstract Query GetQuery { get; }

	}
}
