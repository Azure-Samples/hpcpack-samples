# Microsoft HPC Pack 2016 Linux Node Manager Execution Filter Sample

Execution filter on Linux compute nodes allows cluster admin to plugin customized scripts to be executed (under root) on Linux compute node during different stage of job/task execution.

## Usage

Download the `filters` directory to `/opt/hpcnodemanager/` on each Linux compute node, add execution permission to the scripts and modify them on demand.

### Shortcut

Clusrun can be used to deploy the filters to Linux compute nodes in an HPC Pack cluster.

- Clusrun via HPC Pack Cluster Manager with command:

```bash
cd /opt/hpcnodemanager && curl https://codeload.github.com/Azure-Samples/hpcpack-samples/tar.gz/master | tar -xz --strip=2 hpcpack-samples-master/'Linux node manager execution filters' && chmod +x filters/*
```

- Clusrun via CMD with command:

```CMD
clusrun /nodegroup:LinuxNodes cd /opt/hpcnodemanager ^&^& curl https://codeload.github.com/Azure-Samples/hpcpack-samples/tar.gz/master ^| tar -xz --strip=2 hpcpack-samples-master/'Linux node manager execution filters' ^&^& chmod +x filters/*
```

- Clusrun via Powershell with command:

```Powershell
clusrun /nodegroup:LinuxNodes cd /opt/hpcnodemanager `&`& curl https://codeload.github.com/Azure-Samples/hpcpack-samples/tar.gz/master `| tar -xz --strip=2 hpcpack-samples-master/'Linux node manager execution filters' `&`& chmod +x filters/*
```

### Description

There are three execution filter scripts as entry point, which read json format input from stdin, modify it and write it to stdout.

1. `OnJobTaskStart.sh` is called when a new job (or a task) is dispatched from scheduler to current Linux compute node.

    The json format stdin is like:

    ```
    {
        "m_Item1": {
            "JobId": number,
            "ResIds": array of number,
            "TaskId": number
        },
        "m_Item2": {
            "affinity": array of number,
            "commandLine": string,
            "environmentVariables": object,
            "inputFiles": string,
            "outputFiles": string,
            "role": number,
            "stderr": string,
            "stdin": string,
            "stdout": string,
            "taskRequeueCount": number,
            "workingDirectory": string
        },
        "m_Item3": username as string,
        "m_Item4": password as string,
        "m_Item5": SSH private key as string,
        "m_Item6": SSH public key as string
    }
    ```

2. `OnTaskStart.sh` is called when a new task is dispatched from scheduler to current Linux compute node.

    The json format stdin is like:

    ```
    {
        "m_Item1": {
            "JobId": number,
            "ResIds": array of number,
            "TaskId": number
        },
        "m_Item2": {
            "affinity": array of number,
            "commandLine": string,
            "environmentVariables": object,
            "inputFiles": string,
            "outputFiles": string,
            "role": number,
            "stderr": string,
            "stdin": string,
            "stdout": string,
            "taskRequeueCount": number,
            "workingDirectory": string
        },
    }
    ```

3. `OnJobEnd.sh` is called when a job ends.

    The json format stdin is like:

    ```
    {
        "JobId": number,
        "JobInfo": object,
        "TaskId": number
    }
    ```

## Samples Introduction

1. ResolveUserName.py

    This execution filter is a sample script to compose an Active Directory user name. For an Active Directory integrated Linux environment, it is necessary to compose the RunAs user with different settings, such as: 'winbind seperator' set in /etc/samba/smb.conf for Winbind or 're_expression' set in /etc/sssd/sssd.conf for SSSD to ensure HPC jobs are run as the correct user.

2. ResolveUserNameAndDoMount.py

    This execution filter is a sample script to compose an Active Directory user name, and mount an SMB share if the user is not an HPC administrator. It reuses `ResolveUserName.py` to compose the Active Directory user name.

3. AddDataIoCommand.py

    This execution filter is to modify command for supporting downloading input files and uploading output files of task with HpcData service.

4. AdjustMpiCommand.py

    This execution filter is to modify command for preparation of mpi task.

5. AdjustTaskAffinity.py

    This execution filter is to adjust task affinity in terms of core distribution in NUMA nodes.
