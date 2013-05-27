<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PerformanceTest.aspx.cs" Inherits="CodeProject.GenericHandler.PerformanceTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery.js" type="text/javascript"></script>
    <style type="text/css">
        body { font-family: Verdana, Helvetica, Arial, Times New Roman; font-size: .9em; }
        h1 { border-bottom:solid 1px #555; font-size: 1.4em; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="divSimpleTest">
		<input type="button" value="Start simple test" onclick="SimpleTest()" />
		<span id="total"></span>
		<div id="result"></div>
		<script type="text/javascript">
			var tracking = [];
			var numberOfRequests = 100;
			var finished = 0;
			var $total = $('span#total');
			tracking = [];

			var d = new Date();

			function SimpleTest() {
				tracking.push({ idx: finished, ms: new Date().getTime() });

				$.ajax({
					url: 'PerformanceTestHandler.ashx',
					type: 'GET',
					cache: false,
					data: { method: 'SimpleMethod', args: { idx: finished} },
					success: function (data) {
						tracking.push({ idx: data, ms: new Date().getTime() });
						finished++;
						$total.html(finished);

						if (finished == numberOfRequests)
							showResult();
						else
							SimpleTest();
					},
					error: function () {
						$("#divSimpleTest").append('0');
					}
				});
			};

			function showResult() {
				var $result = $('#result');
				var $list = $("<ul id='list'></ul>");
				var sum = 0;
				var count = 0;
				$.each(tracking, function (idxS, start) {
					$.each(tracking, function (idxE, end) {
						if (idxE > idxS && start.idx == end.idx) {
							var average = end.ms - start.ms;
							sum += average;
							$list.append("<li>" + average + "ms</li>");

							count++;
						}
					});
				});

				$list.append("<li>Average: " + (sum / numberOfRequests) + "ms</li>");
				$result.empty().append($list);
			}
		</script>

    </div>
    </form>
</body>
</html>
