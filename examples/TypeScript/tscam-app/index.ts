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

import { io, Socket } from 'socket.io-client';
import readline from 'readline';
import util from 'util';

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

// Function to handle sequential flow in the example (this structure is actually unnecessary)
async function pause(message?: string, callback?: (next: (doExitImmediately?: boolean) => void) => void, isExit = false): Promise<void> {
    if (message) {
        console.log('---------------------------------------------------------------');
        console.log(message);
    }

    if (isExit) {
        console.log('Press any key to exit...');
    } else {
        console.log('Press any key to continue...');
    }

    return await new Promise<void>(resolve => {
        rl.question('', answer => {
            function onNext(doExitImmediately = false) {
                resolve();
                if (doExitImmediately) process.exit();
            }
            //When the callback function calls resolve, it proceeds to the next step
            if (callback) callback(onNext);
            else onNext();
        });
    });
}

function printJson(title: string, json: any) {
    console.log(
        title,
        util.inspect(json, {
            showHidden: false,
            depth: null,
            colors: true
        })
    );
}

const tscam = 'http://localhost:10000';
let reconnectAlways = true;
// License plate recognition options
// [Reference] https://github.com/bobhyun/TS-ANPR/blob/main/DevGuide.md#12-anpr_read_file
const anprOptionsString = 'ms'; // Set as needed

interface CamOptions {
    href: string;
    alias: string;
    username: string;
    password: string;
    authType: 'basic' | 'digest';
    anprOptions?: string;
    portNo?: number;
    value?: number;
}

let camOptions: CamOptions = {
    href: 'http://192.168.0.30/onvif/device_service',
    alias: '주차장입구', // Camera name
    username: 'admin', // Camera login ID
    password: 'admin', // Camera login Password
    authType: 'basic' // or 'digest' (if omitted, means basic)
};

const client: Socket = io(tscam);
client
    .on('disconnect', (reason: string) => {
        console.log(`Disconnected from ${tscam}`);
        if (reconnectAlways) {
            console.log(`Attempting to reconnect...`);
            client.connect();
        }
    })
    .on('connect', async () => {
        console.log(`Connected to ${tscam}`);

        let camList: any[] = [];

        await pause('1. Discover cameras on the internal network', next => {
            client.emit(
                'discover',
                {
                    timeout: 2000, // Camera response wait time (ms)
                    device: 'Ethernet' // or 'Wi-Fi' (if omitted, handled automatically)
                },
                (result: any) => {
                    printJson('@discover=', result);
                    if (!result.result) return next(true);

                    camList = result.devices;
                    //camOptions.href = camList[0].href; // First camera URI
                    next();
                }
            );
        });

        await pause('2. Read information from the first camera (camera login required)', next => {
            client.emit('info', camOptions, (result: any) => {
                printJson('@info=', result);
                next();
            });
        });

        await pause('3. Request snapshot image (with license plate recognition)', next => {
            client.emit(
                'snapshot',
                {
                    ...camOptions,
                    // If this field is missing, only the snapshot image is received
                    // [Caution]
                    // anprOptions: '' means recognize license plate without options
                    // To not recognize license plate, remove the anprOptions field
                    anprOptions: anprOptionsString
                },
                (result: any) => {
                    printJson('@snapshot=', result);
                    next();
                }
            );
        });

        await pause('4. Control relay output', next => {
            client.emit(
                'relayOutput',
                {
                    ...camOptions,
                    portNo: 0, // Output port number starts from 0
                    value: 1 // 1:ON, 0:OFF
                },
                (result: any) => {
                    printJson('@relayOutput=', result);
                    next();
                }
            );
        });

        let camOptionsArray: CamOptions[] = [
            {
                ...camOptions,
                // If this field is missing, only the snapshot image is received
                // [Caution]
                // anprOptions: '' means recognize license plate without options
                // To not recognize license plate, remove the anprOptions field
                anprOptions: anprOptionsString
            }
            /*
            // Second camera
            , {
              //href: camList[1].href, // Second camera URI
              href: 'http://192.168.0.31/onvif/device_service',
              alias: '주차장출구',    // Camera name
              username: 'admin',     // Camera login ID
              password: 'admin',     // Camera login Password
              authType: 'basic',     // or 'digest' (if omitted, means basic)
              anprOptions: anprOptionsString
            }
            */
        ];
        await pause(
            '5. Waiting for events (events are received through the camera event listener)',
            next => {
                client.emit('watchEvents', camOptionsArray, (result: any) => {
                    printJson('@watchEvents=', result);
                    next();
                });
            }
        );

        await pause('6. Getting the event watch list', next => {
            client.emit('watchList', null, (result: any) => {
                printJson('@watchList=', result);
                next();
            });
        });

        await pause(
            'Waiting for events... (Please input a digital input on the camera.)',
            undefined,
            true
        );

        reconnectAlways = false;

        await pause('7. Stopping event watching', next => {
            client.emit('unwatchEvents', camOptionsArray, (result: any) => {
                printJson('@unwatchEvents=', result);
                next();
            });
        });

        await pause(
            '8. Disconnecting',
            next => {
                client.disconnect();
                next(true);
            },
            true
        );
    })
    .on('@event', (data: any) => {
        console.log('@event=', data);
    }); 