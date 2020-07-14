"""
Installer script for the Sudoku API.

Filename: install.py
Author: Brett Allen
Description: 
    Migrates all associated files to the /opt/Superdoku folder on a Linux machine.
    Expects that the Superdoku API WAR artifact can be deployed to Apache Tomcat as a manual
    post processing step.
"""
import platform
import sys

# Before going any further, assert that the operating system is Linux
target_os = platform.system()
if target_os != "Linux":
    print("Target deployment operating system must be Linux.")
    print("Detected operating system is {}".format(target_os))
    print("Aborting installation procedure.")
    sys.exit(1)

import os
import stat
import subprocess
from shutil import copyfile

# Built-in modules only available on Linux/Unix operating systems
import pwd
import grp

def install_modules():
    """
    Install the set of PIP modules contained within the requirements.txt file.

    Skips over any modules that have already been installed.
    """
    subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", "requirements.txt"])

def chown(path, user, group, recurse=False):
    """
    Change ownership of a given file or directory (path).

    Adds the ability to recursively set the user and group ownership of a directory
    and its nested files and directories.

    Parameters:
        path (string): the path to a file or directory
        user (string): the user to set the ownership to
        group (string): the group to set the ownership to
        recurse (boolean): flag to determine if the nested files or directories of a given directory should
                           also inherit the user and group ownership (otherwise only the top level file or
                           directory will be effected).
    """
    # Get the integer representation of the user and group strings
    uid = pwd.getpwnam(user).pw_uid
    gid = grp.getgrnam(group).gr_gid

    if recurse:
        # Traverse base directory to set owner and group to tomcat for root directory and all nested files
        for root, dirs, files in os.walk(path):
            os.chown(root, uid, gid)
            for d in dirs:
                os.chown("{}/{}".format(root, d), uid, gid)
            for f in files:
                os.chown("{}/{}".format(root, f), uid, gid)
    else:
        # Otherwise, only change the ownership of the top-level directory or file
        os.chown(path, uid, gid)

def main():
    """Invoke the installation procedure as mainline program (not as a module)."""
    print("Installing Superdoku API...")

    # Install required python modules
    install_modules()

    # Create base installation directory based on platform
    base_dir = "/opt/Superdoku"
    print("Migrating server files to {}".format(base_dir))

    # Create the directories as needed
    if not os.path.exists(base_dir):
        os.makedirs(base_dir)

    # Copy resources to base directory
    copyfile("SudokuExtractor.py", "{}/SudokuExtractor.py".format(base_dir))
    copyfile("digit_classifier_cnn.h5", "{}/digit_classifier_cnn.h5".format(base_dir))
    copyfile("run_extractor.sh", "{}/run_extractor.sh".format(base_dir))
    copyfile("SuperdokuAPI/log4j.xml", "{}/log4j.xml".format(base_dir))

    # Change owner of base_dir to tomcat
    chown(path=base_dir, user="tomcat", group="tomcat", recurse=True)

    print("Installation complete.")
    print("Please deploy the associated SuperdokuAPI WAR artifact to Apache Tomcat server.")

if __name__ == "__main__":
    main()
