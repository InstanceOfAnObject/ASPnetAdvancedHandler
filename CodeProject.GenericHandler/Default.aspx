<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CodeProject.GenericHandler.Default" %>

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
    <div>
        <h1>Calling Handler Through URL</h1>
        <p>
        <a href="MyFirstHandler.ashx?help">Show Handler Help</a>
        </p>

        <p>
        <a href="MyFirstHandler.ashx?method=GreetMe&name=AlexCode">Greet me!</a>
        </p>
        
        <p>
        Greet this:<input type="text" id="txtGreetName" /><input type="button" value="Go" onclick="javascript:window.location.href='MyFirstHandler.ashx?method=GreetMe&name=' + document.getElementById('txtGreetName').value" />
        </p>

        <h1>Calling Handler using JQuery $.ajax</h1>
        <p>
        <input type="button" onclick="AJAXGreet()" value="Request using $.ajax" />
        </p>

        <p>
        Name:<input type="text" id="txtAjaxGreet" /><input type="button" onclick="AJAXGreet($('#txtAjaxGreet').val())" value="Request using $.ajax" />
        </p>

        <script type="text/javascript">
            function AJAXGreet(inputText) {
                if (inputText == undefined || inputText == null || inputText == '') {
                    inputText = "AJAX Request";
                }
                /*
                Using $.ajax to perform an AJAX request to our handler is pretty easy.
                In fact, is everything similar to any other $.ajax request, 
                    you just need to structure the way you pass the data to the hadler like:

                    data: { method: 'YOUR METHOD NAME', args: { param1:value1, param2:value2,... } }

                    Be aware that everything is case sensitive!!
                */
                $.ajax({
                    url: 'MyFirstHandler.ashx',
                    type: 'GET',
                    data: { method: 'GreetMe', args: { name: inputText} },
                    success: function (data) {
                        alert(data);
                    }
                });
            }
        </script>
        


        <h1>Support multiple tipes of input arguments</h1>
        <p>
        Name:<input type="text" id="txtAdvArgsName" /><br />
        Age: <input type="text" id="txtAdvArgsAge" /><br />
        <input type="button" onclick="AJAXTalkAboutMe()" value="Request using $.ajax" />
        </p>

        <script type="text/javascript">
            function AJAXTalkAboutMe() {
                var myName = $('#txtAdvArgsName').val();
                var myAge = $('#txtAdvArgsAge').val();
                /*
                Using $.ajax to perform an AJAX request to our handler is pretty easy.
                In fact, is everything similar to any other $.ajax request, 
                you just need to structure the way you pass the data to the hadler like:

                data: { method: 'YOUR METHOD NAME', args: { param1:value1, param2:value2,... } }

                Be aware that everything is case sensitive!!
                */
                $.ajax({
                    url: 'MyFirstHandler.ashx',
                    type: 'POST',   // I'm doing a POST here just to show it can handle it too... 
                    data: { method: 'TalkAboutMe', args: { name: myName, age: myAge} },
                    success: function (data) {
                        alert(data);
                    }
                });
			}

			
        </script>


        <h1>Handling Complex Types as an argument</h1>
        <p>
		Here I'm showing how to pass a complex type as an argument.<br />
		Basically this is a Person class that have a nested class Address that in turn also have a nested class PostalCode.<br />
		Here I'm doing a POST but as always it can be either a POST or a GET
        </p>
		<input type="button" onclick="AJAXSendComplexType()" value="Send person data" />

        <script type="text/javascript">
        	function AJAXSendComplexType() {
        		var person = {
        			Name: 'Alex',
        			Age: 34,
        			Address: {
        				Street: 'A street somewhere',
        				DoorNumber: '123A',
        				PostalCode: {
        					Code: '555-1234',
        					City: 'Geneva',
        					Country: 'Switzerland'
        				}
        			}
        		};

        		$.ajax({
        			url: 'MyFirstHandler.ashx',
        			type: 'POST',
        			data: {
        				method: 'SendPersonData',
        				args: { person: person }
        			},
        			success: function (data) {
        				alert(data);
        			}
        		});
        	}
			
        </script>



        <h1>Handling Arrays</h1>
        <p>
		Here I'm showing how to pass arrays as an argument.<br />
        </p>
		<input type="button" onclick="AJAXSendIntArray()" value="Send Int array" />
		<script type="text/javascript">
			function AJAXSendIntArray() {
				$.ajax({
					url: 'MyFirstHandler.ashx',
					type: 'GET',   // I'm doing a POST here just to show it can handle it too... 
					data: { method: 'AJAXSendIntArray',
						args: {
							items: [1, 2, 3]
						}
					},
					success: function (data) {
						alert(data);
					},
					error: function (a, b, c) {
						alert('error');
					}
				});
			}
		</script>


		<input type="button" onclick="AJAXSendComplextTypeArray()" value="Send complex type data with nested collections" />

		<script type="text/javascript">
			function AJAXSendComplextTypeArray() {
				$.ajax({
					url: 'MyFirstHandler.ashx',
					type: 'GET', 
					data: { method: 'AJAXSendComplextTypeArray',
						args: {
							items: [1, 2, 3],
							addresses: [
								{ Street: 'Avenue A', DoorNumber: '123', PostalCode: { Code: '2910', City: 'Setubal'} },
								{ Street: 'Avenue B', DoorNumber: '45',
									PostalCode: {
										Code: '666',
										City: 'The Deep City',
										Attributes: [
											{ Name: 'attr1', Value: 'some value 1' },
											{ Name: 'attr2', Value: 'some value 2' }
										]
									}
								}
							]
						}
					},
					success: function (data) {
						alert(data);
					},
					error: function (a, b, c) {
						alert('error');
					}
				});
			}
		</script>



		<h1>Returning HTML</h1>
        <p>
			You can use this to render HTML also!
			<a href="MyFirstHandler.ashx?method=GiveMeSomeHTML&text=DummyImput">Click Here</a>
		</p>

		<h1>Filter your methods by Verb</h1>
		<p>
			Here we'll perform calls to methods that only support a certain HTTP verb.<br />
			By default, both handler and methods support all HTTP verbs.<br />
			To specify which verb you want to support by method, just decorate it with the right attribute.<br />
			This can also be done at the Controller level.<br />
			Verbs showing in <span style="color:#0f0;">GREEN</span> represent successful request.<br />
			Verbs showing in <span style="color:#f00;">RED</span> represent denied access request.
		</p>
        <p id="pHttpGETOnlyTest">
			<input type="button" onclick="ExecuteHttpGETOnlyTest()" value="HTTP GET only method" />
			<script type="text/javascript">
				function ExecuteHttpGETOnlyTest() {
					ExecuteHttpGETOnlyTest_Verb('GET');
					ExecuteHttpGETOnlyTest_Verb('POST');
					ExecuteHttpGETOnlyTest_Verb('PUT');
					ExecuteHttpGETOnlyTest_Verb('DELETE');
				};

				function ExecuteHttpGETOnlyTest_Verb(verb) {
					$('#pHttpGETOnlyTest span').remove();

					$.ajax({
						url: 'MyFirstHandler.ashx',
						type: verb,
						data: { method: 'GetData' },
						success: function (data) { $('#pHttpGETOnlyTest').append('<span style="color:#0f0;padding:0 5px;">' + verb + '</span>'); },
						error: function () { $('#pHttpGETOnlyTest').append('<span style="color:#f00;padding:0 5px;">' + verb + '</span>'); }
					});
				};
			</script>
		</p>

        <p id="pHttpPOSTOnlyTest">
			<input type="button" onclick="ExecuteHttpPOSTOnlyTest()" value="HTTP POST only method" />
			<script type="text/javascript">
				function ExecuteHttpPOSTOnlyTest() {
					ExecuteHttpPOSTOnlyTest_Verb('GET');
					ExecuteHttpPOSTOnlyTest_Verb('POST');
					ExecuteHttpPOSTOnlyTest_Verb('PUT');
					ExecuteHttpPOSTOnlyTest_Verb('DELETE');
				};

				function ExecuteHttpPOSTOnlyTest_Verb(verb) {
					$('#pHttpPOSTOnlyTest span').remove();

					$.ajax({
						url: 'MyFirstHandler.ashx',
						type: verb,
						data: { method: 'PostData' },
						success: function (data) { $('#pHttpPOSTOnlyTest').append('<span style="color:#0f0;padding:0 5px;">' + verb + '</span>'); },
						error: function () { $('#pHttpPOSTOnlyTest').append('<span style="color:#f00;padding:0 5px;">' + verb + '</span>'); }
					});
				};
			</script>
		</p>

		<p id="pHttpPUTOnlyTest">
			<input type="button" onclick="ExecuteHttpPUTOnlyTest()" value="HTTP PUT only method" />
			<script type="text/javascript">
				function ExecuteHttpPUTOnlyTest() {
					ExecuteHttpPUTOnlyTest_Verb('GET');
					ExecuteHttpPUTOnlyTest_Verb('POST');
					ExecuteHttpPUTOnlyTest_Verb('PUT');
					ExecuteHttpPUTOnlyTest_Verb('DELETE');
				};

				function ExecuteHttpPUTOnlyTest_Verb(verb) {
					$('#pHttpPUTOnlyTest span').remove();

					$.ajax({
						url: 'MyFirstHandler.ashx',
						type: verb,
						data: { method: 'PutData' },
						success: function (data) { $('#pHttpPUTOnlyTest').append('<span style="color:#0f0;padding:0 5px;">' + verb + '</span>'); },
						error: function () { $('#pHttpPUTOnlyTest').append('<span style="color:#f00;padding:0 5px;">' + verb + '</span>'); }
					});
				};
			</script>
		</p>

		<p id="pHttpDELETEOnlyTest">
			<input type="button" onclick="ExecuteHttpDELETEOnlyTest()" value="HTTP DELETE only method" />
			<script type="text/javascript">
				function ExecuteHttpDELETEOnlyTest() {
					ExecuteHttpDELETEOnlyTest_Verb('GET');
					ExecuteHttpDELETEOnlyTest_Verb('POST');
					ExecuteHttpDELETEOnlyTest_Verb('PUT');
					ExecuteHttpDELETEOnlyTest_Verb('DELETE');
				};

				function ExecuteHttpDELETEOnlyTest_Verb(verb) {
					$('#pHttpDELETEOnlyTest span').remove();

					$.ajax({
						url: 'MyFirstHandler.ashx',
						type: verb,
						data: { method: 'DeleteData' },
						success: function (data) { $('#pHttpDELETEOnlyTest').append('<span style="color:#0f0;padding:0 5px;">' + verb + '</span>'); },
						error: function () { $('#pHttpDELETEOnlyTest').append('<span style="color:#f00;padding:0 5px;">' + verb + '</span>'); }
					});
				};
			</script>
		</p>

		<p id="pHttpPostOrPutOnlyTest">
			<input type="button" onclick="ExecuteHttpPostOrPutOnlyTest()" value="HTTP Post or Put only method" />
			<script type="text/javascript">
				function ExecuteHttpPostOrPutOnlyTest() {
					ExecuteHttpPostOrPutOnlyTest_Verb('GET');
					ExecuteHttpPostOrPutOnlyTest_Verb('POST');
					ExecuteHttpPostOrPutOnlyTest_Verb('PUT');
					ExecuteHttpPostOrPutOnlyTest_Verb('DELETE');
				};

				function ExecuteHttpPostOrPutOnlyTest_Verb(verb) {
					$('#pHttpPostOrPutOnlyTest span').remove();

					$.ajax({
						url: 'MyFirstHandler.ashx',
						type: verb,
						data: { method: 'PostOrPutData' },
						success: function (data) { $('#pHttpPostOrPutOnlyTest').append('<span style="color:#0f0;padding:0 5px;">' + verb + '</span>'); },
						error: function () { $('#pHttpPostOrPutOnlyTest').append('<span style="color:#f00;padding:0 5px;">' + verb + '</span>'); }
					});
				};
			</script>
		</p>

    </div>
    </form>
</body>
</html>
