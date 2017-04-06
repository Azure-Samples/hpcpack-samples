#----------------------------------------------------------------------
# Global variable declarations
#----------------------------------------------------------------------

# Name of Diagnostics Helpers package
$diagsHelper = "Microsoft.Hpc.Diagnostics.Helpers";

# Array for storing information from the nodes in the test
$arrNodes = @();

# The GenerateResults function creates and returns a StepResult object that
# summarizes the diagnostic test data for all of the nodes.
function GenerateResults
{
    # boolean to contain the overall result
    [bool]$bSuccess = $true;

    # Create a new TestResult object to contain StepResults.
    $testResult = New-Object "$diagsHelper.TestResult";
    $testResult.Name = "Path Access Check";

    # Create a new StepResult object to hold a summary of the data for all of
    # the nodes in the test and set its NodeName property to Summary.
    $summResult = New-Object "$diagsHelper.StepResult";
    $summResult.IsSummary = "True";
    $summResult.NodeName = "Summary";

    # Check if any node had a result of Failure. If the result for any node is  
    # Failure, the overall result should be Failure, and the failed node should be
    # added to the list of failed nodes.
    foreach( $node in $arrNodes )
    {

        # Create a new StepResult object to hold the node's result.
        $nodeResult = New-Object "$diagsHelper.StepResult";
        $nodeResult.NodeName = $node.NodeName;

        if ($node.Result -eq [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Success)
        {

            # Set node result to Success.
            $nodeResult.Result = [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Success;

        }
        else
        {

            # Set overall result to Failure.
            $bSuccess = $false;

            # Add the node to the list of failed nodes.
            $summResult.FailedNodes.Add( (New-Object "$diagsHelper.StepResult+Node"($node.NodeName)) );

            # Set node result to Failure.
            $nodeResult.Result = [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Failure;
            
            # Add failure message to node result.
            $nodeResult.Message = $node.Message;

        }

        # Add node result to TestResult.
        $testResult.StepResults.Add( $nodeResult );

    }

    # Set the summary result to Failure or Success.
    if ($bSuccess -eq $true)
    {

        $summResult.Result = [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Success;

    }
    else
    {

        $summResult.Result = [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Failure;

    }

    # Add summary StepResult to TestResult.
    $testResult.StepResults.Insert(0, $summResult);

    return $testResult;

}

# Given XML and XSLT files, generate HTML
function Convert-UsingXslt 
{
    param 
    (
        [string] $xmlFilePath,
        [string] $xslFilePath,
        [string] $outputDir
    )

   # Check XSLT file.
   $xslFilePath = Resolve-Path $xslFilePath
   if ( -not (Test-Path $xslFilePath) ) { throw "Can't access XSL file" } 
   
   # Check XML file.
   $xmlFilePath = Resolve-Path $xmlFilePath
   if ( -not (Test-Path $xmlFilePath) ) { throw "Can't access XML file" } 
   
   # Check output dir.
   $outputDir = Resolve-Path $outputDir
   if ( -not (Test-Path $outputDir) ) { throw "Can't access output dir" } 

   # Get XSL Transform object 
   $EAP = $ErrorActionPreference
   $ErrorActionPreference = "SilentlyContinue"
   $script:xslt = New-Object System.Xml.Xsl.XslCompiledTransform
   $ErrorActionPreference = $EAP
   
   # Load xslt file.
   $xslt.Load( $xslFilePath )
     
   # Perform transformation and output HTML.
   $xslt.Transform( $xmlFilePath, $outputDir + "Report.html" )
}

# Main

# Wrap in try-catch to handle any unexpected exceptions.
try {

    # Load diagnostics helper assembly.
    [Reflection.Assembly]::LoadWithPartialName($diagsHelper) | Out-Null;

    # Split $env:DIAG_NODELIST (a comma-delimited string) into array of node names
    $nodes = $env:DIAG_NODELIST.split(",");

    # Process RunStep result for each node
    foreach ($nodeName in $nodes)
    {

        # Boolean to contain node's success/failure result.
        [bool]$bNodeSuccess = $false;

        # Get node's RunStep output file.
        $outFile = -join( $env:DIAG_DATA, $nodeName, ".OUT" );

        if ( Test-Path $outFile )
        { 
      
            # Get output file's contents.
            $output = Get-Content $outFile

            # If content is "Success", result is Success.
            if ($output -eq "Success")
            {
         
                $bNodeSuccess = $true;
            
            }
         
        }

        # Create a new System.Object object to store node's result.
        $objNode = New-Object System.Object;
        $objNode | Add-Member -type NoteProperty -name NodeName -Value $nodeName;

        if ($bNodeSuccess)
        {

            $objNode | Add-Member -type NoteProperty -name Result -Value "Success";

        } 
        else 
        {

            $objNode | Add-Member -type NoteProperty -name Result -Value "Failure";
            $objNode | Add-Member -type NoteProperty -name Message -Value $output;

        }

        # Add the System.Object object to the global array.   
        $script:arrNodes += $objNode

    }

    # Create a TestResult object that summarizes the result for all nodes.
    $testResult = GenerateResults;

    # Write the TestResult object as an XML file.
    $testResult.XmlSerializeToFile("PostStepResult.xml");

    # Get local paths to XML and XSLT files by replacing \\NODENAME\ path prefix with $env:CCP_DATA
    $resultsDir = [regex]::Replace($env:DIAG_DATA, "\\\\(.*?)\\", $env:CCP_DATA);
    $xmlFile = $resultsDir + "PostStepResult.xml";
    $xsltDir = [regex]::Replace($env:CCP_HOME, "\\\\(.*?)\\", $env:CCP_DATA) + "Bin\";
    $xsltFile = $xsltDir + "diagnostic.xslt";

    # Perform XSLT transformation of Result.xml to generate Report.html
    Convert-UsingXslt $xmlFile $xsltFile $resultsDir;

    return 0;

} 
catch [Exception] 
{

    "PathAccessCheckPostStep: Terminated by unknown exception"
    Write-Host ("Exception " + $_.InvocationInfo.PositionMessage);
    Write-Host $_.Exception.Message;
    Write-Host ("Exception Class: " + $_.Exception.GetType().FullName);
    return -1;

}