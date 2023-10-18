This sample describes the process of setting Expanded Priorities of a job.

The sample is set up to send jobs to the scheduler defined by the CCP_SCHEDULER environment variable, which is set up automatically when installing the Microsoft HPC Pack 2016+ server or client utilities. 

ExpandedPriorities:
While all of the exisiting JobPriority enumerations still exist, HPC Pack 2016+ users are now allowed to set the job's expanded priority, which is a numerical value from 0-4000, giving users finer control over the cluster's queue.

The sample simply goes through the different methods of setting a job's expanded priority. It can be set directly (job.ExpandedPriority = 4000), using ExpandedPriority enumerations, or adding and subtracting from the current priority levels. Also shows samples of helper functions to assist users with using ExpandedPriorities and proofing their code for future changes.