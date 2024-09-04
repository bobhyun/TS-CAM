/*
    socket.io 클라이언트 사용하기 위한 의존성 설치
        io.socket:socket.io-client:2.1.1
    json 사용을 위한 의존성 설치
        org.json:json:20240303

    @author: https://github.com/bobhyun/TS-ANPR
    Copyright (C) 2024. All rights reserved.
 */
import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.client.Ack;
import org.json.JSONException;
import org.json.JSONArray;
import org.json.JSONObject;
import java.net.URISyntaxException;
import java.util.concurrent.CompletableFuture;
import java.util.Scanner;

public class Main {
    private static final String TSCAM = "http://localhost:10000";
    private static boolean reconnectAlways = true;
    private static final String ANPR_OPTIONS_STRING = "ms";
    private static Socket socket;

    public static void main(String[] args) throws URISyntaxException {
        socket = IO.socket(TSCAM);
        CompletableFuture<Void> connectionFuture = new CompletableFuture<>();
        setupSocketListeners(connectionFuture);
        socket.connect();

        connectionFuture.thenCompose(v -> runMainLogic())
                .exceptionally(e -> {
                    System.err.println("Error in main logic: " + e.getMessage());
                    return null;
                })
                .thenRun(() -> {
                    System.out.println("All operations completed. Disconnecting...");
                    System.exit(0);
                });
    }

    private static void setupSocketListeners(CompletableFuture<Void> connectionFuture) {
        socket.on(Socket.EVENT_CONNECT, args -> {
            System.out.println("Connected to " + TSCAM);
            connectionFuture.complete(null);
        });

        socket.on(Socket.EVENT_CONNECT_ERROR, args -> {
            connectionFuture.completeExceptionally(new RuntimeException("Connection error"));
        });

        // @event 수신 핸들러 추가
        socket.on("@event", args -> {
            JSONObject data = (JSONObject) args[0];
            printJson("@event=", data);
        });
    }

    private static CompletableFuture<Void> runMainLogic() {
        return discoverCameras()
            .thenCompose(v -> getFirstCameraInfo())
            .thenCompose(v -> requestSnapshot())
            .thenCompose(v -> controlRelayOutput())
            .thenCompose(v -> watchEvents())
            .thenCompose(v -> getWatchList())
            .thenCompose(v -> waitForEvents())
            .thenCompose(v -> unwatchEvents())
            .thenCompose(v -> disconnect());
    }


    private static CompletableFuture<Void> discoverCameras() {
        return pause("1. 내부망에 연결된 카메라 탐색")
                .thenCompose(v -> {
                    try {
                        JSONObject params = new JSONObject();
                        params.put("timeout", 2000);
                        params.put("device", "Ethernet");
                        return emit("discover", params);
                    } catch (JSONException e) {
                        return CompletableFuture.failedFuture(e);
                    }
                })
                .thenAccept(result -> printJson("@discover=", result))
                .exceptionally(e -> {
                    System.err.println("Error in discoverCameras: " + e.getMessage());
                    return null;
                });
    }


    private static CompletableFuture<Void> getFirstCameraInfo() {
        return pause("2. 첫번째 카메라 정보 읽기 (카메라 로그인 필요)")
                .thenCompose(v -> emit("info", getCamOptions()))
                .thenAccept(result -> printJson("@info=", result));
    }

    // Implement other methods (requestSnapshot, controlRelayOutput, watchEvents, etc.) similarly
    private static CompletableFuture<Void> requestSnapshot() {
        return pause("3. 스냅샷 이미지 요청 (with 차번인식)")
                .thenCompose(v -> {
                    try {
                        JSONObject requestData = new JSONObject(getCamOptions().toString());
                        requestData.put("anprOptions", "ms"); // anprOptionsString 사용

                        return emit("snapshot", requestData);
                    } catch (JSONException e) {
                        return CompletableFuture.failedFuture(e);
                    }
                })
                .thenAccept(result -> printJson("@snapshot=", result))
                .exceptionally(e -> {
                    System.err.println("Error in requestSnapshot: " + e.getMessage());
                    return null;
                });
    }

    private static CompletableFuture<Void> controlRelayOutput() {
        return pause("4. 릴레이 출력")
                .thenCompose(v -> {
                    try {
                        JSONObject requestData = new JSONObject(getCamOptions().toString());
                        requestData.put("portNo", 0);  // 출력 포트번호는 0부터 시작
                        requestData.put("value", 1);   // 1: ON, 0: OFF

                        return emit("relayOutput", requestData);
                    } catch (JSONException e) {
                        return CompletableFuture.failedFuture(e);
                    }
                })
                .thenAccept(result -> printJson("@relayOutput=", result))
                .exceptionally(e -> {
                    System.err.println("Error in controlRelayOutput: " + e.getMessage());
                    return null;
                });
    }

    private static CompletableFuture<Void> watchEvents() {
        return pause("5. 이벤트 수신 대기 (이벤트는 카메라 이벤트 리스너를 통해 수신)")
                .thenCompose(v -> {
                    try {
                        JSONArray camOptionsArray = new JSONArray();
                        camOptionsArray.put(new JSONObject(getCamOptions().toString())
                                .put("anprOptions", "ms"));  // anprOptionsString 사용

                        return emit("watchEvents", camOptionsArray);
                    } catch (JSONException e) {
                        return CompletableFuture.failedFuture(e);
                    }
                })
                .thenAccept(result -> printJson("@watchEvents=", result))
                .exceptionally(e -> {
                    System.err.println("Error in watchEvents: " + e.getMessage());
                    return null;
                });
    }

    private static CompletableFuture<Void> getWatchList() {
        return pause("6. 이벤트 수신 대기 목록")
                .thenCompose(v -> emit("watchList", null))
                .thenAccept(result -> printJson("@watchList=", result));
    }

    private static CompletableFuture<Void> waitForEvents() {
        return pause("이벤트 수신 대기중... (카메라 Digital Input으로 입력을 넣으세요.)", true);
    }

    private static CompletableFuture<Void> unwatchEvents() {
        return pause("7. 이벤트 수신 종료")
                .thenCompose(v -> {
                    try {
                        JSONArray camOptionsArray = new JSONArray();
                        camOptionsArray.put(new JSONObject(getCamOptions().toString())
                                .put("anprOptions", "ms"));  // anprOptionsString 사용

                        return emit("unwatchEvents", camOptionsArray);
                    } catch (JSONException e) {
                        return CompletableFuture.failedFuture(e);
                    }
                })
                .thenAccept(result -> printJson("@unwatchEvents=", result))
                .exceptionally(e -> {
                    System.err.println("Error in unwatchEvents: " + e.getMessage());
                    return null;
                });
    }

    private static CompletableFuture<Void> disconnect() {
        return pause("8. 접속 종료")
                .thenCompose(v -> {
                    CompletableFuture<Void> disconnectFuture = new CompletableFuture<>();
                    socket.on(Socket.EVENT_DISCONNECT, args -> {
                        System.out.println("Disconnected from " + TSCAM);
                        disconnectFuture.complete(null);
                    });
                    reconnectAlways = false;
                    if (socket != null) {
                        socket.disconnect();
                    }
                    return disconnectFuture;
                });
    }

    private static CompletableFuture<Void> pause(String message) {
        return pause(message, false);
    }

    private static CompletableFuture<Void> pause(String message, boolean isExit) {
        System.out.println("---------------------------------------------------------------");
        System.out.println(message);
        if (isExit)
            System.out.println("Press Enter to exit...");
        else
            System.out.println("Press Enter to continue...");
        return CompletableFuture.runAsync(() -> new Scanner(System.in).nextLine());
    }

    private static CompletableFuture<JSONObject> emit(String event, Object data) {
        CompletableFuture<JSONObject> future = new CompletableFuture<>();
        socket.emit(event, data, (Ack) args -> {
            if (args.length > 0 && args[0] instanceof JSONObject) {
                future.complete((JSONObject) args[0]);
            } else {
                future.completeExceptionally(new RuntimeException("Invalid response"));
            }
        });
        return future;
    }

    private static void printJson(String title, JSONObject data) {
        try {
            System.out.println(title + data.toString(4));
        } catch (JSONException e) {
            System.err.println("Error printing JSON: " + e.getMessage());
            System.out.println(title + data.toString()); // 기본 toString() 메서드 사용
        }
    }

    private static JSONObject getCamOptions() {
        try {
            return new JSONObject()
                    .put("href", "http://192.168.0.30/onvif/device_service")
                    .put("alias", "주차장입구")
                    .put("username", "admin")
                    .put("password", "admin")
                    .put("authType", "basic");
        } catch (JSONException e) {
            System.err.println("Error creating camera options: " + e.getMessage());
            return new JSONObject(); // 빈 JSONObject 반환
        }
    }

}
