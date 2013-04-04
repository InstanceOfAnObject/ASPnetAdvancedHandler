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
        				Street: 'A street somewhereeeee',
        				DoorNumber: '123A',
        				PostalCode: {
        					Code: '555-1234',
        					City: 'Lisbon',
        					Country: 'Portugal'
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
		<input type="button" onclick="AJAXSendArray()" value="Send person data" />

		<script type="text/javascript">
			function AJAXSendArray() {
				$.ajax({
					url: 'MyFirstHandler.ashx',
					type: 'GET',   // I'm doing a POST here just to show it can handle it too... 
					data: { method: 'AJAXSendArray',
						args: {
							items: [1, 2, 3],
							addresses: [
								{ Street: 'Avenue A', DoorNumber: '123', PostalCode: '2900-055' },
								{ Street: 'Avenue B', DoorNumber: '45', PostalCode: '1900-456' }
							]
						}
					},
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
