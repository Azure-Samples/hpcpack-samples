# Introduction
HPC Scheduler REST API Samples.
This set of sample sources provide an introduction to the programming model available to the latest REST API for the HPC Scheduler.

# Description
Examples include:
- Authentication between client and REST server using Basic Authentication or Default netowrk credentials or the AAD Authentication.
- Create a job
- Create a task
- Submit a task
- Check the state of the job

# Usage
The username and password should be wrapped with double quotation marks.
CSharpClient2016 -c &lt;cluster_name&gt; -u "&lt;domain_name&gt;\\&lt;user_name&gt;" -p "&lt;password&gt;"

# Node
This is the old version of REST api whose url starts with `WindowsHpc`.
View [CSharpClient2016](https://github.com/Azure-Samples/hpcpack-samples/tree/master/Scheduler/REST/CSharpClient2016) for the latest version of REST API whose url starts with `hpc`.
