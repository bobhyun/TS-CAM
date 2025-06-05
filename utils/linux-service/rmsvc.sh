#!/bin/bash

# tscam Service Removal Script

sudo systemctl stop tscam.service
sudo systemctl disable tscam.service
sudo rm /etc/systemd/system/tscam.service
