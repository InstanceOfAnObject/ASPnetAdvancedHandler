using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework
{
	/// <summary>
	/// Helper class to generate SQL.
	/// </summary>
	public static class ESQLHelper
	{

		/// <summary>
		/// Get the eSQL expression for an order by clause pased on a PagingInfo object.
		/// </summary>
		/// <param name="pafing"></param>
		/// <returns></returns>
		public static string GetOrderByExpression(PagingInfo paging, string columnNamePrefix = "")
		{
			// no fallbacks were implemented as the default values must be set before this helper runs

			if (paging.SortDirection == SortDirectionOptions.Undefined)
			{
				throw new Exception("Unhable to apply paging because no sort direction was specified was specified.");
			}
			if (string.IsNullOrEmpty(paging.SortFieldName))
			{
				throw new Exception("Unhable to apply paging because no sort column was specified.");
			}

			string sortDirection = (paging.SortDirection == SortDirectionOptions.Ascending ? " ASC" : " DESC");

			if (columnNamePrefix == null) columnNamePrefix = string.Empty;
			columnNamePrefix = columnNamePrefix.Trim();
			columnNamePrefix = columnNamePrefix.TrimEnd('.');
			columnNamePrefix += string.IsNullOrEmpty(columnNamePrefix) ? string.Empty : ".";

			return
				" order by " + columnNamePrefix + paging.SortFieldName + sortDirection +
				" SKIP(" + paging.PageSize * (paging.PageNumber - 1) + ")" +
				" LIMIT(" + paging.PageSize + ") ";
		}
	}
}
