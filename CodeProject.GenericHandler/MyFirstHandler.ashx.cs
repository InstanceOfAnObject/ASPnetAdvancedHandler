using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App.Utilities.Web.Handlers;
using System.Text;
using CodeProject.GenericHandler.Demo;

namespace CodeProject.GenericHandler
{
	public class MyFirstHandler : BaseHandler
	{
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

		public object AJAXSendArray(int[] items, Address[] addresses)
		{
			string result = "";
			result += "Items: " + items == null ? "0" : items.Length.ToString();

			if (addresses != null)
			{
				result += "and Addresses: " + addresses == null ? "0" : addresses.Length.ToString();
			}

			return items.Length.ToString();
		}

	}
}