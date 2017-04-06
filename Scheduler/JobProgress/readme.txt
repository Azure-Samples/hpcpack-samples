This sample describes the process of modifying a job's progress information and message using HPC Pack 2016 APIs.

The sample is set up to send jobs to the scheduler defined by the CCP_SCHEDULER environment variable, which is set up automatically when installing the Microsoft HPC Pack 2016 server(Set the CCP_SCHEDULER if use Microsoft HPC Pack 2016 client utilities to configure the scheduler). 

Detail description:
This sample shows the general process of creating a submitting jobs to a cluster, running the job,  modifying the process, and waiting the job to finsih. A job is submitted with a echo parameter task which echos number 1-500, and using an event handler, waiting the job's status to switch, and printing corresponding information.