using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Common;
using System.Data.Objects;
using App.Utilities.Data.EntityFramework;
using System.Data.Objects.DataClasses;
using System.Data;
using System.Reflection;
using App.Utilities.Data.EntityFramework.QueryEngine;
using System.Data.Linq.Mapping;

namespace App.Utilities.Data.EntityFramework
{
	public static class DefaultDataOperations
	{

		/// <summary>
		/// Defines general settings to be used on the CRUD operations.
		/// </summary>
		public static class Settings
		{ 
			public static readonly string RecordStatusColumnName = "StatusID";
			public static readonly int ActiveRecordStatusID = 2;
			public static readonly int InactiveRecordStatusID = 3;
		}

		/// <summary>
		/// Reusable Get by key.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static TEntity Get<TEntity>(this ObjectContext ctx, object key) where TEntity : class
		{
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			TEntity itemByKey = null;
			PropertyInfo keyColumnInfo = GetPrimaryKeyInfo<TEntity>(ctx);

			if (keyColumnInfo != null)
			{
				PagedList<TEntity> r = ctx.Search<TEntity>(new LogicOperator() { ColumnName = keyColumnInfo.Name, Operation = LogicOperatorTypes.Equal, Value = key });
				if (r.Items.Count == 1)
				{
					itemByKey = r.Items[0];
				}
				else if (r.Items.Count > 1)
				{
					throw new Exception("Search by key column returned more than one result!");
				}
			}
			else
			{
				throw new Exception("Entity object must have a primary key column.");
			}

			return itemByKey;
		}

		/// <summary>
		/// Returns the PropertyInfo of the entity primary key
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public static PropertyInfo GetPrimaryKeyInfo<TEntity>(this ObjectContext ctx) where TEntity : class
		{
			/*
			 *	The idea here is to loop through all the entity object properties searching for one that:
			 *		- is decorated with the EdmScalarPropertyAttribute or the ColumnAttribute
			 *		- and have the EntityKeyProperty or IsPrimaryKey value set to true
			 */

			PropertyInfo[] properties = typeof(TEntity).GetProperties();
			foreach (PropertyInfo pI in properties)
			{
				System.Object[] attributes = pI.GetCustomAttributes(true);
				foreach (object attribute in attributes)
				{
					if (attribute is EdmScalarPropertyAttribute)
					{
						if ((attribute as EdmScalarPropertyAttribute).EntityKeyProperty == true)
							return pI;
					}
					else if (attribute is ColumnAttribute)
					{
						if ((attribute as ColumnAttribute).IsPrimaryKey == true)
							return pI;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Reusable Save method for EF4.
		/// This method receives two optional anonymous methods that are executed before the database action, one for Create and another for Update.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="item"></param>
		/// <param name="onCreate"></param>
		/// <param name="onUpdate"></param>
		/// <returns></returns>
		public static TEntity Save<TEntity>(this ObjectContext ctx, TEntity item, Func<TEntity, TEntity> onCreate = null, Func<TEntity, TEntity> onUpdate = null) where TEntity : class
		{
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			System.Data.EntityKey key = (item as EntityObject).EntityKey;

			object itemByKey = null;
			//if (objset.EntitySet.ElementType.KeyMembers.Count == 1)
			//{
			PropertyInfo keyColumnInfo = GetPrimaryKeyInfo<TEntity>(ctx); // objset.EntitySet.ElementType.KeyMembers[0].Name;

			if (keyColumnInfo != null)
			{
				Type keyType = item.GetType().GetProperty(keyColumnInfo.Name).PropertyType;
				object keyValue = item.GetType().GetProperty(keyColumnInfo.Name).GetValue(item, null);

				// handle special values for guid and int value types
				// if guid value is 0000000... or int is lower or equal to 0 then this item will be considered as new and an INSERT will be done
				if (keyValue == null)
					itemByKey = null;
				else if (keyType.Equals(typeof(Guid)) && ((Guid)keyValue).ToString() == (new Guid()).ToString())
				{
					// in the case where the key column is a GUID and the item being passed have its value set to 000000000 
					//	then a new GUID value will be generated (like an auto number but for GUID)
					itemByKey = null;
					item.GetType().GetProperty(keyColumnInfo.Name).SetValue(item, Guid.NewGuid(), null);
				}
				else if (keyType.Equals(typeof(int)) && (int)keyValue <= 0)
					itemByKey = null;
				else
					itemByKey = ctx.Get<TEntity>(item.GetType().GetProperty(keyColumnInfo.Name).GetValue(item, null));
			}
			else
			{
				throw new Exception("Entity object must have a primary key column.");
			}

			if (itemByKey != null)
			{
				if(onUpdate != null) item = onUpdate(item);
				objset.ApplyCurrentValues(item);
			}
			else
			{
				if (onCreate != null) item = onCreate(item);
				objset.AddObject(item);
			}

			ctx.SaveChanges();

			try
			{
				ctx.Refresh(System.Data.Objects.RefreshMode.StoreWins, item);
			}
			catch
			{
				// if the object doesn't have an entitykey then it can't be refreshed this way
			}

			return item;
		}

		/// <summary>
		/// Reusable List method
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		public static PagedList<TEntity> List<TEntity>(this ObjectContext ctx, PagingInfo paging = null) where TEntity : class
		{
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			var list = new PagedList<TEntity>(null, paging);

			if (paging == null)
			{
				paging = PagingInfo.Default;

				var q = (from i in objset
						select i).ToList();

				paging.TotalRecords = q.Count;
				return new PagedList<TEntity>(q, paging);
			}
			else
			{
				string query = string.Format("SELECT VALUE item FROM {0}.{1} AS item", ctx.DefaultContainerName, objset.EntitySet.Name);

				if (string.IsNullOrEmpty(paging.SortFieldName))
				{
					var keyPropertyName = objset.EntitySet.ElementType.KeyMembers[0].ToString();
					paging.SortFieldName = keyPropertyName;
				}

				query += ESQLHelper.GetOrderByExpression(paging);

				ObjectQuery<TEntity> entitiesQuery = new ObjectQuery<TEntity>(query, ctx);
				return new PagedList<TEntity>(entitiesQuery.ToList(), paging);
			}
		}

		/// <summary>
		/// Generic search method that receives a list of search conditions
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="conditions"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		public static PagedList<TEntity> Search<TEntity>(this ObjectContext ctx, BaseQueryOperator condition, PagingInfo paging = null) where TEntity : class
		{
			string select = string.Empty;
			string from = string.Empty;
			string where = string.Empty;

			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			var list = new PagedList<TEntity>(null, paging);

			select = "SELECT VALUE item";
			from = string.Format(" FROM {0}.{1} AS item", ctx.DefaultContainerName, objset.EntitySet.Name);

			if (condition != null && !(condition is NullOperator))
			{
				where = " WHERE " + condition.GetExpression("item");
			}

			string query = select + from + where;
			ObjectQuery<TEntity> entitiesQuery = new ObjectQuery<TEntity>(query, ctx);
			return new PagedList<TEntity>(entitiesQuery.ToList(), paging);
		}

		/// <summary>
		/// Evaluates if records exist that satisfy the query
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static bool Exists<TEntity>(this ObjectContext ctx, Func<TEntity, bool> p) where TEntity : class
		{
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			IEnumerable<TEntity> q;

			if (p != null)
			{
				q = objset.Where<TEntity>(p);
			}
			else
			{
				q = objset;
			}

			return q.Count() > 0;
		}

		/// <summary>
		/// Generic search method using Linq to filter the results.
		/// Paging is optional.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="p"></param>
		/// <param name="paging"></param>
		/// <returns></returns>
		public static PagedList<TEntity> Search<TEntity>(this ObjectContext ctx, Func<TEntity, bool> p, PagingInfo paging = null) where TEntity : class
		{
			PagedList<TEntity> result = new PagedList<TEntity>();
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();

			var q = objset.Where<TEntity>(p);

			if (paging != null)
			{
				result.PagingInfo = paging;
				result.PagingInfo.TotalRecords = q.Count();

				q = q.OrderBy(o => o.GetType().GetProperty(paging.SortFieldName));
				q = q.Take(paging.PageSize);
				q = q.Skip((paging.PageNumber - 1) * paging.PageSize);
			}

			result.Items = q.ToList();

			return result;
		}

		/// <summary>
		/// Reusable delete method that deletes an entity.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="item"></param>
		public static void Delete<TEntity>(this ObjectContext ctx, TEntity item, bool permanently = false) where TEntity : class
		{
			if (item == null)
			{
				throw new NullReferenceException("The item to delete cannot be null.");
			}

			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			if (permanently)
			{
				objset.DeleteObject(item);
			}
			else
			{
				PropertyInfo prop = item.GetType().GetProperty(Settings.RecordStatusColumnName);
				if (prop != null)
				{
					prop.SetValue(item, Settings.InactiveRecordStatusID, null);
				}
				else
				{
					throw new Exception("This record cannot be marked as deleted because it was impossible to find the configured RecordStatusColumn [" + Settings.RecordStatusColumnName + "].");
				}
			}

			ctx.SaveChanges();
		}

		/// <summary>
		/// Resusable delete method that deletes an Entity by its key.
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="key"></param>
		public static void DeleteByKey<TEntity>(this ObjectContext ctx, object key, bool permanently = false) where TEntity : class
		{
			ObjectSet<TEntity> objset = ctx.CreateObjectSet<TEntity>();
			TEntity item = ctx.Get<TEntity>(key);
			ctx.Delete<TEntity>(item, permanently);
		}

		/// <summary>
		/// Undeletes a record. The record must support this feature by including the RecordStatusColumnName (usually named StatusID).
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="ctx"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static TEntity Undelete<TEntity>(this ObjectContext ctx, TEntity item) where TEntity : class
		{
			if (item == null)
			{
				throw new NullReferenceException("The item to undelete cannot be null.");
			}

			PropertyInfo prop = item.GetType().GetProperty(Settings.RecordStatusColumnName);
			if (prop != null)
			{
				prop.SetValue(item, Settings.ActiveRecordStatusID, null);
			}
			else
			{
				throw new Exception("This record cannot be marked as active because it was impossible to find the configured RecordStatusColumn [" + Settings.RecordStatusColumnName + "].");
			}
			ctx.SaveChanges();

			return item;
		}



		//private static EntityKey GetEntityKey<T>(ObjectSet<T> objectSet, object keyValue) where T : class
		//{
		//    var entitySetName = objectSet.Context.DefaultContainerName + "." + objectSet.EntitySet.Name;
		//    var keyPropertyName = objectSet.EntitySet.ElementType.KeyMembers[0].ToString();
		//    var entityKey = new EntityKey(entitySetName, new[] { new EntityKeyMember(keyPropertyName, keyValue) });
		//    return entityKey;
		//}

	}

}
