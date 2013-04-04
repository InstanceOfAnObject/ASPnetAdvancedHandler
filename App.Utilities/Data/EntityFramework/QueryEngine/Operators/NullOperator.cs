using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public class NullOperator : BaseQueryOperator
	{
		public override string GetExpression()
		{
			return string.Empty;
		}

		public override string GetExpression(string columnNamePrefix)
		{
			return string.Empty;
		}

		public override string GetSQLExpression()
		{
			return string.Empty;
		}

		public override string GetSQLExpression(string columnNamePrefix)
		{
			return string.Empty;
		}

		public override string GetFleeExpression(object obj)
		{
			return string.Empty;
		}

		public override Func<T, bool> GetLinqExpression<T>()
		{
			return null;
		}

		public override string GetJson()
		{
			return "{}";
		}
	}
}
