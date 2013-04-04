using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Data.Objects;
using System.Data.Common;

namespace App.Utilities.Data.EntityFramework.QueryEngine
{
	[DataContract]
	public class Query
	{
		public Query() { }

		public static Query FromImportTemplate(BaseImportTemplate importTemplate)
		{
			if (importTemplate == null)
			{
				throw new NullReferenceException("NULL ImportTemplate found. To create a Query from a template an ImportTemplate must be supplied.");
			}

			return importTemplate.GetQuery;
		}

		public static Query FromJson(string query)
		{
			if (string.IsNullOrEmpty(query))
			{
				return new Query();
			}
			else
			{
				try
				{
					// {"Filter":{"GroupType":1,"Operators":[{"ColumnName":"EntityID","Operation":5,"Value":"3"}]}}

					JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
					Dictionary<string, object> obj = (Dictionary<string, object>)jsonSerializer.DeserializeObject(query);

					// fetch the expected elements
					Query q = new Query();

					/* FILTER PROPERTY */
					Dictionary<string, object> filter = obj.ContainsKey("Filter") ? (Dictionary<string, object>)obj["Filter"] : null;
					if (filter == null || filter.Count == 0)
					{
						// do nothing...
					}
					else if (filter.ContainsKey("GroupType"))
					{
						q.Filter = GetGroupOperator(filter);
					}
					else if (filter.ContainsKey("Operation"))
					{
						q.Filter = GetLogicOperator(filter);
					}
					else
					{
						throw new Exception("Error parsing json to object. Json schema not recognized.");
					}

					/* COLUMN INFORMATION */
					object[] colInfo = obj.ContainsKey("ColumnsInformation") ? (object[])obj["ColumnsInformation"] : null;
					foreach (var ci in colInfo)
					{
						Dictionary<string, object> item = (Dictionary<string, object>)ci;
						q.ColumnsInformation.Add(item["Name"].ToString(), (ColumnTypes)int.Parse(item["Type"].ToString()), item["InputFormat"] != null ? item["InputFormat"].ToString() : null);
					}

					return q;
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("[QueryFromJson] - Unhable to deserialize query.", ex);
				}
			}
		}

		private static LogicOperator GetLogicOperator(Dictionary<string, object> json)
		{
			LogicOperator op = new LogicOperator();
			op.ColumnName = json["ColumnName"].ToString();
			op.Operation = (LogicOperatorTypes)int.Parse(json["Operation"].ToString());
			op.Value = json["Value"];
			return op;
		}

		/// <summary>
		/// Gets the typed value either directly or converted into the type specified on the ExtraInformation.ColumnTypes collection.
		/// </summary>
		/// <param name="columnName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public object GetValue(string columnName, object value)
		{
			/*
			 If the value type is string then it's elegible to be converted into another type that may have been specified on the Query extra information.
			 */

			object retval = value;

			if (value.GetType() == typeof(string) && ColumnsInformation.Count > 0)
			{
				var info = ColumnsInformation.Find(c => c.Name == columnName);
				if (info != null)
				{
					retval = Convert.ChangeType(value, info.GetCLRType());
				}
			}

			return retval;
		}

		private static GroupOperator GetGroupOperator(Dictionary<string, object> json)
		{
			GroupOperator gop = new GroupOperator();
			gop.GroupType = (GroupOperatorTypes)int.Parse(json["GroupType"].ToString());

			object[] operators = (object[])json["Operators"];

			foreach (var item in operators)
			{
				Dictionary<string, object> jsonOp = (Dictionary<string, object>)item;

				if (jsonOp.ContainsKey("GroupType"))
				{
					gop.Operators.Add(GetGroupOperator(jsonOp));
				}
				else if (jsonOp.ContainsKey("Operation"))
				{
					gop.Operators.Add(GetLogicOperator(jsonOp));
				}
			}

			return gop;
		}

		BaseQueryOperator _filter = null;
		/// <summary>
		/// defines the query structure
		/// </summary>
		public BaseQueryOperator Filter
		{
			get
			{
				if (_filter == null)
				{
					_filter = new NullOperator();
				}
				
				_filter.Query = this;
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}

		ColumnInformationCollection _columnsInformation = null;
		/// <summary>
		/// Gets or set extra information about the column types.
		/// This is useful when the query was built from json and all the values were passed as string regardless their real type.
		/// </summary>
		public ColumnInformationCollection ColumnsInformation
		{
			get
			{
				if (_columnsInformation == null)
					_columnsInformation = new ColumnInformationCollection();

				return _columnsInformation;
			}
			set
			{
				_columnsInformation = value;
			}
		}

		public string ToJson()
		{
			JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
			StringBuilder sb = new StringBuilder();
			jsonSerializer.Serialize(this, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Gets a query string that returns all rows that match the filter conditions.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		private string GetSelectExpression(ObjectQuery obj)
		{
			string strQuery = string.Format("SELECT VALUE item FROM {0} as item", obj.CommandText);

			if (Filter != null)
			{
				string whereExpression = Filter.GetExpression("item");
				if (!string.IsNullOrEmpty(whereExpression.Trim()))
				{
					strQuery += " WHERE " + whereExpression;
				}
			}

			return strQuery;
		}

		/// <summary>
		/// Gets a query string that returns a paged result of the rows that match the filter conditions.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		private string GetSelectExpressionWithPaging(ObjectQuery obj, PagingInfo paging)
		{
			string strQuery = GetSelectExpression(obj);

			if (paging != null)
			{
				if (string.IsNullOrEmpty(paging.SortFieldName))
				{
					var keys = ((System.Data.Metadata.Edm.EntitySet)obj.GetType().GetProperty("EntitySet").GetValue(obj, null)).ElementType.KeyMembers;
					if (keys != null && keys.Count > 0)
					{
						paging.SortFieldName = "item." + keys[0].Name;
					}
					else
					{
						throw new Exception("Unhable to apply paging because no SortFieldName value was specified and no primary key column was found.");
					}
				}
				else
					paging.SortFieldName = "item." + paging.SortFieldName;

				if (paging.PageSize == -1)
					paging.PageSize = int.MaxValue;

				if (paging.PageNumber == -1)
					paging.PageNumber = 1;

				strQuery += ESQLHelper.GetOrderByExpression(paging);
			}

			return strQuery;
		}

		/// <summary>
		/// Executes the query and returns a PAgedList object.
		/// Be aware that this always returs all columns of the entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		public PagedList<T> ExecuteQuery<T>(ObjectQuery<T> obj, PagingInfo paging)
		{
			if (paging == null)
				paging = PagingInfo.Default;

			ObjectQuery<T> entitiesQueryCount = new ObjectQuery<T>(GetSelectExpression(obj), obj.Context);
			var totalCount = entitiesQueryCount.ToList().Count();
			paging.TotalRecords = totalCount;

			ObjectQuery<T> entitiesQuery = new ObjectQuery<T>(GetSelectExpressionWithPaging(obj, paging), obj.Context);

			PagedList<T> result = new PagedList<T>(entitiesQuery.ToList(), paging);

			return result;
		}






		/*
		 *	TRICK TO ALLOW QUERY WCF SERIALIZATION
		 * 
		 *	This object is quite complex and fails to serialize to XML when passed to a WCF method but successfully serializes to JSON using the default JavaScriptSerializer
		 *	To overcome this quickly I've created this property that is the only thing serialized when the Query object passes through a WCF service.
		 *	
		 *	The trick consists in serializing the object to JSON (this property Get) and send it through the wire. 
		 *	Reaching there the deserialization process will set the value to this property that will deserialize the JSON and inflate the Query object :)
		 *	
		 * Note that this property is Private for two reasons:
		 *		-> The ToJson() method uses JavaScriptSerializer to get the JSON representation of this object.
		 *		   If this property is public it will try to serialize it too which will cause a stack overflow exception (endless loop)
		 *		  
		 *		-> This method shouldn't be available on the intelisense for other purposes other than this.
		 */
		[DataMember]
		private string _WCFSerialization
		{
			get {
				return this.ToJson();
			}
			set {
				var q = FromJson(value);

				this.Filter = q.Filter;
				this.ColumnsInformation = q.ColumnsInformation;
			}
		}

	}
}
