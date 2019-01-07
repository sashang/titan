#!/bin/bash
# Use this to generate a new jwt secret for use in the appsettings.json file.
head -c 32 /dev/urandom | hexdump --format '"%x"'
