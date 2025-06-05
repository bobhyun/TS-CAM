(**
 * Package: Install SocketIOClient via NuGet
 * 
 * This example was tested on .NET 6.0, SocketIOClient 3.1.1.
 *) 

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

open System
open System.Text.Json
open System.Threading.Tasks
open SocketIOClient

// Configuration
let tscam = "http://localhost:10000"
// Vehicle number recognition options
// [Reference] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
let anprOptionsString = "ms" // Set according to purpose
let reconnectAlways = ref true

// Record types
type DiscoverOptions = {
    timeout: int
    device: string
}

type CameraOptions = {
    href: string
    alias: string
    username: string
    password: string
    authType: string
    anprOptions: string
}

type SnapshotOptions = {
    href: string
    alias: string
    username: string
    password: string
    authType: string
    anprOptions: string
}

type RelayOptions = {
    href: string
    alias: string
    username: string
    password: string
    authType: string
    portNo: int
    value: int
}

// Utility functions
let serializeJson (element: JsonElement) =
    let jsOptions = JsonSerializerOptions()
    jsOptions.WriteIndented <- true
    jsOptions.Encoder <- System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    JsonSerializer.Serialize(element, jsOptions)

let pause (message: string) (isExit: bool) =
    printfn "---------------------------------------------------------------"
    printfn "%s" message
    
    if isExit then
        printfn "Press any key to exit..."
    else
        printfn "Press any key to continue..."
    
    Console.ReadKey(true) |> ignore
    printfn ""

// EmitAck function with proper response handling and timeout
let emitAckAsync (client: SocketIO) (eventName: string) (options: obj option) : Task<JsonElement option> =
    let tcs = TaskCompletionSource<JsonElement option>()
    
    task {
        try
            // Define callback function to process server response (acknowledgment)
            let callback = Action<SocketIOResponse>(fun response ->
                try
                    let jsonResponse = response.GetValue<JsonElement>()
                    tcs.SetResult(Some jsonResponse)
                with ex ->
                    printfn "Error parsing response: %s" ex.Message
                    tcs.SetResult(None)
            )
            
            try
                do! client.EmitAsync(eventName, callback, options |> Option.toObj)
            with ex ->
                printfn "Error emitting event '%s': %s" eventName ex.Message
                tcs.SetResult(None)
            
            // Wait for result from TaskCompletionSource
            let! result = tcs.Task
            return result
        with ex ->
            printfn "Error emitting event '%s': %s" eventName ex.Message
            tcs.SetResult(None)
            return None
    }

// Recursive function for reconnection attempts
let rec attemptReconnect (client: SocketIO) =
    task {
        printfn "Disconnected from %s, Attempting to reconnect..." tscam
        
        if !reconnectAlways then
            do! attemptReconnect client
        return ()
    }

[<EntryPoint>]
let main _ =
    // Set console to UTF8 for Korean output
    Console.OutputEncoding <- System.Text.Encoding.UTF8
    
    let client = new SocketIO(tscam)
    let reconnectAlways = ref true
    
    // Handle disconnection
    client.OnDisconnected.Add(fun _ ->
        printfn "Disconnected from %s, Attempting to reconnect..." tscam
        if !reconnectAlways then
            attemptReconnect client |> ignore
    )
    
    // Camera event listener
    client.On("@event", fun response ->
        // Multiple events can come at once, so they are organized in an array
        let json = response.GetValue<JsonElement>()
        printfn "Received '@event': %s" (serializeJson json)
        
        let href = json.GetProperty("href")
        let alias = json.GetProperty("alias")
        let events = json.GetProperty("events")
        
        for item in events.EnumerateArray() do
            let timestamp = item.GetProperty("timestamp")  // Event timestamp
            let eventType = item.GetProperty("type")       // Event type (currently only "digitalInput")
            let portNo = item.GetProperty("portNo")        // Digital input port number
            let value = item.GetProperty("value")          // Input value
            
            let mutable snapshotElement = Unchecked.defaultof<JsonElement>
            let mutable anprElement = Unchecked.defaultof<JsonElement>
            
            let hasSnapshot = item.TryGetProperty("snapshot", &snapshotElement)
            let hasAnpr = item.TryGetProperty("anpr", &anprElement)
            
            if hasSnapshot && hasAnpr then
                printfn "snapshot image = %s" (serializeJson snapshotElement)
                // Snapshot image is automatically saved in the specified data directory
                // "${date}/${alias}/${alias}-YYYYMMDD-hhmmss.SSS_${plateNo}.jpg" path
                let filePath = snapshotElement.GetProperty("filePath").GetString()
                let downloadURI = snapshotElement.GetProperty("uri").GetString()
                
                printfn "anpr result = %s" (serializeJson anprElement)
                
                // Vehicle number recognition result is also an array because multiple license plates can be recognized
                for licensePlate in anprElement.EnumerateArray() do
                    let mutable plateNo = Unchecked.defaultof<JsonElement>
                    if licensePlate.TryGetProperty("text", &plateNo) then
                        let text = plateNo.GetString()
                        if not (isNull text) then
                            printfn "PlateNo: %s" text
                            
                            // Here, output relay (barrier opening) if needed
                            let camOptions = {|
                                href = href.GetString()     // Event camera
                                alias = alias.GetString()   // Camera name
                                username = "admin"          // Camera login ID
                                password = "admin"          // Camera login Password
                                authType = "basic"          // or "digest" (specifies camera login method, defaults to basic if omitted)
                                portNo = 0                  // Output port number starts from 0
                                value = 1                   // 1:ON, 0:OFF
                            |}
                            
                            // Here, we recommend asynchronous calls for real-time processing to avoid delays for the next event
                            // In case of asynchronous calls, the result is received via @relayOutput
                            client.EmitAsync("relayOutput", camOptions) |> ignore
    )
    
    // Relay output asynchronous response reception
    client.On("@relayOutput", fun response ->
        let json = response.GetValue<JsonElement>()
        printfn "Received '@relayOutput': %s" (serializeJson json)
    )
    
    // When connection succeeds
    client.OnConnected.Add(fun _ ->
        task {
            printfn "Connected to %s" tscam
            
            pause "1. Discovering cameras in the internal network" false
            let! result = emitAckAsync client "discover" (Some { timeout = 2000; device = "Ethernet" })
            
            match result with
            | None -> return ()
            | Some json ->
                printfn "@discover = %s" (serializeJson json)
                if not (json.GetProperty("result").GetBoolean()) then return ()
                
                let camList = json.GetProperty("devices")
                let camOptions = { href = "http://192.168.0.30/onvif/device_service"; alias = "Entrance"; username = "admin"; password = "admin"; authType = "basic"; anprOptions = anprOptionsString }
                
                pause "2. Reading first camera info (camera login required)" false
                let! result = emitAckAsync client "info" (Some camOptions)
                
                match result with
                | None -> return ()
                | Some json ->
                    printfn "@info = %s" (serializeJson json)
                    if not (json.GetProperty("result").GetBoolean()) then return ()
                    
                    let camInfo = json.GetProperty("info")
                    
                    pause "3. Requesting snapshot image (with vehicle number recognition)" false
                    let! result = emitAckAsync client "snapshot" (Some { href = camOptions.href; alias = camOptions.alias; username = camOptions.username; password = camOptions.password; authType = camOptions.authType; anprOptions = anprOptionsString })
                    
                    match result with
                    | None -> return ()
                    | Some json ->
                        printfn "@snapshot = %s" (serializeJson json)
                        if not (json.GetProperty("result").GetBoolean()) then return ()
                        
                        let snapshot = json.GetProperty("snapshot")
                        let anpr = json.GetProperty("anpr")
                        
                        pause "4. Relay output" false
                        let! result = emitAckAsync client "relayOutput" (Some { href = camOptions.href; alias = camOptions.alias; username = camOptions.username; password = camOptions.password; authType = camOptions.authType; portNo = 0; value = 1 })
                        
                        match result with
                        | None -> return ()
                        | Some json ->
                            printfn "@relayOutput = %s" (serializeJson json)
                            if not (json.GetProperty("result").GetBoolean()) then return ()
                            
                            pause "5. Waiting for events (events are received through camera event listeners)" false
                            let camOptionsArray = [| { href = camOptions.href; alias = camOptions.alias; username = camOptions.username; password = camOptions.password; authType = camOptions.authType; anprOptions = anprOptionsString } |] : CameraOptions array
                            
                            let! result = emitAckAsync client "watchEvents" (Some camOptionsArray)
                            
                            match result with
                            | None -> return ()
                            | Some json ->
                                printfn "@watchEvents = %s" (serializeJson json)
                                
                                let watchList = json.GetProperty("watchList")
                                if watchList.ValueKind <> JsonValueKind.Array then
                                    printfn "watchList is not an array."
                                    return ()
                                
                                let successCount = ref 0
                                let failureCount = ref 0
                                
                                // Process watchList results
                                for item in watchList.EnumerateArray() do
                                    match item.TryGetProperty("result") with
                                    | true, resultElement ->
                                        let res = resultElement.GetBoolean()
                                        let href = item.GetProperty("href").GetString()
                                        let alias = item.GetProperty("alias").GetString()
                                        let message = item.GetProperty("message").GetString()

                                        if res then
                                            successCount := !successCount + 1
                                        else
                                            failureCount := !failureCount + 1
                                            printfn "Camera %s (%s) failed to subscribe: %s" alias href message
                                    | false, _ ->
                                        failureCount := !failureCount + 1
                                        printfn "Item does not contain 'result' property"

                                if !successCount <= 0 || !failureCount > 0 then
                                    printfn "Failed to subscribe to events. Success: %d, Failure: %d" !successCount !failureCount
                                    return ()

                                // Here, stop the program flow while waiting for events
                                // (Events will be received when digital input is triggered by the camera)
                                pause "Waiting for events... (Trigger digital input on camera)" true

                                // Here, prevent reconnection and proceed to terminate the example program
                                // (This flow is unnecessary in actual applications)
                                reconnectAlways := false

                                // Check event watch list
                                pause "6. Waiting for event watch list" false
                                let! result = emitAckAsync client "watchList" None
                                
                                match result with
                                | None -> ()
                                | Some json ->
                                    printfn "@watchList = %s" (serializeJson json)
                                    if not (json.GetProperty("result").GetBoolean()) then ()

                                // Stop event reception
                                pause "7. Stop event reception" false
                                let! result = emitAckAsync client "unwatchEvents" (Some camOptionsArray)
                                
                                match result with
                                | None -> ()
                                | Some json ->
                                    printfn "@unwatchEvents = %s" (serializeJson json)
                                    if not (json.GetProperty("result").GetBoolean()) then ()
                                
                                pause "8. Disconnect" false
                                do! client.DisconnectAsync()
                                
                                Environment.Exit(0)
        } |> ignore
    )
    
    // Connect to the server
    client.ConnectAsync().Wait()
    
    // This is a console application, so we wait to prevent the main thread from terminating before events occur
    // In GUI applications, this is unnecessary as they use event-based approach
    Task.Delay(-1).Wait()
    
    0
