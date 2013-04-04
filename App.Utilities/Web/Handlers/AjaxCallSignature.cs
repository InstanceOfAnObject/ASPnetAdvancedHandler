using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.ComponentModel;

namespace App.Utilities.Web.Handlers
{
	public class AjaxCallSignature
	{
		public AjaxCallSignature(HttpContext context)
		{
			args = new Dictionary<string, object>();
			method = string.Empty;
			string nullKeyParameter = context.Request.QueryString[null];
			//if (string.IsNullOrEmpty(nullKeyParameter))
			//{
			//    if (!string.IsNullOrEmpty(context.Request.QueryString.ToString()) && context.Request.QueryString.ToString().StartsWith("{"))
			//    {

			//    }
			//}

			if (context.Request.RequestType.ToUpper() == "POST")
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
					//else if (key.ToLower().StartsWith("args[") && key.ToLower().EndsWith("[]"))
					//{	
					//    // is a value array
					//    string _key = key.Trim().Substring(5).Replace("[]", "").TrimEnd(']');
					//    args.Add(_key, context.Request.QueryString[key]);
					//}
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

			
			// evaluate the handler and method attributes against Http allowed verbs
			/*
			 The logic here is:
			 *	-> if no attribute is found means it allows every verb
			 *	-> is a method have verb attbibutes defined then it will ignore the ones on the class
			 *	-> verb attributes on the class are applied to all methods without verb attribues
			 */
			var handlerSupportedVerbs = handler.GetType().GetCustomAttributes(typeof(HttpVerbAttribute), true).Cast<HttpVerbAttribute>();
			var methodSupportedVerbs = m.GetCustomAttributes(typeof(HttpVerbAttribute), true).Cast<HttpVerbAttribute>();

			bool VerbAllowedOnMethod = true;
			bool VerbAllowedOnHandler = true;
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
				a.Add(ProcessProperty(param.Name, param.ParameterType, 0));
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



		private object ProcessProperty(string propertyName, Type propertyType, int depth)
		{
			if (propertyType.IsArray)
			{
				depth++;
				return HydrateArray(propertyName, propertyType, depth);
			}
			else if (propertyType.IsClass && !propertyType.Equals(typeof(String)))
			{
				depth++;
				return HydrateClass(propertyName, propertyType, depth);
			}
			else
			{
				return HydrateValue(propertyName, propertyType);
			}
		}

		private object HydrateArray(string propertyName, Type propertyType, int depth)
		{
			return null;
		}

		/// <summary>
		/// Hydrates CLR primitive types
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public object HydrateValue(string propertyName, Type propertyType)
		{
			if (args.Keys.Contains(propertyName))
			{
				// its usual to pass an empty json string property but casting it to certain types will throw an exception
				if (string.IsNullOrEmpty(args[propertyName].ToString()) || args[propertyName].ToString() == "null" || args[propertyName].ToString() == "undefined")
				{
					// handle numerics. convert null or empty input values to 0
					if (propertyType.Equals(typeof(System.Int16)) || propertyType.Equals(typeof(System.Int32)) ||
						propertyType.Equals(typeof(System.Int64)) || propertyType.Equals(typeof(System.Decimal)) ||
						propertyType.Equals(typeof(System.Double)) || propertyType.Equals(typeof(System.Byte)))
					{
						args[propertyName] = 0;
					}
					else if (propertyType.Equals(typeof(System.Guid)))
					{
						args[propertyName] = new Guid();
					}
					else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						args[propertyName] = null;
					}
				}

				// evaluate special types that are not directly casted from string
				TypeConverter conv = TypeDescriptor.GetConverter(propertyType);
				if (args[propertyName] == null || propertyType == args[propertyName].GetType())
				{
					return args[propertyName];
				}
				else
				{
					return conv.ConvertFromInvariantString(args[propertyName].ToString());
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
		public object HydrateClass(string propertyName, Type propertyType, int depth)
		{
			var argumentObject = Activator.CreateInstance(propertyType);

			var objectProperties = args.Keys.ToList().FindAll(k => k.StartsWith(propertyName + "+"));
			foreach (var p in objectProperties)
			{
				string[] nestedProperties = p.Split('+');
				argumentObject.GetType().GetProperty(nestedProperties[depth]).SetValue(argumentObject, ProcessProperty(propertyName + "+" + nestedProperties[depth], argumentObject.GetType().GetProperty(nestedProperties[depth]).PropertyType, depth), null);

				foreach (var np in nestedProperties)
				{
					//argumentObject.GetType().GetProperty(np).SetValue(argumentObject, args[p], null);
				}
			}

			return argumentObject;
		}

	}
}
