using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	/// <summary>
	/// Available search operations
	/// </summary>
	[DataContract]
	public enum LogicOperatorTypes
	{
		[EnumMember]
		Equal = 1,
		[EnumMember]
		NotEqual = 2,
		[EnumMember]
		Less = 3,
		[EnumMember]
		LessOrEqual = 4,
		[EnumMember]
		Greater = 5,
		[EnumMember]
		GreaterOrEqual = 6,
		[EnumMember]
		BeginsWith = 7,
		[EnumMember]
		DoesNotBeginWith = 8,
		[EnumMember]
		IsIn = 9,
		[EnumMember]
		IsNotIn = 10,
		[EnumMember]
		EndsWith = 11,
		[EnumMember]
		DoesNotEndWith = 12,
		[EnumMember]
		Contains = 13,
		[EnumMember]
		DoesNotContain = 14
	}
}
