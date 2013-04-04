using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{

	/// <summary>
	/// Provides extra information about the query.
	/// </summary>
	public class ColumnInformation
	{

		/// <summary>
		/// Gets or sets the column name to which we want to specify the value type
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the standard value type we'll be seraching for.
		/// By default everything is assumed to be a string, so use this if you need to search for numbers, dates, guid...
		/// </summary>
		public ColumnTypes Type { get; set; }

		/// <summary>
		/// Specifies, when necessary, the input format of the value we're searching for.
		/// This is very important when we're searching for DateTime or Numeric values and their format are not ISO.
		/// A valid value here could be the following for a PT date format: dd-MM-yyyy
		/// </summary>
		public string InputFormat { get; set; }

		/// <summary>
		/// Gets the CLR type that corresponds to the Type defined for this column
		/// </summary>
		/// <returns></returns>
		public Type GetCLRType()
		{
			switch (Type)
			{
				case ColumnTypes.text:
					return typeof(String);
				case ColumnTypes.numeric:
					return typeof(Decimal);
				case ColumnTypes.date:
					return typeof(DateTime);
				case ColumnTypes.bit:
					return typeof(Boolean);
				default:
					throw new InvalidCastException(string.Format("Defined type [{0}] not supported.", Type.ToString()));
			}
		}

	}
}
