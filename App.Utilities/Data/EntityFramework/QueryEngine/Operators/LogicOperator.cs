using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Linq.Expressions;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	public class LogicOperator : BaseQueryOperator
	{
		public LogicOperator() { }

		public LogicOperator(string columnName, LogicOperatorTypes operation, object value) : this() {
			this.ColumnName = columnName;
			this.Operation = operation;
			this.Value = value;
		}

		public string ColumnName { get; set; }

		public LogicOperatorTypes Operation { get; set; }

		public object Value { get; set; }

		private string ValueToQueryString
		{
			get
			{
				string value = string.Empty;

				if (Value == null)
				{
					value = "NULL";
				}
				else if (Value.GetType().Equals(typeof(string)))
				{
					ColumnInformation colInfo = Query.ColumnsInformation.Find(c => c.Name == ColumnName);
					if (colInfo != null)
					{
						switch (colInfo.Type)
						{
							case ColumnTypes.text:
								value = string.Format("\"{0}\"", Value.ToString());
								break;
							case ColumnTypes.numeric:
								value = string.Format("{0}", Value.ToString());
								break;
							case ColumnTypes.date:
								// new DateTime(2007,1,1).ToString("yyyy-MM-dd HH:MM:SS.fffffff")
								DateTime castdDate;
								if (!string.IsNullOrEmpty(colInfo.InputFormat) && colInfo.InputFormat.Trim() != string.Empty)
								{
									DateTime.TryParseExact(Value.ToString(), colInfo.InputFormat, null, DateTimeStyles.None, out castdDate);
								}
								else
								{
									if (!DateTime.TryParseExact(Value.ToString(), "yyyy-MM-dd", null, DateTimeStyles.None, out castdDate))
										if (!DateTime.TryParseExact(Value.ToString(), "yyyy/MM/dd", null, DateTimeStyles.None, out castdDate))
											if (!DateTime.TryParseExact(Value.ToString(), "yyyy.MM.dd", null, DateTimeStyles.None, out castdDate))
												throw new InvalidCastException("LogicOperator was unhable to parse the string as a Date value.");
								}
								value = string.Format("DATETIME'{0}'", new DateTime(castdDate.Year,castdDate.Month,castdDate.Day).ToString("yyyy-MM-dd HH:mm:ss.fffffff")); // string.Format("DATETIME'{0}'", "2007-01-01 00:00:00.0");//Value.ToString());
								break;
							case ColumnTypes.bit:
								throw new NotImplementedException("Bit columns types not implemented yet.");
							case ColumnTypes.guid:
								value = string.Format("Guid'{0}'", Value.ToString());
								break;
							default:
								break;
						}
					}
					else
					{
						value = string.Format("\"{0}\"", Value.ToString());
					}
				}
				else if (Value.GetType().Equals(typeof(Guid)))
				{
					value = string.Format("Guid'{0}'", Value.ToString());
				}
				else if (Value.GetType().Equals(typeof(Int16)) || Value.GetType().Equals(typeof(Int32)) || Value.GetType().Equals(typeof(Int64)))
				{
					value = string.Format("{0}", Value.ToString());
				}
				else
				{
					throw new Exception("Unsuported search value type");
				}

				return value;
			}
		}

		private string ValueToSQLQueryString
		{
			get {
				string value = string.Empty;

				if (Value == null)
				{
					value = "NULL";
				}
				else if (Value.GetType().Equals(typeof(string)))
				{
					ColumnInformation colInfo = Query.ColumnsInformation.Find(c => c.Name == ColumnName);
					if (colInfo != null)
					{
						switch (colInfo.Type)
						{
							case ColumnTypes.text:
								value = string.Format("'{0}'", Value.ToString().Replace("'", "''"));
								break;
							case ColumnTypes.numeric:
								value = string.Format("{0}", Value.ToString());
								break;
							case ColumnTypes.date:
								DateTime castdDate;
								if (!string.IsNullOrEmpty(colInfo.InputFormat) && colInfo.InputFormat.Trim() != string.Empty)
								{
									DateTime.TryParseExact(Value.ToString(), colInfo.InputFormat, null, DateTimeStyles.None, out castdDate);
								}
								else
								{
									if (!DateTime.TryParseExact(Value.ToString(), "yyyy-MM-dd", null, DateTimeStyles.None, out castdDate))
										if (!DateTime.TryParseExact(Value.ToString(), "yyyy/MM/dd", null, DateTimeStyles.None, out castdDate))
											if (!DateTime.TryParseExact(Value.ToString(), "yyyy.MM.dd", null, DateTimeStyles.None, out castdDate))
												throw new InvalidCastException("LogicOperator was unhable to parse the string as a Date value.");
								}
								value = string.Format("'{0}'", new DateTime(castdDate.Year,castdDate.Month,castdDate.Day).ToString("yyyy-MM-dd HH:mm:ss.fff")); // string.Format("DATETIME'{0}'", "2007-01-01 00:00:00.0");//Value.ToString());
								break;
							case ColumnTypes.bit:
								value = string.Format("{0}", Value.ToString());
								break;
							case ColumnTypes.guid:
								value = string.Format("'{0}'", Value.ToString());
								break;
							default:
								break;
						}
					}
					else
					{
						value = string.Format("'{0}'", Value.ToString().Replace("'", "''"));
					}
				}
				else if (Value.GetType().Equals(typeof(Guid)))
				{
					value = string.Format("'{0}'", Value.ToString());
				}
				else if (Value.GetType().Equals(typeof(Int16)) || Value.GetType().Equals(typeof(Int32)) || Value.GetType().Equals(typeof(Int64)))
				{
					value = string.Format("{0}", Value.ToString());
				}
				else if (Value.GetType().Equals(typeof(bool)))
				{
					value = string.Format("{0}", ((bool)Value ? 1 : 0).ToString());
				}
				else
				{
					throw new Exception("Unsuported search value type");
				}

				return value;
			}
		}

		public override string GetExpression()
		{
			return GetExpression(string.Empty);
		}

		public override string GetExpression(string columnNamePrefix)
		{
			// validate column name prefix value
			if (columnNamePrefix == null) columnNamePrefix = string.Empty;
			if (columnNamePrefix.EndsWith(".")) columnNamePrefix.TrimEnd('.');
			columnNamePrefix = columnNamePrefix.Trim();

			// combine the column name with the prefix
			string col = string.IsNullOrEmpty(columnNamePrefix) ? ColumnName : columnNamePrefix + "." + ColumnName;

			string expression = string.Empty;

			switch (Operation)
			{
				case LogicOperatorTypes.Equal:
					expression = col + " = " + ValueToQueryString;
					break;
				case LogicOperatorTypes.NotEqual:
					expression = col + " != " + ValueToQueryString;
					break;
				case LogicOperatorTypes.Less:
					expression = col + " < " + ValueToQueryString;
					break;
				case LogicOperatorTypes.LessOrEqual:
					expression = col + " <= " + ValueToQueryString;
					break;
				case LogicOperatorTypes.Greater:
					expression = col + " > " + ValueToQueryString;
					break;
				case LogicOperatorTypes.GreaterOrEqual:
					expression = col + " >= " + ValueToQueryString;
					break;
				case LogicOperatorTypes.Contains:
					expression = col + " LIKE " + "\"%" + ValueToQueryString.TrimEnd('"').TrimStart('"') + "%\"";
					break;
				case LogicOperatorTypes.DoesNotContain:
					expression = col + " NOT LIKE " + "\"%" + ValueToQueryString.TrimEnd('"').TrimStart('"') + "%\"";
					break;
				case LogicOperatorTypes.BeginsWith:
					expression = col + " LIKE " + ValueToQueryString.TrimEnd('"') + "%\"";
					break;
				case LogicOperatorTypes.DoesNotBeginWith:
					expression = col + " NOT LIKE " + ValueToQueryString.TrimEnd('"').TrimStart('"') + "%\"";
					break;
				case LogicOperatorTypes.EndsWith:
					expression = col + " LIKE " + "\"%" + ValueToQueryString.TrimStart('"');
					break;
				case LogicOperatorTypes.DoesNotEndWith:
					expression = col + " NOT LIKE " + "\"%" + ValueToQueryString.TrimStart('"');
					break;
				case LogicOperatorTypes.IsIn:
					// this operator implementation only supports numbers separated by comma ','
					expression = col + " IN " + "{" + ValueToQueryString + "}";
					break;
				case LogicOperatorTypes.IsNotIn:
					// this operator implementation only supports numbers separated by comma ','
					expression = col + " NOT IN " + "{" + ValueToQueryString + "}";
					break;
				default:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
			}
			return expression;
		}

		public override string GetSQLExpression()
		{
			return GetSQLExpression(string.Empty);
		}

		public override string GetSQLExpression(string columnNamePrefix)
		{
			// validate column name prefix value
			if (columnNamePrefix == null) columnNamePrefix = string.Empty;
			if (columnNamePrefix.EndsWith(".")) columnNamePrefix.TrimEnd('.');
			columnNamePrefix = columnNamePrefix.Trim();

			// combine the column name with the prefix
			string col = string.IsNullOrEmpty(columnNamePrefix) ? ColumnName : columnNamePrefix + "." + ColumnName;

			string expression = string.Empty;

			switch (Operation)
			{
				case LogicOperatorTypes.Equal:
					expression = col + " = " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.NotEqual:
					expression = col + " != " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.Less:
					expression = col + " < " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.LessOrEqual:
					expression = col + " <= " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.Greater:
					expression = col + " > " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.GreaterOrEqual:
					expression = col + " >= " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.Contains:
					var sqlValue = ValueToSQLQueryString;
					sqlValue = sqlValue.StartsWith("'") ? sqlValue.Remove(0, 1) : sqlValue;
					sqlValue = sqlValue.EndsWith("'") ? sqlValue.Remove(sqlValue.Length - 1) : sqlValue;
					expression = col + " LIKE " + "'%" + sqlValue + "%'";
					break;
				case LogicOperatorTypes.DoesNotContain:
					sqlValue = ValueToSQLQueryString;
					sqlValue = sqlValue.StartsWith("'") ? sqlValue.Remove(0, 1) : sqlValue;
					sqlValue = sqlValue.EndsWith("'") ? sqlValue.Remove(sqlValue.Length - 1) : sqlValue;
					expression = col + " NOT LIKE " + "'%" + sqlValue + "%'";
					break;
				case LogicOperatorTypes.BeginsWith:
					expression = col + " LIKE " + ValueToSQLQueryString.TrimEnd('\'') + "%'";
					break;
				case LogicOperatorTypes.DoesNotBeginWith:
					expression = col + " NOT LIKE " + ValueToSQLQueryString.TrimEnd('\'').TrimStart('\'') + "%'";
					break;
				case LogicOperatorTypes.EndsWith:
					expression = col + " LIKE " + "'%" + ValueToSQLQueryString.TrimStart('\'');
					break;
				case LogicOperatorTypes.DoesNotEndWith:
					expression = col + " NOT LIKE " + "'%" + ValueToSQLQueryString.TrimStart('\'');
					break;
				case LogicOperatorTypes.IsIn:
					// this operator implementation only supports numbers separated by comma ','
					expression = col + " IN " + "(" + ValueToSQLQueryString + ")";
					break;
				case LogicOperatorTypes.IsNotIn:
					// this operator implementation only supports numbers separated by comma ','
					expression = col + " NOT IN " + "(" + ValueToSQLQueryString + ")";
					break;
				default:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
			}
			return expression;
		}

		public override Func<T, bool> GetLinqExpression<T>()
		{
			// Get object property info
			PropertyInfo property = typeof(T).GetProperty(ColumnName);
			if (property == null)
			{
				throw new NullReferenceException("GetLinqExpression: The property " + ColumnName + " could not be found.");
			}

			// Create Expression
			var p = Expression.Parameter(typeof(T), ColumnName);
			var prop = Expression.Property(p, ColumnName);
			BinaryExpression op = null;
			switch (Operation)
			{
				case LogicOperatorTypes.Equal:
					op = Expression.Equal(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.NotEqual:
					op = Expression.NotEqual(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.Less:
					op = Expression.LessThan(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.LessOrEqual:
					op = Expression.LessThanOrEqual(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.Greater:
					op = Expression.GreaterThan(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.GreaterOrEqual:
					op = Expression.GreaterThanOrEqual(prop, Expression.Constant(ValueCLRType));
					break;
				case LogicOperatorTypes.BeginsWith:
					throw new NotSupportedException("LogicOperator does not support [BeginsWith] operation");
					break;
				case LogicOperatorTypes.DoesNotBeginWith:
					throw new NotSupportedException("LogicOperator does not support [DoesNotBeginWith] operation");
					break;
				case LogicOperatorTypes.IsIn:
					throw new NotSupportedException("LogicOperator does not support [IsIn] operation");
					break;
				case LogicOperatorTypes.IsNotIn:
					throw new NotSupportedException("LogicOperator does not support [IsNotIn] operation");
					break;
				case LogicOperatorTypes.EndsWith:
					throw new NotSupportedException("LogicOperator does not support [EndsWith] operation");
					break;
				case LogicOperatorTypes.DoesNotEndWith:
					throw new NotSupportedException("LogicOperator does not support [DoesNotEndWith] operation");
					break;
				case LogicOperatorTypes.Contains:
					throw new NotSupportedException("LogicOperator does not support [Contains] operation");
					break;
				case LogicOperatorTypes.DoesNotContain:
					throw new NotSupportedException("LogicOperator does not support [DoesNotContain] operation");
					break;
				default:
					break;
			}

			return Expression.Lambda<Func<T, bool>>(op, new ParameterExpression[] { p }).Compile();
		}

		public override string GetFleeExpression(object obj)
		{ 
			PropertyInfo p = null;
			if (obj != null)
			{
				 p = obj.GetType().GetProperty(ColumnName);
			}
			string col = p != null ? p.GetValue(obj, null).ToString() : ColumnName;

			string expression = string.Empty;

			switch (Operation)
			{
				case LogicOperatorTypes.Equal:
					expression = col + " = " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.NotEqual:
					expression = col + " <> " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.Less:
					expression = col + " < " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.LessOrEqual:
					expression = col + " <= " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.Greater:
					expression = col + " > " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.GreaterOrEqual:
					expression = col + " >= " + ValueToSQLQueryString;
					break;
				case LogicOperatorTypes.BeginsWith:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.DoesNotBeginWith:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.IsIn:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.IsNotIn:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.EndsWith:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.DoesNotEndWith:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.Contains:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				case LogicOperatorTypes.DoesNotContain:
					throw new NotImplementedException(Operation.ToString() + " operation is not implemented.");
					break;
				default:
					break;
			}

			return expression;
		}

		public override string GetJson()
		{
			string json = string.Empty;

			json += string.Format("{\"ColumnName\":\"{0}\"", ColumnName);
			json += string.Format("\"Operation\":\"{0}\"", Operation.ToString());
			json += string.Format("\"Value\":\"{0}\"}", Value);

			return json;
		}


		/// <summary>
		/// Values can arrive heve wraped in a string type
		/// This property tries to identify if what's inside the string is actually another type 
		/// </summary>
		private object ValueCLRType
		{
			get
			{
				if (Value == null)
				{
					return null;
				}
				else if (Type.Equals(Value, typeof(string)))
				{
					Decimal decimalResult = 0;
					Boolean booleanResult = false;
					Guid guidResult = Guid.Empty;
					if (Decimal.TryParse(Value.ToString(), out decimalResult))
						return decimalResult;
					else if (bool.TryParse(Value.ToString(), out booleanResult))
						return booleanResult;
					else if (Guid.TryParse(Value.ToString(), out guidResult))
						return guidResult;
					else
						return Value.ToString();
				}
				else
				{
					return Value;
				}
			}
		}
	}
}
