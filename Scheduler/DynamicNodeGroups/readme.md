This sample describes the process of dynamically changing node groups, and the result it has on the scheduler.

Requirements: At least 2 nodes, with at least one node in a node group named "NodeGroup1", and at least one node in a Nodegroup named "NodeGroup2".

## How to set up Nodegroup
 - run the "HPC Cluster Manager" as an Administator (You can find this program in Start->All Programs->Microsoft HPC Pack 2016+). 
 - Click on the "Node Management" button, then in the list tree, click "By Group". The actions pane  on the righthand side will change (if the actions pane is missing, click on "View", "Actions"). 
 - Click on "Add Group" and give it the name "NodeGroup1". Repeat to add "NodeGroup2". 
 - Now, click on "Online" in the list tree, and you will see a list of your online nodes.
 - Right click on at least one node, and navigate the context menu down to Groups, and click on "NodeGroup1". Repeat for "NodeGroup2". Note: It is important that you choose a node that is assigned either a ComputeNode or WorkstationNode role. These nodes will also be in the ComputeNodes or WorkstationNodes node group by default.

The sample is set up to send jobs to the scheduler defined by the CCP_SCHEDULER environment variable, which is set up automatically when installing the Microsoft HPC Pack 2016+ server or client utilities. 

## Dynamic NodeGroup
If all the resources are being used up in one Nodegroup while nodes in another Nodegroup are idle, we can take advantage of dynamic Nodegroup to have idle nodes join into a Nodegroup and have jobs grow onto it.

The sample begins by submitting a service task (which expands indefinitely) to NodeGroup1 with the intention of taking up all of the group's resources. We can then use the "dynamic node group" feature to help expand the job. Once we see that the job has taken all of the available nodes in the nodegroup, we'll then move an idle node from NodeGroup2 into NodeGroup1 (which can be done via Powershell or through the HPC Cluster Manager). We will then see that the job has grown to the new node without having to modify the job while it is running.

## How to build
If you want to build both .Net8 and .Net Framework 4.7.2
```powershell
dotnet build
```

If you want to build .Net8
```powershell
dotnet build -f net8.0
```

If you want to build .Net Framework 4.7.2
```powershell
dotnet build -f net472
```
