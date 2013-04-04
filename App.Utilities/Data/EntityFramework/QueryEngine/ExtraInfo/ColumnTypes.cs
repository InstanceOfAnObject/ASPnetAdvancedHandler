using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public enum ColumnTypes : int
	{
		text = 1,
		numeric = 2,
		date = 3,
		bit = 4,
		guid = 5
	}
}
