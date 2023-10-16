This sample describes the process of dynamically changing node groups, and the result it has on the scheduler.

Requirements: At least 2 nodes, with at least one node in a node group named "NodeGroup1", and at least one node in a node group named "NodeGroup2".

To set up nodegroups: 
 - run the "HPC Cluster Manager" as an Administator (You can find this program in Start->All Programs->Microsoft HPC Pack 2016). 
 - Click on the "Node Management" button, then in the list tree, click "By Group". The actions pane  on the righthand side will change (if the actions pane is missing, click on "View", "Actions"). 
 - Click on "Add Group" and give it the name "NodeGroup1". Repeat to add "NodeGroup2". 
 - Now, click on "Online" in the list tree, and you will see a list of your online nodes.
 - Right click on at least one node, and navigate the context menu down to Groups, and click on "NodeGroup1". Repeat for "NodeGroup2". Note: It is important that you choose a node that is assigned either a ComputeNode or WorkstationNode role. These nodes will also be in the ComputeNodes or WorkstationNodes node group by default.

The sample is set up to send jobs to the scheduler defined by the CCP_SCHEDULER environment variable, which is set up automatically when installing the Microsoft HPC Pack 2016 server or client utilities. 

Dynamic Node Groups:
If all the resources are being used up in one nodegroup while nodes in another nodegroup are idle, we can take advantage of dynamic nodegroups to have idle nodes join into a nodegroup and have jobs grow onto it.

The sample begins by submitting a service task (which expands indefinitely) to NodeGroup1 with the intention of taking up all of the group's resources. We can then use the "dynamic node group" feature to help expand the job. Once we see that the job has taken all of the available nodes in the nodegroup, we'll then move an idle node from NodeGroup2 into NodeGroup1 (which can be done via Powershell or through the HPC Cluster Manager). We will then see that the job has grown to the new node without having to modify the job while it is running.
**Remember to checkout to x64 build in Visual Studio to enable powershell support.**