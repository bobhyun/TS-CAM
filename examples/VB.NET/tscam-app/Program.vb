' Package: Install SocketIoClientDotNet via NuGet
' 
' This example was tested on .NET 6.0, SocketIoClientDotNet 3.1.1.
' 
' The MIT License (MIT)
' Copyright 2022-2025 TS-Solution Corp.
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to all conditions.
' 
' The above copyright notice and this permission notice shall be included in all
' copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.

Imports System
Imports System.Text.Json
Imports System.Threading.Tasks
Imports SocketIOClient

Module Program
    Public Sub Main()
        MainAsync(New String(){}).GetAwaiter().GetResult()
    End Sub

    Public Async Function MainAsync(args As String()) As Task
        ' Set console to UTF8 for output
        Console.OutputEncoding = System.Text.Encoding.UTF8

        Dim tscam = "http://localhost:10000"
        Dim client = New SocketIOClient.SocketIO(tscam)
        Dim reconnectAlways = True
        ' Vehicle number recognition options
        ' [Reference] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
        Dim anprOptionsString As String = "ms" ' Set according to purpose

        ' Set auto-reconnect when disconnected
        AddHandler client.OnDisconnected, Async Sub(sender, e)
            Console.WriteLine($"Disconnected from {tscam}, Attempting to reconnect...")
            If reconnectAlways Then
                Await AttemptReconnect(client)
            End If
        End Sub

        ' When connection succeeds
        AddHandler client.OnConnected, Async Sub(sender, e)
            Console.WriteLine($"Connected to {tscam}")

            pause("1. Discovering cameras in the internal network")
            Dim result = Await EmitAck(client, "discover", New With {
                .timeout = 2000,         ' Camera response waiting time (ms)
                .device = "Ethernet"     ' or "Wi-Fi" (if not specified, it will be handled automatically)
            })
            If result Is Nothing Then Return

            Dim json = result.Value
            Console.WriteLine($"@discover = {SerializeJson(json)}")
            If Not json.GetProperty("result").GetBoolean() Then Return

            Dim camList = json.GetProperty("devices")
            'Console.WriteLine($"camList = {SerializeJson(camList)}")

            Dim camOptions = New With {
                .href = "http://192.168.0.30/onvif/device_service",
                .alias = "Entrance",
                .username = "admin",
                .password = "admin",
                .authType = "basic"
            }

            pause("2. Reading first camera info (camera login required)")
            result = Await EmitAck(client, "info", camOptions)
            If result Is Nothing Then Return

            json = result.Value
            Console.WriteLine($"@info = {SerializeJson(json)}")
            If Not json.GetProperty("result").GetBoolean() Then Return

            Dim camInfo = json.GetProperty("info")
            'Console.WriteLine($"camInfo = {SerializeJson(camInfo)}")

            pause("3. Requesting snapshot image (with vehicle number recognition)")
            Dim snapshot As JsonElement
            Dim anpr As JsonElement
            Dim hasSnapshot As Boolean = json.TryGetProperty("snapshot", snapshot)
            Dim hasAnpr As Boolean = json.TryGetProperty("anpr", anpr)
            If hasSnapshot AndAlso hasAnpr Then
                'Console.WriteLine($"snapshot image = {SerializeJson(snapshot)}")
                'Console.WriteLine($"anpr result = {SerializeJson(anpr)}")
            End If
            result = Await EmitAck(client, "snapshot", New With {
                .href = camOptions.href,
                .alias = camOptions.alias,
                .username = camOptions.username,
                .password = camOptions.password,
                .authType = camOptions.authType,
                .anprOptions = anprOptionsString
            })
            If result Is Nothing Then Return

            json = result.Value
            Console.WriteLine($"@snapshot = {SerializeJson(json)}")
            If Not json.GetProperty("result").GetBoolean() Then Return

            snapshot = json.GetProperty("snapshot")
            anpr = json.GetProperty("anpr")
            'Console.WriteLine($"snapshot image = {SerializeJson(snapshot)}")
            'Console.WriteLine($"anpr result = {SerializeJson(anpr)}")

            pause("4. Relay output")
            result = Await EmitAck(client, "relayOutput", New With {
                .href = camOptions.href,
                .alias = camOptions.alias,
                .username = camOptions.username,
                .password = camOptions.password,
                .authType = camOptions.authType,
                .portNo = 0, ' Output port number starts from 0
                .value = 1   ' 1:ON, 0:OFF
            })
            If result Is Nothing Then Return

            json = result.Value
            Console.WriteLine($"@relayOutput = {SerializeJson(json)}")
            If Not json.GetProperty("result").GetBoolean() Then Return

            ' You can add more cameras to this array if needed
            Dim camOptionsArray = New Object() {
                New With {
                    .href = camOptions.href,
                    .alias = camOptions.alias,
                    .username = camOptions.username,
                    .password = camOptions.password,
                    .authType = camOptions.authType,
                    .anprOptions = anprOptionsString
                }
            }

            pause("5. Waiting for events (events are received through camera event listeners)")
            ' To handle multiple cameras receiving events simultaneously, configure multiple camOptions in an array
            ' [Note] The number of event-receiving cameras is limited by the vehicle number recognition engine license.

            result = Await EmitAck(client, "watchEvents", camOptionsArray)
            If result Is Nothing Then Return

            json = result.Value
            Console.WriteLine($"@watchEvents = {SerializeJson(json)}")
            Dim watchList = json.GetProperty("watchList")
            If watchList.ValueKind <> JsonValueKind.Array Then
                Console.WriteLine("watchList is not an array.")
                Return
            End If

            Dim successCount As Integer = 0
            Dim failureCount As Integer = 0
            For Each item In watchList.EnumerateArray()
                Dim resultElement As JsonElement = Nothing
                If item.TryGetProperty("result", resultElement) Then
                    Dim res = resultElement.GetBoolean()
                    Dim href = item.GetProperty("href").GetString()
                    Dim aliasName = item.GetProperty("alias").GetString()
                    Dim message = item.GetProperty("message").GetString()
                    If res Then
                        'Console.WriteLine($"Camera {aliasName} ({href}) successfully subscribed")
                        successCount += 1
                    Else
                        'Console.WriteLine($"Camera {aliasName} ({href}) failed to subscribe")
                        failureCount += 1
                    End If
                Else
                    Console.WriteLine("Item does not contain 'result' property.")
                    failureCount += 1
                End If
            Next

            'If (successCount <= 0 Or failureCount > 0) Then Return ' If some cameras failed

            pause("6. Waiting for event watch list")
            result = Await EmitAck(client, "watchList")
            If result IsNot Nothing Then
                json = result.Value
                Console.WriteLine($"@watchList = {SerializeJson(json)}")
                If Not json.GetProperty("result").GetBoolean() Then Return
            End If

            ' Here, stop the program flow while waiting for events
            ' (Events will be received when digital input is triggered by the camera)
            pause("Waiting for events... (Trigger digital input on camera)", True)

            ' Here, prevent reconnection and proceed to terminate the example program
            ' (This flow is unnecessary in actual applications)
            reconnectAlways = False

            pause("7. Stop event reception")
            result = Await EmitAck(client, "unwatchEvents", camOptionsArray)
            If result IsNot Nothing Then
                json = result.Value
                Console.WriteLine($"@unwatchEvents = {SerializeJson(json)}")
                'If Not json.GetProperty("result").GetBoolean() Then Return ' Intentionally not returning to terminate the program
            End If

            pause("8. Disconnect")
            Await client.DisconnectAsync()

            ' Terminate program
            Environment.Exit(0)
        End Sub

        ' Camera event listener
        client.On("@event", Sub(response)
            ' Multiple events can come at once, so they are organized in an array
            Dim json = response.GetValue(Of JsonElement)()
            Console.WriteLine($"Received '@event': {SerializeJson(json)}")

            Dim href = json.GetProperty("href")
            Dim aliasName = json.GetProperty("alias")
            Dim eventsArr = json.GetProperty("events")
            For Each item In eventsArr.EnumerateArray()
                Dim timestamp = item.GetProperty("timestamp")  ' Event timestamp
                Dim type = item.GetProperty("type")            ' Event type (currently only "digitalInput")
                Dim portNo = item.GetProperty("portNo")        ' Digital input port number
                Dim value = item.GetProperty("value")          ' Input value
                Dim snapshot As JsonElement
                Dim anpr As JsonElement
                Dim hasSnapshot As Boolean = item.TryGetProperty("snapshot", snapshot)
                Dim hasAnpr As Boolean = item.TryGetProperty("anpr", anpr)
                If hasSnapshot AndAlso hasAnpr Then
                    Console.WriteLine($"snapshot image = {SerializeJson(snapshot)}")
                    ' Snapshot image is automatically saved in the specified data directory
                    ' "${date}/${alias}/${alias}-YYYYMMDD-hhmmss.SSS_${plateNo}.jpg" path
                    Dim filePath = snapshot.GetProperty("filePath").GetString()
                    Dim downloadURI = snapshot.GetProperty("uri").GetString()

                    Console.WriteLine($"anpr result = {SerializeJson(anpr)}")

                    ' Vehicle number recognition result is also an array because multiple license plates can be recognized
                    For Each licensePlate In anpr.EnumerateArray()
                        Dim plateNo As JsonElement
                        Dim hasPlateNo As Boolean = licensePlate.TryGetProperty("text", plateNo)
                        If hasPlateNo Then
                            Dim text = plateNo.GetString()
                            If text Is Nothing Then Continue For
                            Console.WriteLine($"PlateNo: {text}")

                            ' Here, output relay (barrier opening) if needed
                            Dim camOptions2 = New With {
                                .href = href,
                                .alias = aliasName,
                                .username = "admin",
                                .password = "admin",
                                .authType = "basic",
                                .portNo = 0,
                                .value = 1
                            }

                            ' Here, we recommend asynchronous calls for real-time processing to avoid delays for the next event
                            ' In case of asynchronous calls, the result is received via @relayOutput
                            client.EmitAsync("relayOutput", camOptions2)

                            ' If we make a synchronous call, the program flow will be blocked until
                            ' the response to the relayOutput command is received from the camera
                            ' This can cause delays as the program cannot immediately respond to the next event
                            '
                            'Dim result2 = Await EmitAck(client, "relayOutput", camOptions2)
                            'If Not result2.HasValue Then Return
                            'json = result2.Value
                            'Console.WriteLine($"@relayOutput = {SerializeJson(json)}")
                            '
                        End If
                    Next
                End If
            Next
        End Sub)

        ' Relay output asynchronous response reception
        client.On("@relayOutput", Sub(response)
            Dim json = response.GetValue(Of JsonElement)()
            Console.WriteLine($"Received '@relayOutput': {SerializeJson(json)}")
        End Sub)

        Await client.ConnectAsync()

        ' This is a console application, so we wait to prevent the main thread from terminating before events occur
        ' In GUI applications, this is unnecessary as they use event-based approach
        Await Task.Delay(-1)
    End Function

    ' Utility functions
    Function SerializeJson(element As JsonElement) As String
        Dim jsOptions As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }
        Return JsonSerializer.Serialize(element, jsOptions)
    End Function

    Async Function EmitAck(client As SocketIOClient.SocketIO, eventName As String, Optional options As Object = Nothing, Optional timeoutMs As Integer = 30000) As Task(Of JsonElement?)
        Dim tcs As New TaskCompletionSource(Of JsonElement?)()
        Try
            Await client.EmitAsync(eventName, Sub(response)
                Try
                    Dim jsonResponse = response.GetValue(Of JsonElement)()
                    tcs.SetResult(jsonResponse)
                Catch ex As Exception
                    Console.WriteLine($"Error parsing response: {ex.Message}")
                    tcs.SetResult(Nothing)
                End Try
            End Sub, options)
        Catch ex As Exception
            Console.WriteLine($"Error emitting event '{eventName}': {ex.Message}")
            tcs.SetResult(Nothing)
        End Try
        Dim timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(timeoutMs))
        Dim completedTask = Await Task.WhenAny(tcs.Task, timeoutTask)
        If completedTask Is timeoutTask Then
            Console.WriteLine($"Request timed out after {timeoutMs} milliseconds")
            Return Nothing
        End If
        Return Await tcs.Task
    End Function

    Async Function AttemptReconnect(client As SocketIOClient.SocketIO) As Task
        Dim maxRetries = 5
        Dim delayMs = 5000 ' Retry with 5-second intervals
        For i = 0 To maxRetries - 1
            Dim needDelay As Boolean = False
            Try
                Await client.ConnectAsync()
                Console.WriteLine("Reconnected to server.")
                Return
            Catch ex As Exception
                Console.WriteLine($"Reconnection attempt {i + 1} failed: {ex.Message}")
                needDelay = True
            End Try
            If needDelay Then
                Await Task.Delay(delayMs)
            End If
        Next
        Console.WriteLine("Failed to reconnect after multiple attempts.")
    End Function

    Sub pause(message As String, Optional isExit As Boolean = False)
        If message IsNot Nothing Then
            Console.WriteLine("---------------------------------------------------------------")
            Console.WriteLine(message)
        End If
        If isExit Then
            Console.WriteLine("Press any key to exit...")
        Else
            Console.WriteLine("Press any key to continue...")
        End If
        Console.ReadKey(True)
        Console.WriteLine()
    End Sub
End Module 