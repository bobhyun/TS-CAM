#!/bin/bash

# tscam Service Registration Script

echo "# tscam configuration

[Unit]
Description=tscam (ONVIF Camera Broker)
After=network.target

[Service]
# HTTP port number
# Environment=\"TSCAM_HTTP_PORT=10000\"

# Data storage directory
# Environment=\"TSCAM_DATA_DIR=/var/tscam/data\"

# Log file directory
# Environment=\"TSCAM_LOG_DIR=/var/tscam/log\"

# Remove milliseconds
# Environment=\"TSCAM_NO_MILLISECONDS=1\"

# URI prefix for saved image file downloads
# Environment=\"TSCAM_URI_DATA_PATH_PREFIX=/site1\"

# Console log level (info, warn, error)
# Environment=\"TSCAM_LOG_LEVEL_CONSOLE=error\"

# File log level (info, warn, error)
# Environment=\"TSCAM_LOG_LEVEL_FILE=error\"

# Log file configuration
#     maxSize: Maximum size of a single log file
#     maxFiles: Number of log files to keep (automatically deleted after specified days)
#     size: Maximum size of total log storage (oldest files are deleted when total size exceeds this value)
# Environment=\"TSCAM_LOG_CONFIG={\\\"maxSize\\\":\\\"20m\\\",\\\"maxFiles\\\":\\\"31d\\\",\\\"size\\\":\\\"1024m\\\"}\"

# License plate recognition engine file path
# Environment=\"TSANPR=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/libtsanpr.so\"

# TSANPR initialization params
# Environment=\"TSANPR_COUNTRY=KR\"
# Environment=\"TSANPR_MIN_CHAR=4\"
# Environment=\"TSANPR_SYMBOL=full\"
   
# Required items
WorkingDirectory=/var/tscam

# tscam executable path
#ExecStart=/var/tsanpr/tsanpr-v3.0.0M/linux-x86_64/tscam

# Auto restart after 3 seconds if process dies
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