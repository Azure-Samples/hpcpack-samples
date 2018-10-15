Attribute VB_Name = "HPCExcelMacros"

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

'==========================================================
'
' Section 2: HPC Calculation Macros
'
'==========================================================

'----------------------------------------------------------
'
' HPC_GetVersion returns the version of the macro framework
' implemented in the workbook.  That ensures that future
' versions of the HPC Excel components will always be able
' to run this workbook.
'
' We've implemented version 1.0 of the macro framework,
' so we return the string "1.0".
'
'----------------------------------------------------------
Public Function HPC_GetVersion()

    HPC_GetVersion = "1.0"
    
End Function

'----------------------------------------------------------
'
' HPC_Initialize will be called when the client starts
' a calculation.  Put any pre-calculation steps in this
' function.
'
'----------------------------------------------------------
Public Function HPC_Initialize()
    
End Function

'----------------------------------------------------------
'
' HPC_Partition is used to collect required data for a
' single calculation step (or iteration).  Whatever data
' is returned from this function will be passed to the
' HPC_Execute macro, running on the HPC compute nodes.
'
' When all calculation steps have been completed, return
' "Null" from this function to end the calculation.
'
'----------------------------------------------------------
Public Function HPC_Partition() As Variant

End Function

'----------------------------------------------------------
'
' HPC_Execute performs a single calculation step (or
' iteration).  The input data will match whatever was
' returned from the HPC_Partition function, above.
'
' The return value from this function should be the
' results of the calculation; those results will be
' passed to the HPC_Merge macro, running on the desktop.
'
'----------------------------------------------------------
Public Function HPC_Execute(data As Variant) As Variant

End Function

'----------------------------------------------------------
'
' HPC_Merge is called after a single calculation step (or
' iteration) is complete; the input data will match
' whatever was returned from the HPC_Execute function,
' above.
'
' Use this function to store results: insert results into
' the spreadsheet, write to a database, write a text
' file, or anything else.
'
'----------------------------------------------------------
Public Function HPC_Merge(data As Variant)

End Function

'----------------------------------------------------------
'
' HPC_Finalize is called after the last calculation step
' (or iteration) is complete.  Use this funtion for any
' post-processing steps you want to run after the
' calculation.
'
' The function here cleans up the HPC client object, to
' close the session and end the calculation.
'
'----------------------------------------------------------
Public Function HPC_Finalize()

    ' Clean up the calculation.  It's a good idea to
    ' leave this here, even if you make changes to
    ' this function.  The function we call here is in
    ' the "HPCControlMacros" module.

    CleanUpClusterCalculation

End Function

'----------------------------------------------------------
'
' HPC_ExecutionError is called when there is any error in
' the calculation.
'
' The function here shows a pop-up error message.  You
' can modify this to display the error in a different
' way (for example, in the Excel status bar).
'
'----------------------------------------------------------
Public Function HPC_ExecutionError(errorMessage As String, errorContents As String)

    MsgBox errorMessage & vbCrLf & vbCrLf & errorContents

End Function

