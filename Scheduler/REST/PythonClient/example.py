# Copyright © Microsoft Corporation.  All Rights Reserved.
# This code released under the terms of the 
# MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
#
#Copyright (C) Microsoft Corporation.  All rights reserved.

import time
import sys
import winhpclib

try:
    # Configure server connection information
    # WARNING: Underlying httplib call does not do any verification of the server's certificate.
    # See http://docs.python.org/library/httplib.html
    server = winhpclib.Server('myheadnode', 'domain\myusername', 'mypassword123')
    
    # Create job
    job = winhpclib.Job(server)
    
    # Assign some job properties
    job.properties["Name"] = "Python Job"
    job.properties["Priority"] = "Lowest"
    
    # Instantiate job on the server
    print 'Creating job on the server...'
    job.create()
    
    # Create task
    task = job.create_task()
    
    # Set some task properties
    task.properties["MinCores"] = 1
    task.properties["MaxCores"] = 1
    task.properties["Commandline"] = "ping -t localhost"
    
    # Instantiate task on the server
    print 'Creating task within the job on the server...'
    task.create()
    
    # Submit job
    print 'Submitting job...'
    job.submit()
    
    # Monitor job by polling its state
    running_for = 0
    canceled_already = False
    current_state = job.properties.get('State')
    
    while current_state != 'Canceled':
        time.sleep(1)
        
        # Refresh state property value
        job.refresh_properties('State')
        current_state = job.properties['State']
        print 'Current job state: ', job.properties['State']
        
        # Count how long job is running
        if current_state == 'Running':
            running_for += 1
        
        # If running for about 5 seconds, cancel it forcefully
        if running_for >= 5 and not canceled_already:
            print "Canceling job..."
            job.cancel("Canceled from Python Client", True)
            canceled_already = True
            
    
    # Getting tasks output
    task.refresh_properties('Output')
    print
    print "Task's output:"
    print task.properties['Output']
    print
    
    # Try to cancel for the second time to generate error message
    print
    print 'Trying to cancel already canceled job to cause an exception...'
    print
    job.cancel()

# Catch exception
except winhpclib.Error as error:
    print 'Server returned an error:', error