ReadMe for Scheduler Filter Sample Sources:

Samples for each type of HPC Scheduler filter technology are provided:
- dll
	- Activation FlexLM
		A sample activation filter that integrates with FlexLM. Need to install Windows 11 SDK 10.0.22000.0 (or other SDK versions that contain mc.exe and rc.exe) via Visual Studio Installer. Also need to change Activation FlexLM.csproj -> PreBuildEvent -> path of mc.exe and rx.exe if necessary.
	- Activation HoldUntil
		A sample actvation filter that performs HoldUntil.

	- ComboDiagLogging
		A sample filter that serves as both an activation and submission filter and includes sample logging. This filter is useful for diagnostic purposes as well as demonstrating shared state between a submission filter and an activation filter.

	- Submission JobSize
		A sample submission filter that changes the job based on its size.

	- Custom Node Sorter
		A sample custom node sorter that sorts nodes based on reverse lexicographical order.
	
- exe:
	- Activation FlexLM
		A sample activation filter that integrates with FlexLM. A sample activation filter that integrates with FlexLM. Need to install Windows 11 SDK 10.0.22000.0 (or other SDK versions that contain mc.exe and rc.exe) via Visual Studio Installer. Also need to change Activation FlexLM.csproj -> PreBuildEvent -> path of mc.exe and rx.exe if necessary. This sample needs to be compiled on HPC cluster since the PostBuildEvent in Activation FlexLM.csproj requires HPC cluster command support. Feel free to remove the PostBuildEvent if you want to build it from other environment.

	- Activation HoldUntil
		A sample actvation filter that performs HoldUntil.

	- Submission JobSize
		A sample submission filter that changes the job based on its size.
