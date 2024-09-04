#!/bin/bash

# tscam 서비스 등록 스크립트

echo "# tscam configuration

[Unit]
Description=tscam (ONVIF Camera Broker)
After=network.target

[Service]
# HTTP 포트 번호
# Environment=\"TSCAM_HTTP_PORT=10000\"

# 데이터 저장 디렉토리
# Environment=\"TSCAM_DATA_DIR=/var/tscam/data\"

# 로그 파일 디렉토리
# Environment=\"TSCAM_LOG_DIR=/var/tscam/log\"

# 밀리초 단위 삭제
# Environment=\"TSCAM_NO_MILLISECONDS=1\"

# 저장 이미지 파일 다운로드 경로 prefix
# Environment=\"TSCAM_URI_DATA_PATH_PREFIX=/site1\"

# 콘솔 로그 레벨 (info, warn, error)
# Environment=\"TSCAM_LOG_LEVEL_CONSOLE=error\"

# 파일 로그 레벨 (info, warn, error)
# Environment=\"TSCAM_LOG_LEVEL_FILE=error\"

# 로그 파일 설정
# 	maxSize: 로그 파일 하나의 최대 크기
# 	maxFiles: 보관할 로그 파일 수 (지정한 날짜가 지나면 자동 삭제됨)
# 	size: 전체 로그 저장소의 최대 크기 (총 로그 크기가 설정한 값을 초과하면 가장 오래된 파일부터 삭제됨)  
# Environment=\"TSCAM_LOG_CONFIG={\\\"maxSize\\\":\\\"20m\\\",\\\"maxFiles\\\":\\\"31d\\\",\\\"size\\\":\\\"1024m\\\"}\"

# 차번인식엔진 파일 경로
# Environment=\"TSANPR=/var/tsanpr/tsanpr-KR-v2.4.0M/linux-x86_64/libtsanpr.so\"

# 필수 항목
WorkingDirectory=/var/tscam

# tscam 실행 파일 경로
#ExecStart=/var/tsanpr/tsanpr-KR-v2.4.0M/linux-x86_64/tscam

# 죽으면 3초 후 자동 재시작 
Restart=always
RestartSec=3
LimitNOFILE=400000
Type=simple

[Install]
WantedBy=multi-user.target
" > ~/.tmp.tscam.service
sudo mv ~/.tmp.tscam.service /etc/systemd/system/tscam.service

sudo systemctl daemon-reload
sudo systemctl enable tscam
sudo systemctl restart tscam