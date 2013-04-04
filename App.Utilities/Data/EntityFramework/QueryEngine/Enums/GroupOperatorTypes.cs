using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	[DataContract]
	public enum GroupOperatorTypes : int
	{
		[EnumMember]
		AND = 1,
		[EnumMember]
		OR = 2
	}
}
