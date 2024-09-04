"""
    # asyncio 기반의 socket.io 비동기 클라이언트를 사용하기 위한 의존성 설치
    pip install "python-socketio[asyncio_client]"

    # 콘솔 입력 유틸리티를 위한 의존성 설치
    pip install aioconsole

    Copyright (C) 2024. All rights reserved.
    @author: https://github.com/bobhyun/TS-ANPR
"""

import asyncio
import socketio
from aioconsole import ainput
import json
import sys
import signal

async def pause(message, isExit = False):
    if message:
        print('---------------------------------------------------------------')
        print(message)

    if isExit:
        print('Press any key to exit...')
    else:
        print('Press any key to continue...')
    await ainput()
    
def printJson(title, data):
    print(title, json.dumps(data, indent=4, ensure_ascii=False))


tscam = 'http://localhost:10000'
reconnectAlways = True
# 차번인식 옵션
# [참고] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
anprOptionsString = 'ms' # 용도에 맞게 설정

camOptions = {
  'href': 'http://192.168.0.30/onvif/device_service',
  'alias': '주차장입구', # 이름 지정
  'username': 'admin',  # 카메라 로그인 ID
  'password': 'admin',  # 카메라 로그인 Password
  'authType': 'basic'   # 또는 'digest' (카메라에 로그인 방식 지정, 생략하면 basic을 의미함)
}

client = socketio.AsyncClient()

@client.event
def disconnect():
    print('Disconnected from', tscam)
    if reconnectAlways:
        print('Attempting to reconnect...')
        client.connect(tscam)

@client.event
def connect():
    print('Connected to', tscam)

@client.on('@event')
def onEvent(data):
    printJson('@event=', data)


async def main():
    await client.connect(tscam)

    camList = []

    await pause('1. 내부망에 연결된 카메라 탐색')
    result = await client.call('discover', {
        'timeout': 2000,
        'device': 'Ethernet'
    })    
    printJson('@discover=', result)
    if result.get('result'):
        camList = result.get('devices', [])


    await pause('2. 첫번째 카메라 정보 읽기 (카메라 로그인 필요)')
    result = await client.call('info', camOptions)
    printJson('@info=', result)


    await pause('3. 스냅샷 이미지 요청 (with 차번인식)')
    result = await client.call('snapshot',
        {
          **camOptions,

          # 이 항목이 없으면 스냅샷 이미지만 받음
          # [주의]
          # anprOptions: '' 이렇게 하면 옵션없이 차량번호인식하라는 의미임
          # 차량번호 인식을 하지 않으려면 anprOptions 항목을 삭제해야 함
          'anprOptions': anprOptionsString
        })    
    printJson('@snapshot=', result)
    

    await pause('4. 릴레이 출력')
    result = await client.call('relayOutput',
        {
          **camOptions,
          'portNo': 0,  # 출력 포트번호는 0부터 시작함
          'value': 1    # 1:ON, 0:OFF
        })
    printJson('@relayOutput=', result)


    await pause('5. 이벤트 수신 대기 (이벤트는 카메라 이벤트 리스너를 통해 수신)')
    camOptionsArray = [
      # 첫번째 카메라
      {
        **camOptions,

        # 이 항목이 없으면 스냅샷 이미지만 받음
        # [주의]
        # anprOptions: '' 이렇게 하면 옵션없이 차량번호인식하라는 의미임
        # 차량번호 인식을 하지 않으려면 anprOptions 항목을 삭제해야 함
        'anprOptions': anprOptionsString
      }
      
      # 두번째 카메라
      #, {
      #  #'href': camList[1].href,  # 두번째 카메라 URI
      #  'href': 'http://192.168.0.31/onvif/device_service',
      #  'alias': '주차장출구',       # 이름 지정
      #  'username': 'admin',        # 카메라 로그인 ID
      #  'password': 'admin',        # 카메라 로그인 Password
      #  'authType': 'basic',        # 또는 'digest' (카메라에 로그인 방식 지정, 생략하면 basic을 의미함),
      #  'anprOptions': anprOptionsString
      #}      
    ]
    result = await client.call('watchEvents', camOptionsArray)
    printJson('@watchEvents=', result)


    await pause('6. 이벤트 수신 대기 목록')
    result = await client.call('watchList', '')
    printJson('@watchList=', result)


    await pause('이벤트 수신 대기중... (카메라 Digital Input으로 입력을 넣으세요.)', True)
    
    global reconnectAlways
    reconnectAlways = False

    await pause('7. 이벤트 수신 종료')
    result = await client.call('unwatchEvents', camOptionsArray)
    printJson('@unwatchEvents=', result)
    

    await pause('8. 접속 종료')
    await quit()

async def quit():
    global reconnectAlways
    reconnectAlways = False
    try:
        await client.disconnect()
        print('Disconnect called')
    except Exception as e:
        print(f'Error during disconnect: {e}')

def signal_handler(sig, frame):
    quit()
    sys.exit(0)

# Ctrl+C 시그널 핸들러 등록
signal.signal(signal.SIGINT, signal_handler)



if __name__ == "__main__":
    reconnectAlways = True
    asyncio.run(main())