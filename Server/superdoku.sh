#!/bin/sh
command=$1
pid_file="superdoku.pid"

if [ -z $command ]; then
    echo "ERROR: Missing required positional argument. First argument should specify if the server should be started or stopped (start|stop)"
    exit 1
fi

function CheckServerStatus()
{
    # Check if the server is already running
    local pid=$(head -1 $pid_file)
    echo "Checking pid: $pid"
    server_status=$(/bin/ps -p $pid -o command | /bin/grep superdoku-api.py)
    server_status=$(echo $?)

    # Server is already running if the result is 0
    if [[ $server_status == "0" ]]; then
        server_status="running"
    else
        server_status="stopped"
    fi
}

# Determine python path (must have python 3.6+ installed)
py_version_regex="[p|P]ython3[6-9]*"
py_exec=$(which python3)

if [[ $py_exec =~ $py_version_regex ]]; then
    echo "Python3 executable: $py_exec"
else
    # Attempt to grab from default python executable name
    py_exec=$(which python)

    if [[ $py_exec =~ $py_version_regex ]]; then
        echo "Python3 executable: $py_exec"
    else
        echo "ERROR: Could not find python3 installation. Please verify that Python 3.6+ is installed and added to system path environment variable."
        exit 1
    fi
fi

# Convert command casing to lowercase
command=$(echo $command | tr '[:upper:]' '[:lower:]')
echo "Command: $command"

# Determine course of action
if [[ $command == "start" ]]; then
    # Check if the server is already running
    CheckServerStatus
    if [[ $server_status == "running" ]]; then
        echo "ERROR: Cannot start server because the server is alreay running."
        exit 1
    fi

    echo "Starting server..."

    # Start server with nohup. Server pid will be stored in an associated pid file (see supderdoku-api.py)
    nohup $py_exec superdoku-api.py &> superdoku.out &

    echo "Server started."
elif [[ $command == "stop" ]]; then
     # Check if the server is already stopped
    CheckServerStatus
    if [[ $server_status == "stopped" ]]; then
        echo "ERROR: Cannot stop server because the server is alreay stopped."
        exit 1
    fi

    echo "Stopping server..."

    # Get pid from associated pid file and send a SIGINT signal to it (Ctrl+c equivalent)
    pid=$(head -1 $pid_file)
    echo "Killing process with pid $pid"
    
    # Need to specify as /bin/kill otherwise the wrong kill command will be used and the SIGINT will not work
    /bin/kill -9 $pid

    echo "Server stopped."
else
    echo "Invalid command; must be one of [stop|start]"
fi
