#!/bin/bash

# tscam 서비스 삭제 스크립트

sudo systemctl stop tscam.service
sudo systemctl disable tscam.service
sudo rm /etc/systemd/system/tscam.service
