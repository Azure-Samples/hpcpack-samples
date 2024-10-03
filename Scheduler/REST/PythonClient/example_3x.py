# ---------------------------------- #
# Windows HPC Pack 2016 - Submit Job #
# ---------------------------------- #

# by:
#   Thomas Hilbig, FH-Bielefeld, thomas.hilbig@fh-bielefeld.de
#   Simon Bekemeier, FH-Bielefeld, simon.bekemeier@fh-bielefeld.de

# works with
# - Microsoft HPC Pack 2016 RTM / Update 1 / Update 2 / Update 3 and Microsoft HPC Pack 2019 Preview
# - Python 3.7 and above

import time
import sys
import base64
import ssl
import winhpclib_3x
import getpass

# ------------------------------------------------- #
# put in your HPC Job settings in the fields below: #
# ------------------------------------------------- #

# HPC cluster FQDN
MyHeadNodeName = 'myheadnode'

# Job Details (for more properties see HPC Web Service API at https://docs.microsoft.com/en-us/previous-versions/windows/desktop/hh560265(v=vs.85)#remarks)
# -----------
# Job Name:
JobName = "[MyHPC Job]"
# JobTemplate: Default
JobTemplate = "Default"
# Project:
Project = "[MyHPC Project]"
# Priority: Lowest, BelowNormal, Normal
Priority = "Normal"
# Send email notifications to:
EmailAddress = "[name@domain.com]"
## Job resources
# Select the type of resource to request for this job: Core, Node, Socket, GPU
UnitType = "Core"

# Specify tasks for this job:
# ---------------------------
# Tasks (for more properties see HPC Web Service API at https://docs.microsoft.com/en-us/previous-versions/windows/desktop/hh560262(v=vs.85)#remarks)
# Task name:
TaskName = "[MyHPC Task]"
# Command line: ex. "mpiexec MyProgram.exe <param1> <param2>"
CommandLine = "[YourCommandLine]"
# Working directory:
WorkingDirectory = "\\\myheadnode\\[HPC-Share]\\[username / subfolder / etc.]"
# Standard output (relative path to working directory):
StdOutFile = "%CCP_JOBID%_%CCP_TASKID%_out.txt"
# Standard error (relative path to working directory):
StdErrFile = "%CCP_JOBID%_%CCP_TASKID%_err.txt"
# Specify the minimum and maximum number of resources to use for this task.
# Minimum:
MinCores = 1
# Maximum:
MaxCores = 1

# -------------------- #
# end HPC Job settings #
# -------------------- #

# input interactive username & password
print ('input your username and password interactively ...')
username = input('User: ')
password = getpass.getpass('Password:')

try:
    # Configure server connection information
    server = winhpclib_3x.Server(MyHeadNodeName, username, password)
    
    # Create job
    job = winhpclib_3x.Job(server)
    
    # Job Details (for more properties see HPC Web Service API at https://docs.microsoft.com/en-us/previous-versions/windows/desktop/hh560265(v=vs.85)#remarks)
    # -----------
    # Job Name:
    job.properties["Name"] = JobName
    # JobTemplate:
    job.properties["JobTemplate"] = JobTemplate
    # Project:
    job.properties["Project"] = Project
    # Priority:
    job.properties["Priority"] = Priority
    # Send email notifications to:
    job.properties["EmailAddress"] = EmailAddress
    
    ## Job resources
    # Select the type of resource to request for this job:
    job.properties["UnitType"] = UnitType
    # Enter the minimum and/or maximum of the selected resource type that this job is allowed to use:
    # Minimum:
    #job.properties[""] = ""
    # Maximum:
    #job.properties[""] = ""
    
    # Instantiate job on the server
    print ('Creating job on the server...')
    job.create()
    
    # Create task
    task = job.create_task()
    
    # Specify tasks for this job:
    # ---------------------------
    # Tasks (for more properties see HPC Web Service API at https://docs.microsoft.com/en-us/previous-versions/windows/desktop/hh560262(v=vs.85)#remarks)
    # Task name:
    task.properties["Name"] = TaskName
    # Command line:
    task.properties["Commandline"] = CommandLine
    # Working directory:
    task.properties["WorkDirectory"] = WorkingDirectory
    # Standard output (relative path to working directory):
    task.properties["StdOutFilePath"] = StdOutFile
    # Standard error (relative path to working directory):
    task.properties["StdErrFilePath"] = StdErrFile
    # Specify the minimum and maximum number of resources to use for this task.
    # Minimum:
    task.properties["MinCores"] = MinCores
    # Maximum:
    task.properties["MaxCores"] = MaxCores
    
    # Instantiate task on the server
    print ('Creating task within the job on the server...')
    task.create()
    
    # Submit job
    print ('Submitting job...')
    job.submit(username, password)

# Catch exception
except winhpclib_3x.Error as error:
    print ('Server returned an error:', error)