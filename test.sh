#!/bin/bash
./build.sh
mono tools/Cake/Cake.exe build.cake -Target=test
