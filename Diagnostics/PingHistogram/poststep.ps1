# poststep.ps1
# Gather the RunStep results data from each node.
# Compose an XML summary with the data.
# Compose an HTML report with a chart of the data.

Write-Host "Starting poststep.ps1...";

if ((Get-PSSnapin | where {$_.Name -eq "Microsoft.Hpc"}) -eq $null)
{   
	Write-Host "Importing Microsoft.HPC module to PowerShell...";
	Add-PSSnapin "Microsoft.HPC";
}

Write-Host "Loading Microsoft.Hpc.Diagnostics.Helpers...";
# Name of Diagnostics Helpers package.
$diagsHelper = "Microsoft.Hpc.Diagnostics.Helpers";
[Reflection.Assembly]::LoadWithPartialName($diagsHelper) | Out-Null;

# Initialize an array for storing information from the nodes in the test.
$arrNodes = @();

# 7 arrays, each one to contain the node names with the particular successful ping response time:
# <1ms, 1ms, 2ms, 3ms, 4ms, 5ms and >5ms.
[string[]]$arrNodesPRTLT1ms = @();
[string[]]$arrNodesPRT1ms   = @();
[string[]]$arrNodesPRT2ms   = @();
[string[]]$arrNodesPRT3ms   = @();
[string[]]$arrNodesPRT4ms   = @();
[string[]]$arrNodesPRT5ms   = @();
[string[]]$arrNodesPRTGT5ms = @();

# GenerateResultsRow creates a StepResult Row object
# which details the number of nodes and node names for a particular ping response time,
# and adds the row to the table.
function GenerateResultsRow
{
	param
	(
		[string] $strResponseTime,
		[string[]] $arrNodesPRT,
		[Microsoft.Hpc.Diagnostics.Helpers.StepResult+Table] $summaryTable
	)
	
	# Create a row for the summary table.
	$summaryRow = New-Object "$diagsHelper.StepResult+Row";

	# Add the column data.
	$summaryRow.Items.Add( (New-Object "$diagsHelper.StepResult+RowItem"($strResponseTime)) );
	$summaryRow.Items.Add( (New-Object "$diagsHelper.StepResult+RowItem"($arrNodesPRT.Count)) );
	$strNodesPRTList = "-";
	[System.Int64]$i64NumNode = 1;
	foreach($nodeName in $arrNodesPRT)
	{
		if ($i64NumNode -eq 1)
		{
			$strNodesPRTList = "";
		}
		$strNodesPRTList += $nodeName;
		if ($i64NumNode -lt $arrNodesPRT.Count)
		{
			$strNodesPRTList += ", ";
		}
		$i64NumNode++;
	}
	$summaryRow.Items.Add( (New-Object "$diagsHelper.StepResult+RowItem"($strNodesPRTList)) );
	
	Write-Host ("Adding row to xml table: Response Time " + $strResponseTime + ", # of Nodes " + $arrNodesPRT.Count + " and Node Names " + $strNodesPRTList);

	# Add the row to the table.
	$summaryTable.Rows.Add($summaryRow);
}

# GenerateResults creates and returns a StepResult object
# which summarizes the diagnostic test data for all of the nodes.
function GenerateResults
{
	# boolean to contain the overall result.
	[bool]$bSuccess = $true;

	# Create a new TestResult object to contain StepResults.
	$testResult = New-Object "$diagsHelper.TestResult";
	$testResult.Name = "Ping Response Times";

	# Create a new StepResult object to hold a summary of the data for all of
	# the nodes in the test and set its NodeName property to Summary.
	$summResult = New-Object "$diagsHelper.StepResult";
	$summResult.IsSummary = "True";
	$summResult.NodeName = "Summary";

	# Check if any node had a result of Failure. If the result for any node is  
	# Failure, the overall result should be Failure, and the failed node should be
	# added to the list of failed nodes.
	foreach($node in $arrNodes)
	{
		if ($node.Result -eq [Microsoft.Hpc.Diagnostics.Helpers.StepResult+ResultCode]::Failure)
		{
			# Add the node to the list of failed nodes.
			$summResult.FailedNodes.Add( (New-Object "$diagsHelper.StepResult+Node"($node.NodeName)) );

			# Set overall result to Failure.
			$bSuccess = $false;
		}
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

	# Create a table for the ping response time summary data.
	$summaryTable = New-Object "$diagsHelper.StepResult+Table"("Ping Response Times");

	# Create the columns for the table.
	$summaryTable.Columns.Add( (New-Object "$diagsHelper.StepResult+Column"("Response Time")) );
	$summaryTable.Columns.Add( (New-Object "$diagsHelper.StepResult+Column"("# of Nodes")) );
	$summaryTable.Columns.Add( (New-Object "$diagsHelper.StepResult+Column"("Node Names")) );

	# Generate the row data for each ping response time.
	GenerateResultsRow "<1ms" $arrNodesPRTLT1ms $summaryTable;
	GenerateResultsRow "1ms"  $arrNodesPRT1ms   $summaryTable;
	GenerateResultsRow "2ms"  $arrNodesPRT2ms   $summaryTable;
	GenerateResultsRow "3ms"  $arrNodesPRT3ms   $summaryTable;
	GenerateResultsRow "4ms"  $arrNodesPRT4ms   $summaryTable;
	GenerateResultsRow "5ms"  $arrNodesPRT5ms   $summaryTable;
	GenerateResultsRow ">5ms" $arrNodesPRTGT5ms $summaryTable;

	# Add the table to the StepResult object.
	$summResult.Tables.Add($summaryTable);

	# Add summary StepResult to TestResult.
	$testResult.StepResults.Insert(0, $summResult);

	return $testResult;
}

# Given XML file, XSLT file, output path and desired output HTML file name, generate the HTML file.
function ConvertXmlToHtml
{
	param 
	(
		[string] $originalXmlFilePath,
		[string] $xslFilePath,
		[string] $outputFilePath,
		[string] $outputFile
	)

	# Check XSLT file.
	if ( -not (Test-Path $xslFilePath) )
	{
		throw "Cannot access XSL file: " + $xslFilePath;
	}

	# Check XML file.
	if ( -not (Test-Path $originalXmlFilePath) )
	{
		throw "Cannot access XML file: " + $originalXmlFilePath;
	}

	# Check output dir.
	if ( -not (Test-Path (Split-Path $outputFilePath)) )
	{
		throw "Cannot access output dir" + $outputFilePath;
	}

	# Get XSL Transform object.
	$EAP = $ErrorActionPreference
	$ErrorActionPreference = "SilentlyContinue"
	$script:xslt = New-Object System.Xml.Xsl.XslCompiledTransform
	$ErrorActionPreference = $EAP

	Write-Host ("Loading xslt file: " + $xslFilePath + "...");

	# Load xslt file.
	$xslt.Load( $xslFilePath )
   
	Write-Host ("Transforming XML file: " + $originalXmlFilePath + "...");

	# Do transform.
	$xslt.Transform( $originalXmlFilePath, $outputFilePath + $outputFile )

	Write-Host ("Created HTML file: " + $outputFilePath + $outputFile + "...");
}

# BuildReportRow details the number of nodes and node names for a particular ping response time,
# and adds the row to the HTML table.
function BuildReportRow
{
	param
	(
		[string] $strAnchorName,
		[string] $strResponseTime,
		[string[]] $arrNodesPRT
	)

	$strNodesPRTList = "-";
	[System.Int64]$i64NumNode = 1;
	foreach($nodeName in $arrNodesPRT)
	{
		if ($i64NumNode -eq 1)
		{
			$strNodesPRTList = "";
		}
		$strNodesPRTList += $nodeName;
		if ($i64NumNode -lt $arrNodesPRT.Count)
		{
			$strNodesPRTList += ", ";
		}
		$i64NumNode++;
	}

	Add-Content -Path $reportName `
		    -Value	("					<tr><td><a name=""" + $strAnchorName + """>" + $strResponseTime + "</a></td><td>" `
                            + $arrNodesPRT.Count `
                            + "</td><td>" `
			    + $strNodesPRTList + "</td></tr>");
}

# Name: BuildReport
# Parameters:
#   [string] $reportName The path to the Report.html file.
#
# Return value: None
# Purpose: Builds a histogram chart using the results of the diagnostic test into the Report.html file.
#
function BuildReport
{
	param
	(
		[string] $reportName
	)

	Write-Host "Building html report...";

        $htm = @"
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>HPC Diagnostic Test - Ping Histogram</title>
<style>
body {
	font-family:tahoma, "segoe ui", verdana, sans-serif;
	font-size:12px;
	color:#111111;
	padding:20px;
	margin:0px;
}
h1 {
	font-weight:normal;
	font-size:1.4em;
	padding:0px 0px 2px 4px;
	margin:0px;
	color:#333333;
}
h2 {
	font-size:1.3em;
	color:#333333;
	margin:20px 0px 0px 0px;
	font-weight:normal;
	padding:0px;
}
h5 {
	color:#666666;
	margin:0px;
	padding:0px;
	font-weight:normal;
	font-size:1em;
}
/* Histogram Height and Width dimensions */
.graphcontainer {
	font-family:tahoma, "segoe ui", verdana, sans-serif;
	height:386px;
	width:500px;
}
.graphcontainer td {
	font-size:.8em;
}
.graph {
	border-left:1px solid #666666;
	border-bottom:1px solid #666666;
}
.graphbar {
	width:100%;
	margin:0px;
	border-left:1px solid #999999;
	border-top:1px solid #999999;
	border-right:1px solid #999999;
	background-color:#6699cc;
	font-size:1px;
	line-height:0;
}
.bottomlabels	{
	margin:0px;
	text-align:center;
}
.leftlabels	{
	height:100%;
	padding:0px;
	margin:0px;
}
.leftlabels td {
	padding:2px 5px 0px 0px;
	margin:0px;
	border-top:1px solid #666666;
}
#topresults {
	padding:0px;
	margin:0 0 20px 0;
}
#topresults #green {
	background-color:#eeffee;
	color:#118811;
}
/* Table Grids */
#grid {
	border-top:1px solid #dddddd;
	border-left:1px solid #dddddd;
	padding:0px;
	margin:0px 0px 0px 5px;
	font-size:1em;
}
#grid td {
	border-right:1px solid #dddddd;
	border-bottom:1px solid #dddddd;
	background-color:#ffffff;
	padding:7px;
	font-size:1em;
}
#grid #colheader {
	background-color:#eeeeee;
	font-size:1em;
}
#detailsdiv {
	padding-top:20px;
}
#detailsdiv table {
	font-size:1em;
}
</style>
</head>
<body>
<h1>Ping Response Times</h1>
<div id="topresults">
  <h2 id="green"> Test Result: Success</h2>
</div>
<hr>
<br>
<h5>Ping Response Times Histogram</h5><br>
<!-- Chart table that holds the histogram and the axis labels -->
<table class="graphcontainer" cellpadding="0" cellspacing="0" border="0">
<tr>
	<td colspan="2">
		<!-- left axis label, may need to include <br> line breaks to make it look nicer -->
		Number<br> of Nodes
	</td>
</tr>
<tr>
	<td align="right">
		<!-- left axis labels, labels should correspond to the top of the range they refer to -->
			<table class="leftlabels" cellpadding="0" cellspacing="0" border="0">
"@

	Set-Content -Path $reportName -Value $htm;

	# Calculate the response time which has the most nodes.
	[System.Int64]$int64MaxNumNodesPRT = 0;
	if ($arrNodesPRTLT1ms.Count -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRTLT1ms.Count ;
	}
	if ($arrNodesPRT1ms.Count   -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRT1ms.Count ;
	}
	if ($arrNodesPRT2ms.Count   -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRT2ms.Count ;
	}
	if ($arrNodesPRT3ms.Count   -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRT3ms.Count ;
	}
	if ($arrNodesPRT4ms.Count   -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRT4ms.Count ;
	}
	if ($arrNodesPRT5ms.Count   -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRT5ms.Count ;
	}
	if ($arrNodesPRTGT5ms.Count -gt $int64MaxNumNodesPRT)
	{
		$int64MaxNumNodesPRT = $arrNodesPRTGT5ms.Count ;
	}

	# Using $int64MaxNumNodesPRT calculate the maximum value of our y-axis
	# and the number of dashes we want on the y-axis for a clean looking chart.

	# If the max is    0-   2 our y-axis max is    2 and the number of dashes is 2.
	# If the max is    3-   4 our y-axis max is    4 and the number of dashes is 4.
	# If the max is    5-   6 our y-axis max is    6 and the number of dashes is 6.
	# If the max is    7-   8 our y-axis max is    8 and the number of dashes is 8.
	# If the max is    9-  10 our y-axis max is   10 and the number of dashes is 5.
	# If the max is   11-  20 our y-axis max is   20 and the number of dashes is 4.
	# If the max is   21-  40 our y-axis max is   40 and the number of dashes is 8.
	# If the max is   41-  60 our y-axis max is   60 and the number of dashes is 6.
	# If the max is   61-  80 our y-axis max is   80 and the number of dashes is 8.
	# If the max is   81- 100 our y-axis max is  100 and the number of dashes is 5.
	# If the max is  101- 200 our y-axis max is  200 and the number of dashes is 4.
	# If the max is  201- 400 our y-axis max is  400 and the number of dashes is 8.
	# If the max is  401- 600 our y-axis max is  600 and the number of dashes is 6.
	# If the max is  601- 800 our y-axis max is  800 and the number of dashes is 8.
	# If the max is  801-1000 our y-axis max is 1000 and the number of dashes is 5.
	# And so on...

	# Variable to store the y-axis max.
	[System.Int64]$int64YAxisMax = 2;

	# Variable to store 10x multiplier.	
	[System.Int64]$int64Multiplier = 1;

	# Variable to store how man dashes we want on our y-axis.
	[System.Int64]$int64NumDashes = 0;

	Write-Host "Calculating y-axis maximum for html report histogram...";

	# Loop until we have our nice y-axis max and number of dashes.
	[bool]$bKeepGoing = $true;
	while ($bKeepGoing)
	{
		$bKeepGoing = $false ;
		$int64YAxisMax = (2 * $int64Multiplier);
		$int64NumDashes = 2;
		if ($int64Multiplier -gt 1)
		{
			$int64NumDashes = 4;
		}

		if ($int64MaxNumNodesPRT -gt $int64YAxisMax)
		{
			$int64YAxisMax = (4 * $int64Multiplier);
			$int64NumDashes = 4;
			if ($int64Multiplier -gt 1)
			{
				$int64NumDashes = 8;
			}

			if ($int64MaxNumNodesPRT -gt $int64YAxisMax)
			{
				$int64YAxisMax = (6 * $int64Multiplier);
				$int64NumDashes = 6;

				if ($int64MaxNumNodesPRT -gt $int64YAxisMax)
				{
					$int64YAxisMax = (8 * $int64Multiplier);
					$int64NumDashes = 8;

					if ($int64MaxNumNodesPRT -gt $int64YAxisMax)
					{
						$int64YAxisMax = (10 * $int64Multiplier);
						$int64NumDashes = 5;

						if ($int64MaxNumNodesPRT -gt $int64YAxisMax)
						{
							$int64Multiplier *= 10 ;
							$bKeepGoing = $true ;
						}
					}
				}
			}
		}
	}

	# Draw our y-axis dash number labels.
	[System.Int64]$int64DashIt = 0;
	[System.Int64]$int64DashVal = 0;
	for ($int64DashIt = $int64NumDashes; $int64DashIt -gt 0; $int64DashIt--)
	{
		$int64DashVal = ($int64YAxisMax * $int64DashIt) / $int64NumDashes ;
		Add-Content -Path $reportName -Value ("			<tr><td valign=""top"">" + $int64DashVal + "</td></tr>");
	}

	$h1 = @"
			</table>
			</td>
			<td class="graph">
				<!-- graph content area, one td line per graph bar -->
				<table width="100%" height="303" cellpadding="1" cellspacing="0" border="0"><tr>
"@

	Add-Content -Path $reportName -Value $h1;

	# Draw all of our bars relative to the table height of 300 pixels.
	[System.Int64]$int64Height = 0;
	$int64Height = (300 * $arrNodesPRTLT1ms.Count) / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#lt1ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRT1ms.Count)   / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#1ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRT2ms.Count)   / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#2ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRT3ms.Count)   / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#3ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRT4ms.Count)   / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#4ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRT5ms.Count)   / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#5ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");
	$int64Height = (300 * $arrNodesPRTGT5ms.Count) / $int64YAxisMax;
	Add-Content -Path $reportName -Value ("			<td valign=""bottom""><a href=""#gt5ms""><div class=""graphbar"" style=""height:" + $int64Height + "px;"">&nbsp;</div></a></td>");

	$h2 = @"
		</table>
	</td>
</tr>
<tr>
	<td>&nbsp; <!-- blank lower left square --></td>
	<td align="center" valign="top">
		<!-- bottom axis labels, one 'td' line per bar label -->
		<!-- for proper alignment, labels should be of equal width, possibly having to force spaces with &nbsp; for proper alignment -->
		<table class="bottomlabels" width="100%" cellpadding="1" cellspacing="0" border="0"><tr>
			<td>&nbsp;&lt;1 ms</td>
			<td>&nbsp;1 ms</td>
			<td>&nbsp;2 ms</td>
			<td>&nbsp;3 ms</td>
			<td>&nbsp;4 ms</td>
			<td>&nbsp;5 ms</td>
			<td>&gt;5 ms</td>
		</table>
		<br>
		<!-- Bottom axis label -->
		Response Time
	</td>
</tr>
</table>
<!-- Table of ping times -->
<div id="detailsdiv">
	<hr>
	<br>
	<table border="0" cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<h5>Ping Response Times Details</h5>
			</td>
		</tr>
		<tr>
			<td height="8"></td>
		</tr>
		<tr>
			<td>
				<table id="grid" border="0" cellpadding="5" cellspacing="0">
					<tr>
						<td id="colheader">Response Time</td>
						<td id="colheader"># of Nodes</td>
						<td id="colheader">Node Names</td>
					</tr>
"@
	Add-Content -Path $reportName -Value $h2;

	BuildReportRow "lt1ms" "&lt;1ms" $arrNodesPRTLT1ms;
	BuildReportRow "1ms" "1ms" $arrNodesPRT1ms;
	BuildReportRow "2ms" "2ms" $arrNodesPRT2ms;
	BuildReportRow "3ms" "3ms" $arrNodesPRT3ms;
	BuildReportRow "4ms" "4ms" $arrNodesPRT4ms;
	BuildReportRow "5ms" "5ms" $arrNodesPRT5ms;
	BuildReportRow "gt5ms" "&gt;5ms" $arrNodesPRTGT5ms;

	$h3 = @"
				</table>
			</td>
		</tr>
	</table><br>
</div>
</body>
</html>
"@

	Add-Content -Path $reportName -Value $h3;

	Write-Host "Finished building html report.";
}

# Main
#

# Wrap in try-catch to handle any unexpected exceptions.
try
{

# Split comma-delimited string $env:DIAG_NODELIST into array of node names.
$nodes = $env:DIAG_NODELIST.Split(",");

# Process RunStep result for each node.
foreach ($nodeName in $nodes)
{
	# Boolean to contain node's success/failure result.
	[bool]$bSuccess = $false;

	# Int64 to contain node's average ping response time in ms.
	[System.Int64]$i64PingAvgMs = -1;

	# Get node's RunStep output file.
	$fileOut = ($env:DIAG_DATA + $nodeName + ".OUT");

	# If output file not found, result is Failure.
	if (Test-Path $fileOut)
	{
		# If file content does not contain "Average =", result is Failure.
		$matchAvgLine = Select-String $fileOut -Pattern "Average =" -SimpleMatch
		if ($matchAvgLine)
		{
			$strAvgLineUpr = ($matchAvgLine.ToString()).ToUpper();
			$intIndexOneAfterLastEq = $strAvgLineUpr.LastIndexOf('=') + 1;
			$intIndexLastM = $strAvgLineUpr.LastIndexOf('M');
			$int64PingAvgMs = 0;
			if ($intIndexLastM -gt $intIndexOneAfterLastEq)
			{
				$strPingAvgMs = $strAvgLineUpr.SubString($intIndexOneAfterLastEq,$intIndexLastM - $intIndexOneAfterLastEq);
				$int64PingAvgMs = [System.Int64]::Parse($strPingAvgMs,[System.Globalization.NumberStyles]::Any);
			}

			switch ($int64PingAvgMs)
			{
				0       { $arrNodesPRTLT1ms = $arrNodesPRTLT1ms + $nodeName }
				1       { $arrNodesPRT1ms   = $arrNodesPRT1ms   + $nodeName }
				2       { $arrNodesPRT2ms   = $arrNodesPRT2ms   + $nodeName }
				3       { $arrNodesPRT3ms   = $arrNodesPRT3ms   + $nodeName }
				4       { $arrNodesPRT4ms   = $arrNodesPRT4ms   + $nodeName }
				5       { $arrNodesPRT5ms   = $arrNodesPRT5ms   + $nodeName }
				default { $arrNodesPRTGT5ms = $arrNodesPRTGT5ms + $nodeName }
			}

			$bSuccess = $true;
		}
	}

	# Create a new System.Object object to store node's result.
	$objNode = New-Object System.Object;

	# Add the member columns.
	$objNode | Add-Member -type NoteProperty -name NodeName -Value $nodeName;
	$objNode | Add-Member -type NoteProperty -name PingAvgMs -Value $int64PingAvgMs;
	if ($bSuccess)
	{
		$objNode | Add-Member -type NoteProperty -name Result -Value "Success";
	} 
	else 
	{
		$objNode | Add-Member -type NoteProperty -name Result -Value "Failure";
	}

	# Add the System.Object object to the global array.   
	$arrNodes += $objNode
}

# Create a TestResult object that summarizes the result for all nodes.
#$testResult = GenerateResults;

# Get local paths to XML and XSLT files.
$resultsDir = $env:DIAG_DATA;
#$xmlFile = $resultsDir + "PostStepResult.xml";
#$xsltFile = $env:CCP_HOME + "Bin\diagnostic.xslt";

#Write-Host ("Creating XML file: " + $xmlFile + "...");

# Write the TestResult object as an XML file.
#$testResult.XmlSerializeToFile($xmlFile);

# Perform XSLT transformation of PostStepResult.xml into PostStepResult.html
#$resultsHtmlFile = "PostStepResult.html"
#ConvertXmlToHtml $xmlFile $xsltFile $resultsDir $resultsHtmlFile;

# Generate output Report.html with histogram of the data in
# the 7 ping response time $arrNodesPRT* node list arrays.
BuildReport ($resultsDir + "Report.html");

Write-Host "Finished poststep.ps1.";

return;

}
catch [Exception] 
{

Write-Host ("Exception " + $_.InvocationInfo.PositionMessage);
Write-Host $_.Exception.Message;
Write-Host ("Exception Class: " + $_.Exception.GetType().FullName);

}
