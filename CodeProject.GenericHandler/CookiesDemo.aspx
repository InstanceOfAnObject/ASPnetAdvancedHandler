<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CookiesDemo.aspx.cs" Inherits="CodeProject.GenericHandler.CookiesDemo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

	<script src="jquery.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
		<input type="button" id="btnRequestWithCookie" value="Do request" onclick="DoRequest()" />

		<script type="text/javascript">

			function DoRequest() {
				$.ajax({
					url: 'CookiesHandler.ashx',
					type: 'GET',
					data: { method: 'UseCookie', args: { cookie: 'mycookie'} },
					success: function (data) {
						alert(data);
					}
				});
			}

		</script>
    </div>
    </form>
</body>
</html>
