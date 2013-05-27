using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace App.Utilities.Web.Handlers
{
	public class AjaxCallSignature
	{
		public AjaxCallSignature(HttpContext context)
		{
			args = new Dictionary<string, object>();
			method = string.Empty;
			string nullKeyParameter = context.Request.QueryString[null];

			if (new List<string>() { "POST", "PUT", "DELETE" }.Contains(context.Request.RequestType.ToUpper()))
			{
				string[] requestParams = context.Request.Params.AllKeys;
				foreach (var item in requestParams)
				{
					if (item.ToLower() == "method")
					{
						method = context.Request.Params[item];
					}
					else if (item.ToLower().StartsWith("args["))
					{
						string key = item.Trim().TrimEnd(']').Substring(5);
						key = key.Trim().Replace("][", "+");

						string value = context.Request.Params[item];
						args.Add(key, value);
					}
					else
					{
						string key = item;
						string value = context.Request.Params[item];
						args.Add(key, value);
					}
				}
			}
			else if (context.Request.RequestType.ToUpper() == "GET")
			{
				// evaluate the data passed as json
				if (!string.IsNullOrEmpty(nullKeyParameter))
				{
					if (nullKeyParameter.ToLower() == "help")
					{
						method = "help";
						return;
					}
					else
					{
						object json = null;
						JavaScriptSerializer serializer = new JavaScriptSerializer();
						json = serializer.DeserializeObject(context.Request.QueryString[null]);

						try
						{
							Dictionary<string, Object> dict = (Dictionary<string, Object>)json;

							if (dict.ContainsKey("method"))
								method = dict["method"].ToString();
							else
								throw new Exception("Invalid BaseHandler call. MethodName parameter is mandatory in json object.");

							if (dict.ContainsKey("returntype"))
								returnType = dict["returntype"].ToString();

							if (dict.ContainsKey("args"))
								args = (Dictionary<string, Object>)dict["args"];
							else
								args = new Dictionary<string, object>();
						}
						catch
						{
							throw new InvalidCastException("Unable to cast json object to AjaxCallSignature");
						}
					}
				}

				// evaluate data passed as querystring params
				foreach (string key in context.Request.QueryString.Keys)
				{
					if (key == null)
						continue;

					if (key.ToLower() == "method")
					{
						if (string.IsNullOrEmpty(method))
						{
							method = context.Request.QueryString[key];
						}
						else
						{
							throw new Exception("Method name was already specified on the json data. Specify the method name only once, either on QueryString params or on the json data.");
						}
					}
					else if (key.ToLower() == "returntype")
					{
						returnType = context.Request.QueryString[key];
					}
					else if (key.ToLower().StartsWith("args["))
					{
						string _key = key.Trim().Substring(5).TrimEnd(']').Replace("][", "+");
						args.Add(_key, context.Request.QueryString[key]);
					}
					else
					{
						args.Add(key, context.Request.QueryString[key]);
					}
				}
			}
		}

		public string method { get; set; }
		public string returnType { get; set; }
		public Dictionary<string, object> args { get; set; }

		public object Invoke(BaseHandler handler, HttpContext context)
		{
			MethodInfo m = null;
			if (string.IsNullOrEmpty(method))
			{
				// call the request method
				// if no method is passed then well call the method by HTTP verb (GET, POST, DELETE, UPDATE)
				method = context.Request.RequestType.ToUpper();
			}
			m = handler.GetType().GetMethod(method);

			List<object> a = new List<object>();

			if (m == null)
			{
				if (method.ToLower() == "help")
				{
					m = handler.GetType().BaseType.GetMethod("Help");
				}
				else
				{
					throw new Exception(string.Format("Method {0} not found on Handler {1}.", method, this.GetType().ToString()));
				}
			}
			else
			{
				// evaluate the handler and method attributes against Http allowed verbs
				/*
				 The logic here is:
				 *	-> if no attribute is found means it allows every verb
				 *	-> is a method have verb attbibutes defined then it will ignore the ones on the class
				 *	-> verb attributes on the class are applied to all methods without verb attribues
				 */
				var handlerSupportedVerbs = handler.GetType().GetCustomAttributes(typeof(HttpVerbAttribute), true).Cast<HttpVerbAttribute>();
				var methodSupportedVerbs = m.GetCustomAttributes(typeof(HttpVerbAttribute), true).Cast<HttpVerbAttribute>();

				bool VerbAllowedOnMethod = (methodSupportedVerbs.Count() == 0);
				bool VerbAllowedOnHandler = (handlerSupportedVerbs.Count() == 0);
				if (methodSupportedVerbs.Count() > 0)
				{
					VerbAllowedOnMethod = methodSupportedVerbs.FirstOrDefault(x => x.HttpVerb == context.Request.RequestType.ToUpper()) != null;
				}
				else if (handlerSupportedVerbs.Count() > 0)
				{
					VerbAllowedOnHandler = handlerSupportedVerbs.FirstOrDefault(x => x.HttpVerb == context.Request.RequestType.ToUpper()) != null;
				}

				if (!VerbAllowedOnMethod || !VerbAllowedOnHandler)
				{
					throw new HttpVerbNotAllowedException();
				}

				// security validation: Search for RequireAuthenticationAttribute on the method
				//		value=true the user must be authenticated (only supports FromsAuthentication for now
				//		value=false invoke the method
				object[] attrs = m.GetCustomAttributes(typeof(RequireAuthenticationAttribute), true);
				if (attrs != null && attrs.Length > 0)
				{

					if (!context.Request.IsAuthenticated && ((RequireAuthenticationAttribute)attrs[0]).RequireAuthentication)
					{
						throw new InvalidOperationException("Method [" + m.Name + "] Requires authentication");
					}
				}
			}

			foreach (var param in m.GetParameters())
			{
				a.Add(ProcessProperty(param.Name, param.ParameterType, string.Empty));
			}

			// OnMethodInvoke -> Invoke -> AfterMethodInvoke
			OnMethodInvokeArgs cancelInvoke = new OnMethodInvokeArgs(m);
			handler.OnMethodInvoke(cancelInvoke);

			object invokeResult = null;
			if (!cancelInvoke.Cancel)
			{
				invokeResult = m.Invoke(handler, a.ToArray());
				handler.AfterMethodInvoke(invokeResult);
			}

			return invokeResult;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyType"></param>
		/// <param name="parentNamespace">represents the full path of the parent node of the input parameter</param>
		/// <returns></returns>
		private object ProcessProperty(string propertyName, Type propertyType, string parentNamespace)
		{
			if (propertyType.IsArray || (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>)))
			{
				return HydrateArray(propertyName, propertyType, parentNamespace);
			}
			else if (propertyType.IsClass && !propertyType.Equals(typeof(String)))
			{
				return HydrateClass(propertyName, propertyType, parentNamespace);
			}
			else
			{
				return HydrateValue(propertyName, propertyType, parentNamespace);
			}
		}

		private object HydrateArray(string propertyName, Type propertyType, string parentNamespace)
		{
			Array result = null;
			Type elementType;
			
			if (propertyType.IsGenericType)
				elementType = propertyType.GetGenericArguments()[0];
			else
				elementType = propertyType.GetElementType();

			String propFQN = String.IsNullOrEmpty(parentNamespace) ? propertyName : parentNamespace + "+" + propertyName;

			if (elementType.IsValueType)
			{
			    TypeConverter conv = TypeDescriptor.GetConverter(elementType);
			    string[] values = args[propFQN + "+"].ToString().Split(new char[] { ',' });

				result = Array.CreateInstance(elementType, values.Length);

			    for (int i = 0; i < values.Length; i++)
			    {
			        result.SetValue(conv.ConvertFromString(values[i]), i);
			    }
			}
			else
			{
				// get the properties in the current nesting depth
				var objectProperties = args.Keys.ToList().FindAll(k => k.StartsWith(propFQN + "+"));

				// get the number of items in the array
				int max_index = 0;
				foreach (var p in objectProperties)
				{
					string idx = p.Remove(0, propFQN.Length + 1);
					idx = idx.Substring(0, idx.IndexOf('+'));
					int i = Convert.ToInt32(idx);
					if (i > max_index) max_index = i;
				}

				// create the instance of the array
				result = Array.CreateInstance(elementType, max_index + 1);
				for (int i = 0; i <= max_index; i++)
				{
					String nsPrefix = String.Format("{0}+{1}", propFQN, i.ToString());
					result.SetValue(ProcessProperty(propertyName + "+" + i.ToString(), result.GetType().GetElementType(), parentNamespace), i);
				}
			}

			if (propertyType.IsGenericType) 
			{
				return Activator.CreateInstance(propertyType, new object[] { result });
			}
 			else
				return result;
		}

		/// <summary>
		/// Hydrates CLR primitive types
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public object HydrateValue(string propertyName, Type propertyType, String parentNamespace)
		{
			String propFQN = string.IsNullOrEmpty(parentNamespace) ? propertyName : parentNamespace + "+" + propertyName;
			if (args.Keys.Contains(propFQN))
			{
				// its usual to pass an empty json string property but casting it to certain types will throw an exception
				if (string.IsNullOrEmpty(args[propFQN].ToString()) || args[propFQN].ToString() == "null" || args[propFQN].ToString() == "undefined")
				{
					// handle numerics. convert null or empty input values to 0
					if (propertyType.Equals(typeof(System.Int16)) || propertyType.Equals(typeof(System.Int32)) ||
						propertyType.Equals(typeof(System.Int64)) || propertyType.Equals(typeof(System.Decimal)) ||
						propertyType.Equals(typeof(System.Double)) || propertyType.Equals(typeof(System.Byte)))
					{
						args[propFQN] = 0;
					}
					else if (propertyType.Equals(typeof(System.Guid)))
					{
						args[propFQN] = new Guid();
					}
					else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						args[propFQN] = null;
					}
				}

				// evaluate special types that are not directly casted from string
				TypeConverter conv = TypeDescriptor.GetConverter(propertyType);
				if (args[propFQN] == null || propertyType == args[propFQN].GetType())
				{
					return args[propFQN];
				}
				else
				{
					return conv.ConvertFromInvariantString(args[propFQN].ToString());
				}
			}
			else
			{
				return null;	// if there are missing arguments try passing null
			}
		}

		/// <summary>
		/// Hydrates complex types
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public object HydrateClass(string propertyName, Type propertyType, string parentNamespace)
		{
			var argumentObject = Activator.CreateInstance(propertyType);

			// search for properties on the current namespace
			string nsPrefix = string.IsNullOrEmpty(parentNamespace) ? propertyName : parentNamespace + "+" + propertyName;

			var objectProperties = args.Keys.ToList().FindAll(k => k.StartsWith(nsPrefix));

			// loop through them 
			foreach (var p in objectProperties)
			{
				String propName = p.Remove(0, nsPrefix.Length + 1).Split('+')[0];

				argumentObject.GetType()
					.GetProperty(propName)
						.SetValue(argumentObject, ProcessProperty(propName, argumentObject.GetType().GetProperty(propName).PropertyType, nsPrefix), null);
			}

			return argumentObject;
		}

	}
}
