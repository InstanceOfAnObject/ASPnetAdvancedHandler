using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Linq.Expressions;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	[DataContract]
	public class GroupOperator : BaseQueryOperator
	{

		public GroupOperator() { }
		public GroupOperator(GroupOperatorTypes type) { this.GroupType = type; }
		public GroupOperator(GroupOperatorTypes type, params BaseQueryOperator[] operators)
			: this(type)
		{
			if (operators != null)
			{
				this.Operators = new List<BaseQueryOperator>();
				foreach (var o in operators)
				{
					Operators.Add(o);
				}
			}
		}

		[DataMember]
		public GroupOperatorTypes GroupType { get; set; }

		List<BaseQueryOperator> _operators = null;
		[DataMember]
		public List<BaseQueryOperator> Operators
		{
			get
			{
				if (_operators == null)
					_operators = new List<BaseQueryOperator>();

				_operators.ForEach(o => o.Query = this.Query);

				return _operators;
			}
			set
			{
				_operators = value;
			}
		}

		private string GetGroupTypeString
		{
			get
			{
				switch (GroupType)
				{
					case GroupOperatorTypes.AND:
						return "AND";
					case GroupOperatorTypes.OR:
						return "OR";
					default:
						throw new NotImplementedException(GroupType.ToString() + " group operation is not implemented.");
				}
			}
		}

		public override string GetExpression()
		{
			return GetExpression(string.Empty);
		}

		public override string GetExpression(string columnNamePrefix)
		{
			string expression = string.Empty;

			foreach (var o in Operators)
			{
				if (!string.IsNullOrEmpty(expression))
					expression += string.Format(" {0} ", GetGroupTypeString);

				expression += o.GetExpression(columnNamePrefix);
			}

			return expression;
		}

		public override string GetSQLExpression()
		{
			return GetSQLExpression(string.Empty);
		}

		public override string GetSQLExpression(string columnNamePrefix)
		{
			string expression = string.Empty;

			foreach (var o in Operators)
			{
				if (!string.IsNullOrEmpty(expression))
					expression += string.Format(" {0} ", GetGroupTypeString);

				expression += o.GetSQLExpression(columnNamePrefix);
			}

			return "(" + expression + ")";
		}

		public override Func<T, bool> GetLinqExpression<T>()
		{
			throw new NotImplementedException("GetLinqExpression is not implemented on GroupOperator");

			//BinaryExpression binExp = null;

			//foreach (var o in Operators)
			//{
			//    if (GroupType == GroupOperatorTypes.AND)
			//    {
			//        binExp = Expression.Or(o.GetLinqExpression<T>(), null);
			//    }
			//    else
			//    {
			//        binExp = Expression.And(null, null);
			//    }
			//}

		}

		public override string GetFleeExpression(object obj)
		{
			string expression = string.Empty;

			foreach (var o in Operators)
			{
				if (!string.IsNullOrEmpty(expression))
					expression += string.Format(" {0} ", GetGroupTypeString);

				expression += o.GetFleeExpression(obj);
			}

			return "(" + expression + ")";
		}

		public override string GetJson()
		{
			string json = string.Empty;

			json += string.Format("{\"GroupType\":\"{0}\"", GroupType.ToString());
			json += "\"Operators\":[";
			foreach (var op in Operators)
			{
				json += op.GetJson() + ",";
			}
			json = json.TrimEnd(',');
			json += "]";

			return json;
		}

	}
}
