Attribute VB_Name = "HPCControlMacros"

'==========================================================
'
' This is a skeleton macro file for using the HPC/Excel
' VBA macros with an HPC cluster.
'
' Be sure to add references to the required component:
'
' Microsoft_Hpc_Excel
'
' using the VBA editor menu Tools -> References.
'
'==========================================================

Option Explicit

'==========================================================
'
' Section 1: Variables and constants
'
'==========================================================

'----------------------------------------------------------
'
' This is the cluster scheduler, or head node.  Fill in
' the hostname of your cluster scheduler.
'
'----------------------------------------------------------
Private Const HPC_ClusterScheduler = "HEADNODE_NAME"

'----------------------------------------------------------
'
' This is a network share used to store a temporary copy
' of the workbook.  Make sure that the directory exists,
' that you have write access to the directory, and that
' the compute nodes in the cluster have read access.
'
'----------------------------------------------------------
Private Const HPC_NetworkShare = "\\PATH\TO\SHARE\DIRECTORY"

'----------------------------------------------------------
'
' Optionally, use a job template on the cluster.  See the
' HPC Server documentation for more about job templates.
' Fill in the name of the template you want to use, or
' leave this empty ("") to use the default job template.
'
'----------------------------------------------------------
Private Const HPC_JobTemplate = ""

'----------------------------------------------------------
'
' This object is our client for connecting to the HPC
' cluster and running calculations.
'
'----------------------------------------------------------
Private HPCExcelClient As IExcelClient


'==========================================================
'
' Section 2: Control Functions
'
'==========================================================

'----------------------------------------------------------
'
' This is the main calculation function, which connects
' to the client object and runs a calculation.  The method
' can run both desktop and cluster calculations, controlled
' by the function parameter "CalculateOnDesktop".
'
' You'll see below two functions that run calculations
' by calling this method, with the parameter set for either
' cluster or desktop calculation.
'
'----------------------------------------------------------
Private Sub CalculateWorkbook(CalculateOnDesktop As Boolean)

    Dim HPCWorkbookPath As String

    On Error GoTo ErrorHandler
   
    ' Create a new excelCient instance per session
    Set HPCExcelClient = New ExcelClient
        
    ' Initialize the excel client object with the current workbook
    HPCExcelClient.Initialize ActiveWorkbook
    
    If CalculateOnDesktop = False Then

        ' We need a copy of the file on the network, so it's accessible
        ' by the cluster compute nodes.  Save a temporary copy to the
        ' share directory.
    
        HPCWorkbookPath = HPC_NetworkShare & Application.PathSeparator & ActiveWorkbook.name
    
        ActiveWorkbook.SaveCopyAs HPCWorkbookPath
    
        ' Create a cluster session with the desired options.  Here, we're
        ' just using the scheduler name and (optionally) a job template.
        
        If HPC_JobTemplate <> "" Then
            HPCExcelClient.OpenSession headNode:=HPC_ClusterScheduler, remoteWorkbookPath:=HPCWorkbookPath, jobTemplate:=HPC_JobTemplate
        Else
            HPCExcelClient.OpenSession headNode:=HPC_ClusterScheduler, remoteWorkbookPath:=HPCWorkbookPath
        End If

    End If
    
    ' Run on local machine or cluster as chosen in workbook
    HPCExcelClient.Run CalculateOnDesktop
    Exit Sub
    
ErrorHandler:
    ' Notify user of error and clean up any allocated resources
    MsgBox Prompt:=Err.Description, Title:="HPC Calculation Error"
    If Not HPCExcelClient Is Nothing Then
        HPCExcelClient.Dispose
    End If
End Sub

'----------------------------------------------------------
'
' This is a public method for running a calculation on the
' desktop.  It uses the "CalculateWorkbook" function, above,
' and sets the "Desktop" parameter to True.
'
'----------------------------------------------------------
Public Sub CalculateWorkbookOnDesktop()
    CalculateWorkbook (True)
End Sub

'----------------------------------------------------------
'
' This is a public method for running a calculation on the
' cluster.  It uses the "CalculateWorkbook" function, above,
' and sets the "Desktop" parameter to False.
'
'----------------------------------------------------------
Public Sub CalculateWorkbookOnCluster()
    CalculateWorkbook (False)
End Sub

'----------------------------------------------------------
'
' This method is used to clean up a calculation after it's
' finished; here, we're closing the cluster session so we
' don't waste resources.
'
'----------------------------------------------------------
Public Sub CleanUpClusterCalculation()

    On Error Resume Next
    HPCExcelClient.CloseSession
    On Error GoTo 0

End Sub

