# Superdoku REST Service (API)
Fully functioning REST service using Python Flask framework.

## Installation
The first step before being able to use the Superdoku API is to run the ```install.py``` script.
This script will place all the files in the present working directory of the install script to a centralized location on the machine so that the OS can reference a common path. Necessary environment variables will be set so that the server can be started at system boot.

After installation is complete and everything went successfully, the Superdoku API will start at system boot. You can manually start and stop the server using the ```superdoku.sh``` shell script.

## Starting the API
```
sh superdoku.sh start
```

## Stopping the API
```
sh superdoku.sh stop
```

## Troubleshooting
If desired, the ```superdoku-api.py``` can be executed directly from the command line. Only caveat to this is that the server will block whichever terminal window it was executed from (unless executed in separate window).