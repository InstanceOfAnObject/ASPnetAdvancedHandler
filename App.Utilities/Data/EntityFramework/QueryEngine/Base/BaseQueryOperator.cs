using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public abstract class BaseQueryOperator
	{

		public BaseQueryOperator() { }

		/// <summary>
		/// Holds a refenrence to the parent Query object.
		/// This property is not meant to be changed from "outside"
		/// </summary>
		[DataMember]
		protected internal Query Query { get; set; }

		/// <summary>
		/// Each operator must be able to convert itself into a string based query expression.
		/// This function returns the 'WEHERE' clause to be used on a EF4 E-SQL query. 
		/// </summary>
		public abstract string GetExpression();

		/// <summary>
		/// Each operator must be able to convert itself into a string based query expression.
		/// This function returns the 'WEHERE' clause to be used on a EF4 E-SQL query. 
		/// </summary>
		/// <param name="columnNamePrefix">This prefix will be applied on all column names. Ex: A column named 'column1' and a prefix 'item' would result in 'item.column1'</param>
		/// <returns></returns>
		public abstract string GetExpression(string columnNamePrefix);

		/// <summary>
		/// Each operator must be able to convert itself into a string based query expression.
		/// This function returns the 'WHERE' clasuse to be used on a SQL Server Database query.
		/// </summary>
		/// <returns></returns>
		public abstract string GetSQLExpression();

		/// <summary>
		/// Each operator must be able to convert itself into a Linq expression.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public abstract Func<T, bool> GetLinqExpression<T>();

		/// <summary>
		/// Each operator must be able to convert itself into a string based query expression.
		/// This function returns the 'WHERE' clasuse to be used on a SQL Server Database query.
		/// </summary>
		/// <returns></returns>
		public abstract string GetSQLExpression(string columnNamePrefix);

		/// <summary>
		/// Returns the query as a Flee expression.
		/// The column names will be replaced by the actual values found on the passed object. 
		/// </summary>
		/// <param name="obj">
		/// Receives an object to be used on the property values.
		/// i.e. if a column name "Age" is specified on an operator, it will try to find a "Age" property on this object and replace the column name by that value.
		/// </param>
		/// <returns></returns>
		public abstract string GetFleeExpression(object obj);

		/// <summary>
		/// Get the Json representation of the object
		/// </summary>
		/// <returns></returns>
		public abstract string GetJson();



		public BaseQueryOperator And(string columnName, LogicOperatorTypes operation, object value)
		{
			return And(new LogicOperator(columnName, operation, value));
		}
		public BaseQueryOperator And(BaseQueryOperator operation)
		{
			if (operation == null || operation is NullOperator)
			{
				return this;
			}
			else if (this is NullOperator)
			{
				return operation;
			}
			else if (this is GroupOperator && ((GroupOperator)this).GroupType == GroupOperatorTypes.AND)
			{
				((GroupOperator)this).Operators.Add(operation);
				return this;
			}
			else
			{
				return new GroupOperator(GroupOperatorTypes.AND, this, operation);
			}
		}

		public BaseQueryOperator Or(string columnName, LogicOperatorTypes operation, object value)
		{
			return Or(new LogicOperator(columnName, operation, value));
		}
		public BaseQueryOperator Or(BaseQueryOperator operation)
		{
			if (operation == null || operation is NullOperator)
			{
				return this;
			}
			else if (this is NullOperator)
			{
				return operation;
			}
			else if (this is GroupOperator && ((GroupOperator)this).GroupType == GroupOperatorTypes.OR)
			{
				((GroupOperator)this).Operators.Add(operation);
				return this;
			}
			else
			{
				return new GroupOperator(GroupOperatorTypes.OR, this, operation);
			}
		}
	}
}
