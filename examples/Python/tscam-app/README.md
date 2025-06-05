English | [한국어](doc.i18n/ko-KR/README.md) | [日本語](doc.i18n/ja-JP/README.md) | [Tiếng Việt](doc.i18n/vi-VN/README.md)

# TS-CAM Python Example

This example is a Python implementation of the TS-CAM Socket.IO client. It sequentially performs the following functions:

1. Discover cameras on the internal network
2. Read information from the first camera (camera login required)
3. Request snapshot image with license plate recognition options
4. Control relay output
5. Wait for events
6. Check event watch list
7. Stop event watching

The program waits for user input at each step and performs real-time camera control and event reception through Socket.IO server communication.

## Project Structure

```
tscam-app/
└── main.py
```

## Required Libraries

- python-socketio >=5.10.0
- Python >=3.9

## How to Run

1. Install dependencies:
   ```bash
   pip install python-socketio
   ```
2. Run the program:
   ```bash
   python main.py
   ```
