This sample demonstrates basic implementation of Windows HPC Scheduler client 
written in Python. It uses new RESTful web service interface introduced in 
Windows HPC Server 2008 R2 Service Pack 2.

Files:

 + winhpclib.py - simple library to generate requests / parse responses from
                  HpcWebService and to send/receive them with use of 'httplib'.

 + example.py   - shows usage of the winhpclib.py by implementing simple 
                  scenario involving job creation, submission, cancelation
                  and also invalid action, which will produce an error.

Sample was tested with Python 2.7 and requires to change server name and user
credentials in example.py line 7. REST web service is expected to use basic
authentication and user job credentials have to be cached ealier (see hpccred
command).
