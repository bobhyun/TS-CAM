/*
  socket.io 클라이언트를 사용하기 위한 의존성 설치
    npm i socket.io-client

  콘솔 입력 유틸리티를 위한 의존성 설치
    npm i readline

  @author: https://github.com/bobhyun/TS-ANPR
  Copyright (C) 2024. All rights reserved.
*/

const io = require('socket.io-client')
const readline = require('readline')
const util = require('util')

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
})

// 예제에서 순차적 흐름으로 처리하기 위한 함수 (실제로는 이런 구성이 불필요함)
async function pause(message, callback, isExit = false) {
  if (message) {
    console.log(
      '---------------------------------------------------------------'
    )
    console.log(message)
  }

  if (isExit) {
    console.log('Press any key to exit...')
  } else {
    console.log('Press any key to continue...')
  }

  return await new Promise(resolve => {
    rl.question('', answer => {
      function onNext(doExitImmediately = false) {
        resolve()
        if (doExitImmediately) process.exit()
      }

      //callback 함수에서 resolve를 호출하면 다음 단계로 남어가도록 구성함
      if (callback) callback(onNext)
      else onNext()
    })
  })
}

function printJson(title, json) {
  console.log(
    title,
    util.inspect(json, {
      showHidden: false,
      depth: null,
      colors: true
    })
  )
}

const tscam = 'http://localhost:10000'
let reconnectAlways = true
// 차번인식 옵션
// [참고] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
const anprOptionsString = 'ms' // 용도에 맞게 설정

let camOptions = {
  href: 'http://192.168.0.30/onvif/device_service',
  alias: '주차장입구', // 이름 지정
  username: 'admin', // 카메라 로그인 ID
  password: 'admin', // 카메라 로그인 Password
  authType: 'basic' // 또는 'digest' (카메라에 로그인 방식 지정, 생략하면 basic을 의미함)
}

const client = io(tscam)
client
  .on('disconnect', reason => {
    console.log(`Disconnected from ${tscam}`)
    if (reconnectAlways) {
      console.log(`Attempting to reconnect...`)
      client.connect()
    }
  })
  .on('connect', async () => {
    console.log(`Connected to ${tscam}`)

    let camList = []

    await pause('1. 내부망에 연결된 카메라 탐색', next => {
      client.emit(
        'discover',
        {
          timeout: 2000, // 카메라 응답 대기 시간 (ms)
          device: 'Ethernet' // 또는 'Wi-Fi' (이 항목을 지정 안하면 알아서 처리함)
        },
        result => {
          printJson('@discover=', result)
          if (!result.result) return next(true)

          camList = result.devices
          //camOptions.href = camList[0].href // 첫번째 카메라 URI
          next()
        }
      )
    })

    await pause('2. 첫번째 카메라 정보 읽기 (카메라 로그인 필요)', next => {
      client.emit('info', camOptions, result => {
        printJson('@info=', result)
        next()
      })
    })

    await pause('3. 스냅샷 이미지 요청 (with 차번인식)', next => {
      client.emit(
        'snapshot',
        {
          ...camOptions,

          // 이 항목이 없으면 스냅샷 이미지만 받음
          // [주의]
          // anprOptions: '' 이렇게 하면 옵션없이 차량번호인식하라는 의미임
          // 차량번호 인식을 하지 않으려면 anprOptions 항목을 삭제해야 함
          anprOptions: anprOptionsString
        },
        result => {
          printJson('@snapshot=', result)
          next()
        }
      )
    })

    await pause('4. 릴레이 출력', next => {
      client.emit(
        'relayOutput',
        {
          ...camOptions,
          portNo: 0, // 출력 포트번호는 0부터 시작함
          value: 1 // 1:ON, 0:OFF
        },
        result => {
          printJson('@relayOutput=', result)
          next()
        }
      )
    })

    let camOptionsArray = [
      // 첫번째 카메라
      {
        ...camOptions,

        // 이 항목이 없으면 스냅샷 이미지만 받음
        // [주의]
        // anprOptions: '' 이렇게 하면 옵션없이 차량번호인식하라는 의미임
        // 차량번호 인식을 하지 않으려면 anprOptions 항목을 삭제해야 함
        anprOptions: anprOptionsString
      }

      /*
      // 두번째 카메라
      , {
        //href: camList[1].href, // 두번째 카메라 URI
        href: 'http://192.168.0.31/onvif/device_service',
        alias: '주차장출구',    // 이름 지정
        username: 'admin',     // 카메라 로그인 ID
        password: 'admin',     // 카메라 로그인 Password
        authType: 'basic',     // 또는 'digest' (카메라에 로그인 방식 지정, 생략하면 basic을 의미함),
        anprOptions: anprOptionsString
      }
      */
    ]
    await pause(
      '5. 이벤트 수신 대기 (이벤트는 카메라 이벤트 리스너를 통해 수신)',
      next => {
        client.emit('watchEvents', camOptionsArray, result => {
          printJson('@watchEvents=', result)
          next()
        })
      }
    )

    await pause('6. 이벤트 수신 대기 목록', next => {
      client.emit('watchList', null, result => {
        printJson('@watchList=', result)
        next()
      })
    })

    await pause(
      '이벤트 수신 대기중... (카메라 Digital Input으로 입력을 넣으세요.)',
      null,
      true
    )

    reconnectAlways = false

    await pause('7. 이벤트 수신 종료', next => {
      client.emit('unwatchEvents', camOptionsArray, result => {
        printJson('@unwatchEvents=', result)
        next()
      })
    })

    await pause(
      '8. 접속 종료',
      next => {
        client.disconnect()
        next(true)
      },
      true
    )
  })
  .on('@event', data => {
    console.log('@event=', data)
  })
