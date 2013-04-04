using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Utilities.Web.Handlers
{
	/// <summary>
	/// Represent the list of supported response content types
	/// </summary>
	public struct ResponseContentTypes
	{
		public const string JSON = "application/json";
		public const string XML = "application/xml";
		public const string HTML = "text/html";

		public const string Image_JPG = "image/jpeg";
		public const string Image_PNG = "image/x-png";
		public const string Image_GIF = "image/gif";
		public const string Image_BMP = "image/x-ms-bmp";

		public const string Video_MPG = "video/mpeg";
		public const string Video_MPV2 = "video/mpeg-2";
		public const string Video_MOV = "video/quicktime";
		public const string Video_AVI = "video/x-msvideo";

		public const string Application_RTF = "application/rtf";
		public const string Application_PDF = "application/pdf";
		public const string Application_MSWORD = "application/msword";
		public const string Application_MSEXCEL = "application/vnd.ms-excel";
		public const string Application_MSPOWERPOINT = "application/mspowerpoint";
		public const string Application_MSPROJECT = "application/vnd.ms-project";
		public const string Application_ZIP = "application/zip";
	}
}