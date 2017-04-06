//------------------------------------------------------------------------------
// <copyright file="Service1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Sample of a SOA service that interacts with an Excel Workbook on an
//      HPC cluster.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using HPCExcel = Microsoft.Hpc.Excel;
using Excel = Microsoft.Office.Interop.Excel;

namespace StaticWorkbookService
{
    public class StaticWorkbookService1 : IStaticWorkbookService
    {
        static HPCExcel.ExcelDriver _driver = null;
        static string _spreadsheet = null;

        public object[] CalculateParameters( string spreadsheetPath, string[] inputRanges, object[] inputValues, string[] outputRanges )
        {
            if (null == _driver)
            {
                _driver = new HPCExcel.ExcelDriver();
            }

            spreadsheetPath = Environment.ExpandEnvironmentVariables(spreadsheetPath);
            if (null == _spreadsheet || !_spreadsheet.Equals(spreadsheetPath))
            {
                _driver.OpenWorkbook(spreadsheetPath);
                _spreadsheet = spreadsheetPath;
                _driver.App.Calculation = Excel.XlCalculation.xlCalculationManual;
            }

            // insert inputs

            if (inputRanges.Length != inputValues.Length) throw new Exception("Invalid parameters: input ranges and values don't match");

            for (int i = 0; i < inputRanges.Length; i++)
            {
                _driver.SetCellValue(inputRanges[i], inputValues[i].ToString());
            }

            // force recalculation

            _driver.App.CalculateFull();

            // collect outputs

            if (outputRanges.Length == 0) return new object[] { };

            object[] rslt = new object[outputRanges.Length];
            for (int i = 0; i < outputRanges.Length; i++)
            {
                rslt[i] = _driver.GetCellValue(outputRanges[i]);
            }

            return rslt;

        }

    }
}
