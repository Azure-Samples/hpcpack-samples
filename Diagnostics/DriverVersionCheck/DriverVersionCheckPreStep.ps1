#----------------------------------------------------------------------
# Global variable declarations
#----------------------------------------------------------------------

# Name of Diagnostics Helpers package
$diagsHelper = "Microsoft.Hpc.Diagnostics.Helpers";

# Wrap all in a try-catch block to handle any unexpected exceptions.
try {

    # Create an object as Microsoft.Hpc.Diagnostics.Helpers.StepResult.
    $stepResult = New-Object "$diagsHelper.StepResult";

    # The diagnostic service automatically sets the DIAG_NODELIST environment 
    # variable to a comma-separated list of the nodes that the user specified
    # for the test. 
    $nodeList = $env:DIAG_NODELIST;

    # Counter for nodes that are not in the Unreachable state. If all nodes are 
    # Unreachable, the test does not run.
    $nodeOutputCount = 0;
    $hpcNodes = @();
  
    if($nodeList -ne $null)
    {
     
        # Process each node in the list of nodes.
        foreach ($node in $nodeList.Split(','))
        {
            $nodeObj = (Get-HpcNode -Name $node)

            # Print node name and state info to stdout.
            "Node:"+$node+" NodeState:"+$nodeObj.NodeState+" nodehealth:"+$nodeObj.servicehealth
            
            # Check that the node is not Unreachable.
            if($nodeObj.servicehealth -ne "Unreachable")           

            {

                # Create a Node object for the node and add it to the StepResult object.
                $hpcNodes += New-Object "$diagsHelper.StepResult+Node($node)";
                $nodeOutputCount++;

            }

            else

            {

                # Write a warning to the Prestep.result file to provide diagnostic 
                # information about why the test did not run on one of the nodes 
                # that the user selected. 
                $warning = -join ("Node ",$node," was Unreachable")
                Write-Warning $warning

            }

        }

        $stepResult.Nodes = $hpcNodes;

    }

    # If no nodes found that aren't Unreachable...
    if($nodeOutputCount -eq 0)
    {

        # Write error message to stdout.
        Write-Warning "No nodes were found that were not Unreachable so the test was not run. Check that the nodes on which you want to run the `ntest are not Unreachable, then rerun the test.";
        
        # Return -1 indicating that the step resulted in an error.
        return -1;

    }

    # else 1 or more nodes are online.
    else 
    
    {

        # Write StepResult object to XML file.
        $stepResult.XmlSerializeToFile(".\DriverVersionCheckPreStepResult.xml");

        # Return 0 indicating "no error".
        return 0;
    
    }
    
} 
catch [Exception]
{

    # Write error message to stdout.
    "DriverVersionCheckPreStep.ps1: Terminated by unknown exception"
    Write-Host ("Exception " + $_.InvocationInfo.PositionMessage);
    Write-Host $_.Exception.Message;
    Write-Host ("Exception Class: " + $_.Exception.GetType().FullName);
    return -1;

}    