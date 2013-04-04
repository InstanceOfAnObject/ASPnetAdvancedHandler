using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;

namespace App.Utilities.Data.EntityFramework
{
	public static class DbDataRecordExtensions
	{

		/// <summary>
		/// Converts a single DbDataRwcord object into something else.
		/// The destination type must have a default constructor.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="record"></param>
		/// <returns></returns>
		public static T ConvertTo<T>(this DbDataRecord record)
		{
			T item = Activator.CreateInstance<T>();
			for (int f = 0; f < record.FieldCount; f++)
			{
				PropertyInfo p = item.GetType().GetProperty(record.GetName(f));
				if (p != null && p.PropertyType == record.GetFieldType(f))
				{
					p.SetValue(item, record.GetValue(f), null);
				}
			}

			return item;
		}

		/// <summary>
		/// Converts a list of DbDataRecord to a list of something else.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static PagedList<T> ConvertTo<T>(this PagedList<DbDataRecord> list)
		{
			PagedList<T> result = (PagedList<T>)Activator.CreateInstance<PagedList<T>>();

			list.Items.ForEach(rec => 
				{
					result.Items.Add(rec.ConvertTo<T>());
				});

			return result;
		}

	}
}
