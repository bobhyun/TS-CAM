package com.example.tscam;

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

import io.socket.client.IO;
import io.socket.client.Socket;
import io.socket.client.Ack;
import org.json.JSONException;
import org.json.JSONArray;
import org.json.JSONObject;
import org.json.JSONArray;
import java.net.URISyntaxException;
import java.util.concurrent.CompletableFuture;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.IOException;

public class Main {
    private static final String TSCAM = "http://localhost:10000";
    private static boolean reconnectAlways = true;
    private static final String ANPR_OPTIONS_STRING = "ms";
    private static Socket socket;
    private static final BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));

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
            System.out.println("Connected to server: " + TSCAM);
            connectionFuture.complete(null);
        });

        socket.on(Socket.EVENT_CONNECT_ERROR, args -> {
            connectionFuture.completeExceptionally(new RuntimeException("Connection failed"));
        });

        // Add event listener to receive events through the camera event listener
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
        return pause("1. Discovering cameras in the network")
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
        return pause("2. Getting first camera info (camera login required)")
                .thenCompose(v -> emit("info", getCamOptions()))
                .thenAccept(result -> printJson("@info=", result));
    }

    // Implement other methods (requestSnapshot, controlRelayOutput, watchEvents, etc.) similarly
    private static CompletableFuture<Void> requestSnapshot() {
        return pause("3. Requesting snapshot image (with vehicle number recognition)")
                .thenCompose(v -> {
                    try {
                        JSONObject requestData = new JSONObject(getCamOptions().toString());
                        requestData.put("anprOptions", "ms"); // anprOptionsString used

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
        return pause("4. Controlling relay output")
                .thenCompose(v -> {
                    try {
                        JSONObject requestData = new JSONObject(getCamOptions().toString());
                        requestData.put("portNo", 0);  // Output port number starts from 0
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
        return pause("5. Waiting for events (events are received through the camera event listener)")
                .thenCompose(v -> {
                    try {
                        JSONArray camOptionsArray = new JSONArray();
                        camOptionsArray.put(new JSONObject(getCamOptions().toString())
                                .put("anprOptions", "ms"));  // anprOptionsString used

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
        return pause("6. Getting event watch list")
                .thenCompose(v -> emit("watchList", null))
                .thenAccept(result -> printJson("@watchList=", result));
    }

    private static CompletableFuture<Void> waitForEvents() {
        return pause("Waiting for events... (Please input a digital input on the camera.)", true);
    }

    private static CompletableFuture<Void> unwatchEvents() {
        return pause("7. Stopping event watching")
                .thenCompose(v -> {
                    try {
                        JSONArray camOptionsArray = new JSONArray();
                        camOptionsArray.put(new JSONObject(getCamOptions().toString())
                                .put("anprOptions", "ms"));  // anprOptionsString used

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
        return pause("8. Disconnecting")
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
        
        return CompletableFuture.runAsync(() -> {
            try {
                reader.readLine();
            } catch (IOException e) {
                System.err.println("Input error: " + e.getMessage());
            }
        });
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
            System.out.println(title + data.toString()); // Use default toString() method
        }
    }

    private static JSONObject getCamOptions() {
        try {
            return new JSONObject()
                    .put("href", "http://192.168.0.30/onvif/device_service")
                    .put("alias", "Parking entrance")
                    .put("username", "admin")
                    .put("password", "admin")
                    .put("authType", "basic");
        } catch (JSONException e) {
            System.err.println("Error creating camera options: " + e.getMessage());
            return new JSONObject(); // Return empty JSONObject
        }
    }

}