package com.example.tscam

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

import io.socket.client.IO
import io.socket.client.Socket
import io.socket.client.Ack
import org.json.JSONException
import org.json.JSONArray
import org.json.JSONObject
import java.net.URISyntaxException
import java.util.concurrent.CompletableFuture
import java.io.BufferedReader
import java.io.InputStreamReader
import kotlin.system.exitProcess

class Main {
    companion object {
        private const val TSCAM = "http://localhost:10000"
        private var reconnectAlways = true
        private const val ANPR_OPTIONS_STRING = "ms"
        private lateinit var socket: Socket

        @JvmStatic
        @Throws(URISyntaxException::class)
        fun main(args: Array<String>) {
            socket = IO.socket(TSCAM)
            val connectionFuture = CompletableFuture<Void>()
            setupSocketListeners(connectionFuture)
            socket.connect()

            connectionFuture.thenCompose { runMainLogic() }
                .exceptionally { e ->
                    System.err.println("Error in main logic: ${e.message}")
                    null
                }
                .thenRun {
                    println("All operations completed. Disconnecting...")
                    exitProcess(0)
                }
        }

        private fun setupSocketListeners(connectionFuture: CompletableFuture<Void>) {
            socket.on(Socket.EVENT_CONNECT) {
                println("Connected to server: $TSCAM")
                connectionFuture.complete(null)
            }

            socket.on(Socket.EVENT_CONNECT_ERROR) {
                connectionFuture.completeExceptionally(RuntimeException("Connection failed"))
            }

            // Add event listener to receive events through the camera event listener
            socket.on("@event") { args ->
                val data = args[0] as JSONObject
                printJson("@event=", data)
            }
        }

        private fun runMainLogic(): CompletableFuture<Void> {
            return discoverCameras()
                .thenCompose { getFirstCameraInfo() }
                .thenCompose { requestSnapshot() }
                .thenCompose { controlRelayOutput() }
                .thenCompose { watchEvents() }
                .thenCompose { getWatchList() }
                .thenCompose { waitForEvents() }
                .thenCompose { unwatchEvents() }
                .thenCompose { disconnect() }
        }

        private fun discoverCameras(): CompletableFuture<Void> {
            return pause("1. Discovering cameras in the network")
                .thenCompose {
                    try {
                        val params = JSONObject().apply {
                            put("timeout", 2000)
                            put("device", "Ethernet")
                        }
                        emit("discover", params)
                    } catch (e: JSONException) {
                        CompletableFuture.failedFuture(e)
                    }
                }
                .thenAccept { result -> printJson("@discover=", result) }
                .exceptionally { e ->
                    System.err.println("Error in discoverCameras: ${e.message}")
                    null
                }
        }

        private fun getFirstCameraInfo(): CompletableFuture<Void> {
            return pause("2. Getting first camera info (camera login required)")
                .thenCompose { emit("info", getCamOptions()) }
                .thenAccept { result -> printJson("@info=", result) }
        }

        private fun requestSnapshot(): CompletableFuture<Void> {
            return pause("3. Requesting snapshot image (with vehicle number recognition)")
                .thenCompose {
                    try {
                        val requestData = JSONObject(getCamOptions().toString()).apply {
                            put("anprOptions", "ms") // anprOptionsString used
                        }
                        emit("snapshot", requestData)
                    } catch (e: JSONException) {
                        CompletableFuture.failedFuture(e)
                    }
                }
                .thenAccept { result -> printJson("@snapshot=", result) }
                .exceptionally { e ->
                    System.err.println("Error in requestSnapshot: ${e.message}")
                    null
                }
        }

        private fun controlRelayOutput(): CompletableFuture<Void> {
            return pause("4. Controlling relay output")
                .thenCompose {
                    try {
                        val requestData = JSONObject(getCamOptions().toString()).apply {
                            put("portNo", 0)  // Output port number starts from 0
                            put("value", 1)   // 1: ON, 0: OFF
                        }
                        emit("relayOutput", requestData)
                    } catch (e: JSONException) {
                        CompletableFuture.failedFuture(e)
                    }
                }
                .thenAccept { result -> printJson("@relayOutput=", result) }
                .exceptionally { e ->
                    System.err.println("Error in controlRelayOutput: ${e.message}")
                    null
                }
        }

        private fun watchEvents(): CompletableFuture<Void> {
            return pause("5. Waiting for events (events are received through the camera event listener)")
                .thenCompose {
                    try {
                        val camOptionsArray = JSONArray().apply {
                            put(JSONObject(getCamOptions().toString()).apply {
                                put("anprOptions", "ms")  // anprOptionsString used
                            })
                        }
                        emit("watchEvents", camOptionsArray)
                    } catch (e: JSONException) {
                        CompletableFuture.failedFuture(e)
                    }
                }
                .thenAccept { result -> printJson("@watchEvents=", result) }
                .exceptionally { e ->
                    System.err.println("Error in watchEvents: ${e.message}")
                    null
                }
        }

        private fun getWatchList(): CompletableFuture<Void> {
            return pause("6. Getting event watch list")
                .thenCompose { emit("watchList", null) }
                .thenAccept { result -> printJson("@watchList=", result) }
        }

        private fun waitForEvents(): CompletableFuture<Void> {
            return pause("Waiting for events... (Please input a digital input on the camera.)", true)
        }

        private fun unwatchEvents(): CompletableFuture<Void> {
            return pause("7. Stopping event watching")
                .thenCompose {
                    try {
                        val camOptionsArray = JSONArray().apply {
                            put(JSONObject(getCamOptions().toString()).apply {
                                put("anprOptions", "ms")  // anprOptionsString used
                            })
                        }
                        emit("unwatchEvents", camOptionsArray)
                    } catch (e: JSONException) {
                        CompletableFuture.failedFuture(e)
                    }
                }
                .thenAccept { result -> printJson("@unwatchEvents=", result) }
                .exceptionally { e ->
                    System.err.println("Error in unwatchEvents: ${e.message}")
                    null
                }
        }

        private fun disconnect(): CompletableFuture<Void> {
            return pause("8. Disconnecting")
                .thenCompose {
                    val disconnectFuture = CompletableFuture<Void>()
                    socket.on(Socket.EVENT_DISCONNECT) {
                        println("Disconnected from $TSCAM")
                        disconnectFuture.complete(null)
                    }
                    reconnectAlways = false
                    socket.disconnect()
                    disconnectFuture
                }
        }

        private fun pause(message: String): CompletableFuture<Void> {
            return pause(message, false)
        }

        private val reader = BufferedReader(InputStreamReader(System.`in`))

        private fun pause(message: String, isExit: Boolean): CompletableFuture<Void> {
            println("---------------------------------------------------------------")
            println(message)
            if (isExit) {
                println("Press Enter to exit...")
            } else {
                println("Press Enter to continue...")
            }
            
            val future = CompletableFuture<Void>()
            Thread {
                try {
                    reader.readLine()
                    future.complete(null)
                } catch (e: Exception) {
                    println("Input error: ${e.message}")
                    future.complete(null)
                }
            }.start()
            
            return future
        }

        private fun emit(event: String, data: Any?): CompletableFuture<JSONObject> {
            val future = CompletableFuture<JSONObject>()
            socket.emit(event, data, Ack { args ->
                if (args.isNotEmpty() && args[0] is JSONObject) {
                    future.complete(args[0] as JSONObject)
                } else {
                    future.completeExceptionally(RuntimeException("Invalid response"))
                }
            })
            return future
        }

        private fun printJson(title: String, data: JSONObject) {
            try {
                println(title + data.toString(4))
            } catch (e: JSONException) {
                System.err.println("Error printing JSON: ${e.message}")
                println(title + data.toString()) // Use default toString() method
            }
        }

        private fun getCamOptions(): JSONObject {
            return try {
                JSONObject().apply {
                    put("href", "http://192.168.0.30/onvif/device_service")
                    put("alias", "Parking entrance")
                    put("username", "admin")
                    put("password", "admin")
                    put("authType", "basic")
                }
            } catch (e: JSONException) {
                System.err.println("Error creating camera options: ${e.message}")
                JSONObject() // Return empty JSONObject
            }
        }
    }
}