using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections;
using System.Runtime.Serialization;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	/// <summary>
	/// Provides a way of importing the jqGrig search structure into a QueryEngine Query
	/// </summary>
	[DataContract]
	public class jqGridSearchImportTemplate : BaseImportTemplate
	{
		public jqGridSearchImportTemplate()
		{ 
		
		}
		public jqGridSearchImportTemplate(string json)
		{
			jsonSearch = json;
		}

		/// <summary>
		/// Gets the jqGrid search data in jason form.
		/// </summary>
		public string jsonSearch { get; private set; }

		/// <summary>
		/// Parses the json search data into a Query object
		/// </summary>
		public override Query GetQuery
		{
			get 
			{
				Query q = new Query();

				if (!string.IsNullOrEmpty(jsonSearch))
				{
					JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
					Dictionary<string, object> jqGridFilter = jsonSerializer.Deserialize<Dictionary<string, object>>(jsonSearch);

					// evaluate group type (AND | OR)
					GroupOperatorTypes gType = jqGridFilter["groupOp"].ToString().ToUpper() == "OR" ? GroupOperatorTypes.OR : GroupOperatorTypes.AND;
					GroupOperator groupOperator = new GroupOperator(gType, null);

					// evaluate rules (the query members)
					ArrayList rules = (ArrayList)jqGridFilter["rules"];
					foreach (var rule in rules)
					{
						string field = ((Dictionary<string, object>)rule)["field"].ToString();
						string data = ((Dictionary<string, object>)rule)["data"].ToString();
						string op = ((Dictionary<string, object>)rule)["op"].ToString();
						LogicOperatorTypes logicalOperator;

						switch (op)
						{
							case "eq":
								logicalOperator = LogicOperatorTypes.Equal;
								break;
							case "ne":
								logicalOperator = LogicOperatorTypes.NotEqual;
								break;
							case "lt":
								logicalOperator = LogicOperatorTypes.Less;
								break;
							case "le":
								logicalOperator = LogicOperatorTypes.LessOrEqual;
								break;
							case "gt":
								logicalOperator = LogicOperatorTypes.Greater;
								break;
							case "ge":
								logicalOperator = LogicOperatorTypes.GreaterOrEqual;
								break;
							case "bw":
								logicalOperator = LogicOperatorTypes.BeginsWith;
								break;
							case "bn":
								logicalOperator = LogicOperatorTypes.DoesNotBeginWith;
								break;
							case "in":
								logicalOperator = LogicOperatorTypes.IsIn;
								break;
							case "ni":
								logicalOperator = LogicOperatorTypes.IsNotIn;
								break;
							case "ew":
								logicalOperator = LogicOperatorTypes.EndsWith;
								break;
							case "en":
								logicalOperator = LogicOperatorTypes.DoesNotEndWith;
								break;
							case "cn":
								logicalOperator = LogicOperatorTypes.Contains;
								break;
							case "nc":
								logicalOperator = LogicOperatorTypes.DoesNotContain;
								break;
							default:
								throw new NotImplementedException(op + " jqGrid filter operator is not supported.");
						}

						groupOperator.Operators.Add(new LogicOperator() { ColumnName = field, Operation = logicalOperator, Value = data });
					}

					q.Filter = groupOperator;
				}

				return q;
			}
		}
	}
}
