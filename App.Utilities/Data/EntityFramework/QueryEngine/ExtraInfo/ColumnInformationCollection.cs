using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public class ColumnInformationCollection : List<ColumnInformation>
	{

		public ColumnInformationCollection() { }

		public void Add(string name, ColumnTypes type, string inputFormat = null) 
		{
			var colInfo = new ColumnInformation() { Name = name, Type = type, InputFormat = inputFormat };
			this.Add(colInfo);
		}

	}
}
