"""
The MIT License (MIT)
Copyright © 2022-2025 TS-Solution Corp.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to all conditions.

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""

import socketio
import json
import sys
import threading
import time
from typing import Optional, Dict, Any

# Socket.IO client
sio = socketio.Client()

# Configuration
TSCAM = "http://localhost:10000"
ANPR_OPTIONS = "ms"  # Vehicle number recognition options

# Camera configuration
CAMERA_CONFIG = {
    "href": "http://192.168.0.30/onvif/device_service",  # Camera URI
    "alias": "Entrance",  # Camera name
    "username": "admin",  # Camera login ID
    "password": "admin",  # Camera login password
    "authType": "basic"    # or "digest" (specifies camera login method, defaults to basic if omitted)
}

reconnect_always = True

watchlist_response = {}

def pause(message: str = "", is_exit: bool = False):
    """
    Pause execution and prompt the user to continue or exit.
    """
    print("-" * 60)
    if message:
        print(message)
    print("Press Enter to exit..." if is_exit else "Press Enter to continue...")
    input()
    print()

def emit_ack(event: str, data: Any = None, timeout: int = 10000) -> Optional[Dict]:
    """
    Emit an event and wait for acknowledgment using a callback and threading.Event.
    Returns the response data or None if failed or timed out.
    """
    from threading import Event
    response_data = None
    response_event = Event()
    def callback(*args):
        nonlocal response_data
        try:
            response_data = args[0] if args else {}
            if not isinstance(response_data, dict):
                response_data = {"result": False, "message": f"Unexpected response type: {type(response_data)}"}
        except Exception as e:
            response_data = {"result": False, "message": f"Error in callback: {str(e)}"}
        response_event.set()
    try:
        if data is not None:
            sio.emit(event, data=data, callback=callback)
        else:
            sio.emit(event, callback=callback)
        response_received = response_event.wait(timeout / 1000.0)
        if not response_received:
            print(f"Timeout ({timeout/1000.0:.1f}s) waiting for response from {event}")
            return {"result": False, "message": f"Timeout waiting for response from {event}"}
        return response_data
    except Exception as e:
        print(f"Error in emit_ack for {event}: {str(e)}")
        return {"result": False, "message": str(e)}

@sio.event
def connect():
    """
    Called when connected to the server.
    """
    print(f"Connected to {TSCAM}")

@sio.event
def disconnect():
    """
    Called when disconnected from the server.
    """
    print("Disconnected from server")

@sio.on('@event')
def on_event(data):
    """
    Handle camera events. Print snapshot and ANPR results if available.
    If a license plate is recognized, send a relayOutput command to open the barrier.
    """
    print(f"\nReceived '@event':\n{json.dumps(data, indent=2, ensure_ascii=False)}")
    href = data.get("href")
    alias = data.get("alias")
    events = data.get("events", [])
    for item in events:
        # Extract event details
        timestamp = item.get("timestamp")  # Event timestamp
        event_type = item.get("type")      # Event type (e.g., "digitalInput")
        port_no = item.get("portNo")       # Digital input port number
        value = item.get("value")          # Input value
        snapshot = item.get("snapshot")    # Snapshot image
        anpr = item.get("anpr")            # ANPR result
        if snapshot and anpr:
            print("\nSnapshot image:")
            print(f"File path: {json.dumps(snapshot, ensure_ascii=False)}")
            print(f"Download URI: {json.dumps(snapshot, ensure_ascii=False)}")
            print("\nANPR result:")
            print(f"Plate number: {json.dumps(anpr, ensure_ascii=False)}")
            # ANPR result may be an array (multiple plates)
            plates = anpr if isinstance(anpr, list) else [anpr]
            for license_plate in plates:
                text = license_plate.get("text")
                if text:
                    # Print recognized plate number
                    # print(f"PlateNo: {text}")
                    # Prepare relay output command to open the barrier
                    cam_options = {
                        "href": href,            # Event camera
                        "alias": alias,          # Camera name
                        "username": "admin",     # Camera login ID
                        "password": "admin",     # Camera login password
                        "authType": "basic",     # or "digest"
                        "portNo": 0,             # Output port number starts from 0
                        "value": 1               # 1:ON, 0:OFF
                    }
                    # Emit relayOutput asynchronously for real-time processing
                    sio.emit("relayOutput", cam_options)

@sio.on('@relayOutput')
def on_relay_output(data):
    """
    Handle asynchronous response for relayOutput command.
    """
    print("\nReceived '@relayOutput':")
    print(json.dumps(data, indent=2, ensure_ascii=False))

@sio.on('@watchList')
def on_watchlist(data):
    """
    Handle @watchList event response and store it for main flow.
    """
    # print("\nReceived '@watchList':")
    # print(json.dumps(data, indent=2, ensure_ascii=False))
    global watchlist_response
    watchlist_response = data

def main():
    """
    Main function to run the example step by step, similar to the C# sample.
    """
    global reconnect_always, watchlist_response
    try:
        print(f"Connecting to {TSCAM}...")
        sio.connect(TSCAM, wait_timeout=10)

        # 1. Discover cameras in the internal network
        pause("1. Discovering cameras in the internal network")
        result = emit_ack("discover", {"timeout": 2000, "device": "Ethernet"})
        if not result or not result.get('result'):
            print("Failed to discover cameras"); return
        print(f"@discover = {json.dumps(result, indent=2, ensure_ascii=False)}")
        cam_options = CAMERA_CONFIG.copy()
        
        # 2. Read first camera info (camera login required)
        pause("2. Reading first camera info (camera login required)")
        result = emit_ack("info", cam_options)
        if not result or not result.get('result'):
            print("Failed to get camera info"); return
        print(f"@info = {json.dumps(result, indent=2, ensure_ascii=False)}")
        
        # 3. Request snapshot image (with vehicle number recognition)
        pause("3. Requesting snapshot image (with vehicle number recognition)")
        cam_options2 = cam_options.copy()
        cam_options2["anprOptions"] = ANPR_OPTIONS
        result = emit_ack("snapshot", cam_options2)
        if not result or not result.get('result'):
            print("Failed to take snapshot"); return
        print(f"@snapshot = {json.dumps(result, indent=2, ensure_ascii=False)}")
        
        # 4. Relay output
        pause("4. Relay output")
        cam_options3 = cam_options.copy()
        cam_options3["portNo"] = 0
        cam_options3["value"] = 1
        result = emit_ack("relayOutput", cam_options3)
        if not result or not result.get('result'):
            print("Failed to control relay"); return
        print(f"@relayOutput = {json.dumps(result, indent=2, ensure_ascii=False)}")
        
        # 5. Watch events
        pause("5. Waiting for events (events are received through camera event listeners)")
        cam_options4 = cam_options.copy()
        cam_options4["anprOptions"] = ANPR_OPTIONS
        #print("watchEvents param:", [cam_options4])  # Debug: show parameters
        result = emit_ack("watchEvents", [cam_options4])
        if not result or not result.get('result'):
            print("Failed to watch events")
            print("watchEvents response:", json.dumps(result, indent=2, ensure_ascii=False))
            return
        print(f"@watchEvents = {json.dumps(result, indent=2, ensure_ascii=False)}")
        
        # 6. Get event watch list
        pause("6. Waiting for event watch list")
        watchlist_response = {}
        sio.emit("watchList")
        for _ in range(100):
            if watchlist_response:
                break
            time.sleep(0.1)
        if not watchlist_response:
            print("Timeout waiting for @watchList event")
        else:
            print("@watchList =", json.dumps(watchlist_response, indent=2, ensure_ascii=False))

        print("-" * 60)
        print("Waiting for events... (Trigger digital input on camera)")
        print("Press any key to exit...")
        input()
        print()

        # 7. Stop event reception
        # (Optional: Unsubscribe from events and disconnect)
        pause("7. Stop event reception")
        result = emit_ack("unwatchEvents", [cam_options4])
        if result:
            print(f"@unwatchEvents = {json.dumps(result, indent=2, ensure_ascii=False)}")
        
        pause("8. Disconnect")
        sio.disconnect()
    except Exception as e:
        print(f"Error: {str(e)}")
        import traceback
        traceback.print_exc()
    finally:
        try:
            sio.disconnect()
        except:
            pass

if __name__ == "__main__":
    main()
