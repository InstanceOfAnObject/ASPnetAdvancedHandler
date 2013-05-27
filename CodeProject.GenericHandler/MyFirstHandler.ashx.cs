using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Utilities.Web.Handlers;
using System.Text;
using CodeProject.GenericHandler.Demo;
using System.Web.Script.Serialization;
using System.ComponentModel;

namespace CodeProject.GenericHandler
{
	public class MyFirstHandler : BaseHandler
	{
		[Description("Greets the name passed as an argument.")]
		public object GreetMe(string name) 
		{
			return string.Format("Hello {0}!", name);
		}

		public object TalkAboutMe(string name, int age) {
			return string.Format("My name is {0} and I'm {1}!", name, age);
		}

		public object SendPersonData(Person person)
		{
			return person.Address.Street;
		}

		/// <summary>
		/// Return HTML instead of JSON
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public object GiveMeSomeHTML(string text)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<head><title>My Handler!</title></head>");
			sb.Append("<body>");
			sb.Append("<p>This is a HTML page returned from the Handler</p>");
			sb.Append("<p>The text passed was: " + text + "</p>");
			sb.Append("</body>");

			context.Response.ContentType = "text/html";
			SkipContentTypeEvaluation = true;	// the handler won't try to identify the content type automatically
			SkipDefaultSerialization = true;	// the handler won't serialize the result to JSON automatically

			return sb.ToString();
		}

		public object AJAXSendIntArray(int[] items)
		{
			var jsonSer = new JavaScriptSerializer();
			string json = jsonSer.Serialize(items);

			return json;
		}

		public object AJAXSendComplextTypeArray(int[] items, Address[] addresses)
		{
			string json = string.Empty;

			var jsonSer = new JavaScriptSerializer();
			string jsonItems = jsonSer.Serialize(items);
			string jsonAddresses = jsonSer.Serialize(addresses);

			json = jsonItems + "\n" + jsonAddresses;
			return json;
		}

		[HttpGet]
		public object GetData()
		{
			return "Here's your GET response.";
		}

		[HttpPost]
		public object PostData()
		{
			return "Here's your POST response.";
		}

		[HttpPut]
		public object PutData()
		{
			return "Here's your PUT response.";
		}

		[HttpDelete]
		public object DeleteData()
		{
			return "Here's your DELETE response.";
		}

		[HttpPost]
		[HttpPut]
		public object PostOrPutData()
		{
			return "Here's your POST or PUT response.";
		}

	}
}