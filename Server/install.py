import platform
import os
import stat
import sys
import subprocess
from shutil import copyfile

def install_modules():
    """
    Install the set of PIP modules contained within the requirements.txt file
    Skips over any modules that have already been installed.
    """
    subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", "requirements.txt"])

def main():
    target_os = platform.system()
    print("Installing Superdoku API for {}".format(target_os))

    # Install required python modules
    install_modules()

    # Create base installation directory based on platform
    base_dir = os.path.expanduser("~/")
    if target_os == "Linux":
        base_dir = "/opt/"

    base_dir = base_dir + "superdoku"
    bin_dir = base_dir + "/bin"

    print("Migrating server files to {}".format(base_dir))

    # Create the directories as needed
    if not os.path.exists(base_dir):
        os.makedirs(base_dir)

    if not os.path.exists(bin_dir):
        os.makedirs(bin_dir)

    # Copy executables to the bin folder
    copyfile("superdoku.sh", "{}/superdoku.sh".format(bin_dir))
    copyfile("superdoku-api.py", "{}/superdoku-api.py".format(bin_dir))
    copyfile("SudokuExtractor.py", "{}/SudokuExtractor.py".format(bin_dir))
    copyfile("digit_classifier_cnn.h5", "{}/digit_classifier_cnn.h5".format(bin_dir))

    # Don't copy this file because it might not exist yet
    pid_file = "{}/superdoku.pid".format(bin_dir)
    if not os.path.exists(pid_file):
        os.mknod(pid_file)

    # Change permissions on superdoku.sh for execute
    os.chmod("{}/superdoku.sh".format(bin_dir), stat.S_IRWXU | stat.S_IRGRP | stat.S_IXGRP | stat.S_IROTH | stat.S_IXOTH)

    # Copy the service file to the respective systemd directory if Linux
    if target_os == "Linux":
        copyfile("superdoku.service", "/etc/systemd/system/superdoku.service")

        # Reload systemd config and start the superdoku service (set the superdoku server to start a system boot)
        subprocess.check_call(["systemctl", "daemon-reload"])
        subprocess.check_call(["systemctl", "enable", "superdoku.service"])
        subprocess.check_call(["systemctl", "start", "superdoku"])
        subprocess.check_call(["systemctl", "status", "superdoku"])
    elif target_os == "Windows":
        print("Service support for Windows not yet implemented.")
        print("Please execute the superdoku.sh script manually with: sh superdoku.sh [start|stop]")

if __name__ == "__main__":
    main()
