/**
 * Package: Install SocketIOClient via NuGet
 * 
 * This example was tested on .NET 6.0, SocketIOClient 3.1.1.
 */ 

// The MIT License (MIT)
// Copyright 2022-2025 TS-Solution Corp.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to all conditions.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace tscam_app;

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    public static async Task Main(string[] args)
    {
        // Set console to UTF8 for Korean output
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var tscam = "http://localhost:10000";
        var client = new SocketIOClient.SocketIO(tscam);
        var reconnectAlways = true;
        // Vehicle number recognition options
        // [Reference] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
        string anprOptionsString = "ms"; // Set according to purpose

        // Set auto-reconnect when disconnected
        client.OnDisconnected += async (sender, e) =>
        {
            Console.WriteLine($"Disconnected from {tscam}, Attempting to reconnect...");
            if (reconnectAlways)
                await AttemptReconnect(client);
        };

        // When connection succeeds
        client.OnConnected += async (sender, e) =>
        {
            Console.WriteLine($"Connected to {tscam}");

            pause("1. Discovering cameras in the internal network");
            JsonElement? result = await EmitAck(client, "discover", new
            {
                timeout = 2000,         // Camera response waiting time (ms)
                device = "Ethernet"     // or "Wi-Fi" (if not specified, it will be handled automatically)
            });
            if (result == null || !result.HasValue)
                return;

            JsonElement json = result.Value;
            Console.WriteLine($"@discover = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;

            JsonElement camList = json.GetProperty("devices");
            //Console.WriteLine($"camList = {SerializeJson(camList)}");

            var camOptions = new
            {
                //href = camList[0].GetProperty("href").GetString(), // First camera URI
                href = "http://192.168.0.30/onvif/device_service",
                alias = "Entrance",    // Camera name
                username = "admin",     // Camera login ID
                password = "admin",     // Camera login Password
                authType = "basic",     // or "digest" (specifies camera login method, defaults to basic if omitted),
            };

            pause("2. Reading first camera info (camera login required)");
            result = await EmitAck(client, "info", camOptions);
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@info = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;

            JsonElement camInfo = json.GetProperty("info");
            //Console.WriteLine($"camInfo = {SerializeJson(camInfo)}");

            pause("3. Requesting snapshot image (with vehicle number recognition)");
            result = await EmitAck(client, "snapshot", new
            {
                camOptions.href,
                camOptions.alias,
                camOptions.username,
                camOptions.password,
                camOptions.authType,

                // If this item is not present, only snapshot image will be received
                // [Note]
                // anprOptions = "" means to perform vehicle number recognition without options
                // To not perform vehicle number recognition, delete the anprOptions item
                anprOptions = anprOptionsString
            });
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@snapshot = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;

            JsonElement snapshot = json.GetProperty("snapshot");
            JsonElement anpr = json.GetProperty("anpr");
            //Console.WriteLine($"snapshot image = {SerializeJson(snapshot)}");
            //Console.WriteLine($"anpr result = {SerializeJson(anpr)}");

            pause("4. Relay output");
            result = await EmitAck(client, "relayOutput", new
            {
                camOptions.href,
                camOptions.alias,
                camOptions.username,
                camOptions.password,
                camOptions.authType,
                portNo = 0, // Output port number starts from 0
                value = 1   // 1:ON, 0:OFF
            });
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@relayOutput = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;

            pause("5. Waiting for events (events are received through camera event listeners)");
            //    To handle multiple cameras receiving events simultaneously, configure multiple camOptions in an array
            //    [Note] The number of event-receiving cameras is limited by the vehicle number recognition engine license.

            var camOptionsArray = new[]
            {
                new // First camera
                {
                    camOptions.href,
                    camOptions.alias,
                    camOptions.username,
                    camOptions.password,
                    camOptions.authType,

                    // Set to automatically receive snapshot images for vehicle number recognition when events occur
                    // Setting snapshot = true instead of anprOptions receives only snapshot images
                    // If anprOptions item is not set, only event content is received
                    anprOptions = anprOptionsString                    
                },
                /*
                new // Second camera
                {
                    //href = camList[1].GetProperty("href").GetString(), // Second camera URI
                    href = "http://192.168.0.31/onvif/device_service",
                    alias = "Exit",    // Camera name
                    username = "admin",     // Camera login ID
                    password = "admin",     // Camera login Password
                    authType = "basic",     // or "digest" (specifies camera login method, defaults to basic if omitted),
                    anprOptions = anprOptionsString 
                }
                */
            };

            result = await EmitAck(client, "watchEvents", camOptionsArray);
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@watchEvents = {SerializeJson(json)}");
            //if (json.GetProperty("result").GetBoolean() != true)
            //    return;

            // Check result of each item in array
            JsonElement watchList = json.GetProperty("watchList");
            if (watchList.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("watchList is not an array.");
                return;
            }

            int successCount = 0;
            int failureCount = 0;
            foreach (JsonElement item in watchList.EnumerateArray())
            {
                if (item.TryGetProperty("result", out JsonElement resultElement))
                {
                    bool res = resultElement.GetBoolean();
                    string? href = item.GetProperty("href").GetString();
                    string? alias = item.GetProperty("alias").GetString();
                    string? message = item.GetProperty("message").GetString();

                    if (res)
                    {
                        //Console.WriteLine($"Camera {alias} ({href}) successfully subscribed");
                        successCount++;
                    }
                    else
                    {
                        //Console.WriteLine($"Camera {alias} ({href}) failed to subscribe");
                        failureCount++;
                    }
                }
                else
                {
                    Console.WriteLine("Item does not contain 'result' property.");
                    failureCount++;
                }
            }

            //if (successCount <= 0 || failureCount > 0)  // If some cameras failed
            //    return;


            pause("6. Waiting for event watch list");
            result = await EmitAck(client, "watchList");
            if (result != null || result.HasValue)
            {
                json = result.Value;
                Console.WriteLine($"@watchList = {SerializeJson(json)}");
                if (json.GetProperty("result").GetBoolean() != true)
                {
                    return;
                }
            }

            // Here, stop the program flow while waiting for events
            // (Events will be received when digital input is triggered by the camera)
            pause("Waiting for events... (Trigger digital input on camera)", true);

            // Here, prevent reconnection and proceed to terminate the example program
            // (This flow is unnecessary in actual applications)
            reconnectAlways = false;

            
            pause("7. Stop event reception");
            result = await EmitAck(client, "unwatchEvents", camOptionsArray);
            if (result != null || result.HasValue)
            { 
                json = result.Value;
                Console.WriteLine($"@unwatchEvents = {SerializeJson(json)}");
                if (json.GetProperty("result").GetBoolean() != true) {
                    //return;   // Intentionally not returning to terminate the program
                }
            }


            pause("8. Disconnect");
            await client.DisconnectAsync();

            // Terminate program
            Environment.Exit(0);
        };

        // Camera event listener
        client.On("@event", response =>
        {
            // Multiple events can come at once, so they are organized in an array
            JsonElement json = response.GetValue<JsonElement>();
            Console.WriteLine($"Received '@event': {SerializeJson(json)}");

            JsonElement href = json.GetProperty("href");
            JsonElement alias = json.GetProperty("alias");
            JsonElement events = json.GetProperty("events");
            foreach (JsonElement item in events.EnumerateArray())
            {
                JsonElement timestamp = item.GetProperty("timestamp");  // Event timestamp
                JsonElement type = item.GetProperty("type");            // Event type (currently only "digitalInput")
                JsonElement portNo = item.GetProperty("portNo");        // Digital input port number
                JsonElement value = item.GetProperty("value");          // Input value
                JsonElement? snapshot = item.GetProperty("snapshot");   // Snapshot image
                JsonElement? anpr = item.GetProperty("anpr");           // Vehicle number recognition result

                if (!snapshot.HasValue || !anpr.HasValue)
                    continue;

                Console.WriteLine($"snapshot image = {SerializeJson(snapshot.Value)}");
                // Snapshot image is automatically saved in the specified data directory
                // "${date}/${alias}/${alias}-YYYYMMDD-hhmmss.SSS_${plateNo}.jpg" path
                var filePath = snapshot.Value.GetProperty("filePath").GetString();
                var downloadURI = snapshot.Value.GetProperty("uri").GetString();

                Console.WriteLine($"anpr result = {SerializeJson(anpr.Value)}");

                // Vehicle number recognition result is also an array because multiple license plates can be recognized
                foreach (JsonElement licensePlate in anpr.Value.EnumerateArray())
                {
                    if (licensePlate.TryGetProperty("text", out JsonElement plateNo))
                    {
                        string? text = plateNo.GetString();
                        if (text == null)
                            continue;

                        Console.WriteLine($"PlateNo: {text}");

                        // Here, output relay (barrier opening) if needed
                        var camOptions = new
                        {
                            href = href,            // Event camera
                            alias = alias,          // Camera name
                            username = "admin",     // Camera login ID
                            password = "admin",     // Camera login Password
                            authType = "basic",     // or "digest" (specifies camera login method, defaults to basic if omitted)
                            portNo = 0,             // Output port number starts from 0
                            value = 1               // 1:ON, 0:OFF
                        };

                        // Here, we recommend asynchronous calls for real-time processing to avoid delays for the next event
                        // In case of asynchronous calls, the result is received via @relayOutput
                        client.EmitAsync("relayOutput", camOptions);

                        // If we make a synchronous call, the program flow will be blocked until
                        // the response to the relayOutput command is received from the camera
                        // This can cause delays as the program cannot immediately respond to the next event
                        /*
                        var result = await EmitAck(client, "relayOutput", camOptions);
                        if (!result.HasValue)
                            return;

                        json = result.Value;
                        Console.WriteLine($"@relayOutput = {SerializeJson(json)}");
                        */
                    }
                }
            }

        });

        // Relay output asynchronous response reception
        client.On("@relayOutput", response =>
        {
            JsonElement json = response.GetValue<JsonElement>();
            Console.WriteLine($"Received '@relayOutput': {SerializeJson(json)}");
        });


        await client.ConnectAsync();

        // This is a console application, so we wait to prevent the main thread from terminating before events occur
        // In GUI applications, this is unnecessary as they use event-based approach
        await Task.Delay(-1);
    }

    // Utility functions
    static string SerializeJson(JsonElement element)
    {
        var jsOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Serialize(element, jsOptions);
    }

    static async Task<JsonElement?> EmitAck(SocketIOClient.SocketIO client, string eventName, object? options = null, int timeoutMs = 30000)
    {
        var tcs = new TaskCompletionSource<JsonElement?>();

        try
        {
            await client.EmitAsync(eventName, response =>
            {
                try
                {
                    JsonElement jsonResponse = response.GetValue<JsonElement>();
                    tcs.SetResult(jsonResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing response: {ex.Message}");
                    tcs.SetResult(null);
                }
            }, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error emitting event '{eventName}': {ex.Message}");
            tcs.SetResult(null);
        }

        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(timeoutMs));
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            Console.WriteLine($"Request timed out after {timeoutMs} milliseconds");
            return null;
        }

        return await tcs.Task;
    }

    static async Task AttemptReconnect(SocketIOClient.SocketIO client)
    {
        int maxRetries = 5;
        int delayMs = 5000; // Retry with 5-second intervals

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await client.ConnectAsync();
                Console.WriteLine("Reconnected to server.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnection attempt {i + 1} failed: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        Console.WriteLine("Failed to reconnect after multiple attempts.");
    }

    static void pause(string message, bool isExit = false)
    {
        if (message != null)
        {
            Console.WriteLine("---------------------------------------------------------------");
            Console.WriteLine(message);
        }

        if (isExit)
        {
            Console.WriteLine("Press any key to exit...");
        }
        else
        {
            Console.WriteLine("Press any key to continue...");
        }
        Console.ReadKey(true);
        Console.WriteLine();
    }
}