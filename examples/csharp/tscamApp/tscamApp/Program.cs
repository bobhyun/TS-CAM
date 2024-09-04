/**
 * 설치 패키지: NuGet으로 SocketIOClient 패키지를 설치
 * 
 * 이 예제는 .NET 6.0, SocketIOClient 3.1.1에서 테스트 했습니다.
 */ 

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // 한글 출력을 위해 콘솔을 UTF8로 설정
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var tscam = "http://localhost:10000";
        var client = new SocketIOClient.SocketIO(tscam);
        var reconnectAlways = true;
        // 차번인식 옵션
        // [참고] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
        string anprOptionsString = "ms"; // 용도에 맞게 설정

        // 접속이 끊기면 자동 재접속 설정
        client.OnDisconnected += async (sender, e) =>
        {
            Console.WriteLine($"Disconnected from {tscam}, Attempting to reconnect...");
            if (reconnectAlways)
                await AttemptReconnect(client);
        };

        // 접속 성공한 경우
        client.OnConnected += async (sender, e) =>
        {
            Console.WriteLine($"Connected to {tscam}");


            pause("1. 내부망에 연결된 카메라 탐색");
            JsonElement? result = await EmitAck(client, "discover", new
            {
                timeout = 2000,         // 카메라 응답 대기 시간 (ms)
                device = "Ethernet"     // 또는 "Wi-Fi" (이 항목을 지정 안하면 알아서 처리함)
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
                //href = camList[0].GetProperty("href").GetString(), // 첫번째 카메라 URI
                href = "http://192.168.0.30/onvif/device_service",
                alias = "주차장입구",    // 이름 지정
                username = "admin",     // 카메라 로그인 ID
                password = "admin",     // 카메라 로그인 Password
                authType = "basic",     // 또는 "digest" (카메라에 로그인 방식 지정, 생략하면 basic을 의미함)
            };



            pause("2. 첫번째 카메라 정보 읽기 (카메라 로그인 필요)");
            result = await EmitAck(client, "info", camOptions);
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@info = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;

            JsonElement camInfo = json.GetProperty("info");
            //Console.WriteLine($"camInfo = {SerializeJson(camInfo)}");



            pause("3. 스냅샷 이미지 요청 (with 차번인식)");
            result = await EmitAck(client, "snapshot", new
            {
                camOptions.href,
                camOptions.alias,
                camOptions.username,
                camOptions.password,
                camOptions.authType,

                // 이 항목이 없으면 스냅샷 이미지만 받음
                // [주의]
                // anprOptions = "" 이렇게 하면 옵션없이 차량번호인식하라는 의미임
                // 차량번호 인식을 하지 않으려면 anprOptions 항목을 삭제해야 함
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



            pause("4. 릴레이 출력");
            result = await EmitAck(client, "relayOutput", new
            {
                camOptions.href,
                camOptions.alias,
                camOptions.username,
                camOptions.password,
                camOptions.authType,
                portNo = 0, // 출력 포트번호는 0부터 시작함
                value = 1   // 1:ON, 0:OFF
            });
            if (result == null || !result.HasValue)
                return;

            json = result.Value;
            Console.WriteLine($"@relayOutput = {SerializeJson(json)}");
            if (json.GetProperty("result").GetBoolean() != true)
                return;



            pause("5. 이벤트 수신 대기 (이벤트는 카메라 이벤트 리스너를 통해 수신)");
            //    카메라 여러 대에서 동시에 이벤트를 수신하는 경우를 표현하기 위해 camOptions 여러 개를 배열로 구성
            //    [알림] 이벤트 수신 카메라 수는 차번인식엔진 라이선스에 따라 최대 수량이 제한됩니다.

            var camOptionsArray = new[]
            {
                new // 첫번째 카메라
                {
                    camOptions.href,
                    camOptions.alias,
                    camOptions.username,
                    camOptions.password,
                    camOptions.authType,

                    // 이벤트 발생시 자동으로 스냅샷 이미지를 받아와 차번인식하도록 설정
                    // anprOptions 대신 snapshot = true로 설정하면 스냅샷 이미지만 수신
                    // anprOptions 항목을 설정하지 않으면 이벤트 내용만 수신됨
                    anprOptions = anprOptionsString                    
                },
                /*
                new // 두번째 카메라
                {
                    //href = camList[1].GetProperty("href").GetString(), // 두번째 카메라 URI
                    href = "http://192.168.0.31/onvif/device_service",
                    alias = "주차장출구",    // 이름 지정
                    username = "admin",     // 카메라 로그인 ID
                    password = "admin",     // 카메라 로그인 Password
                    authType = "basic",     // 또는 "digest" (카메라에 로그인 방식 지정, 생략하면 basic을 의미함),
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

            // 배열 내의 각 항목들의 result 검사
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

            //if (successCount <= 0 || failureCount > 0)  // 카메라 중 일부가 실패한 경우
            //    return;


            pause("6. 이벤트 수신 대기 목록");
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

            // 여기서 이벤트 수신하도록 동안 프로그램 흐름을 중지
            // (이 상태로 카메라의 디지털 입력를 발생시키면 이벤트가 수신됨 
            pause("이벤트 수신 대기중... (카메라 Digital Input으로 입력을 넣으세요.)", true);

            // 여기서는 재접속을 방지하고 예제 프로그램 종료하는 흐름으로 진행
            // (실제 응용 프로그램에서는 이런 흐름은 필요없음)
            reconnectAlways = false;

            
            pause("7. 이벤트 수신 종료");
            result = await EmitAck(client, "unwatchEvents", camOptionsArray);
            if (result != null || result.HasValue)
            { 
                json = result.Value;
                Console.WriteLine($"@unwatchEvents = {SerializeJson(json)}");
                if (json.GetProperty("result").GetBoolean() != true) {
                    //return;   // 프로그램을 종료하기 위해 의도적으로 return 하지않음
                }
            }


            pause("8. 접속 종료");
            await client.DisconnectAsync();

            // 프로그램 종료
            Environment.Exit(0);
        };

        // 카메라 이벤트 리스너
        client.On("@event", response =>
        {
            // 이벤트 여러 개가 한번에 들어올수 있어 배열로 구성
            JsonElement json = response.GetValue<JsonElement>();
            Console.WriteLine($"Received '@event': {SerializeJson(json)}");

            JsonElement href = json.GetProperty("href");
            JsonElement alias = json.GetProperty("alias");
            JsonElement events = json.GetProperty("events");
            foreach (JsonElement item in events.EnumerateArray())
            {
                JsonElement timestamp = item.GetProperty("timestamp");  // 이벤트 발생 일시
                JsonElement type = item.GetProperty("type");            // 이벤트 종류 (현재는 "digitalInput"만 있음)
                JsonElement portNo = item.GetProperty("portNo");        // 디지털 입력 포트 번호
                JsonElement value = item.GetProperty("value");          // 입력 값
                JsonElement? snapshot = item.GetProperty("snapshot");   // 스냅샷 이미지
                JsonElement? anpr = item.GetProperty("anpr");           // 차번인식 결과

                if (!snapshot.HasValue || !anpr.HasValue)
                    continue;

                Console.WriteLine($"snapshot image = {SerializeJson(snapshot.Value)}");
                // 스냅샷 이미지는 지정된 data 디렉토리 아래에
                // $"{date}/{alias}/${alias}-YYYYMMDD-hhmmss.SSS_${plateNo}.jpg" 경로에 자동 저장됨
                var filePath = snapshot.Value.GetProperty("filePath").GetString();
                var downloadURI = snapshot.Value.GetProperty("uri").GetString();

                Console.WriteLine($"anpr result = {SerializeJson(anpr.Value)}");


                // 차번인식 결과도 번호판이 여러 개일 수 있으므로 배열로 구성
                foreach (JsonElement licensePlate in anpr.Value.EnumerateArray())
                {
                    if (licensePlate.TryGetProperty("text", out JsonElement plateNo))
                    {
                        string? text = plateNo.GetString();
                        if (text == null)
                            continue;

                        Console.WriteLine($"PlateNo: {text}");

                        // 여기서 필요시 릴레이 출력 (차단기 개방)
                        var camOptions = new
                        {
                            href = href,            // 이벤트 발생 카메라
                            alias = alias,          // 이름 지정
                            username = "admin",     // 카메라 로그인 ID
                            password = "admin",     // 카메라 로그인 Password
                            authType = "basic",     // 또는 "digest" (카메라에 로그인 방식 지정, 생략하면 basic을 의미함)
                            portNo = 0,             // 출력 포트번호는 0부터 시작함
                            value = 1               // 1:ON, 0:OFF
                        };

                        // 여기서는 다음 이벤트에 대한 지연이 발생하지 않도록 실시간 처리를 위해 비동기 호출을 권장함
                        // 비동기 호출의 경우 @relayOutput로 결과가 수신됨
                        client.EmitAsync("relayOutput", camOptions);

                        // 만약 이렇게 동기 호출을 하면 카메라에서 보내오는
                        // relayOutput 명령에 대한 응답을 수신할 때까지 프로그램 흐름이 중단되어
                        // 다음 이벤트가 도착하더라도 즉시 대응할 수 없으므로 지연이 발생할 수 있음
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

        // 릴레이 출력 비동기 응답 수신
        client.On("@relayOutput", response =>
        {
            JsonElement json = response.GetValue<JsonElement>();
            Console.WriteLine($"Received '@relayOutput': {SerializeJson(json)}");
        });


        await client.ConnectAsync();

        // 예제가 콘솔 프로그램이라 이벤트가 발생하기 전에
        // 메인 스레드가 종료되지 않도록 대기시킴
        // GUI 프로그램을 작성할 경우는 이벤트 방식이므로 이렇게 할 필요없음
        await Task.Delay(-1);
    }

    // 유틸리티 함수들
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
        int delayMs = 5000; // 5초 간격으로 재시도

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
        Console.ReadKey();
    }

}
