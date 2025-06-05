/**
 * The MIT License (MIT)
 * Copyright © 2022-2025 TS-Solution Corp.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to all conditions.
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

const io = require('socket.io-client')
const readline = require('readline')
const util = require('util')

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
})

// Function to handle sequential flow in the example (this structure is actually unnecessary)
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

      //When the callback function calls resolve, it proceeds to the next step
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
      '5. Waiting for events (events are received through the camera event listener)',
      next => {
        client.emit('watchEvents', camOptionsArray, result => {
          printJson('@watchEvents=', result)
          next()
        })
      }
    )

    await pause('6. Getting the event watch list', next => {
      client.emit('watchList', null, result => {
        printJson('@watchList=', result)
        next()
      })
    })

    await pause(
      'Waiting for events... (Please input a digital input on the camera.)',
      null,
      true
    )

    reconnectAlways = false

    await pause('7. Stopping event watching', next => {
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
