// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Configuration;



namespace SubmissionJobSize
{
    class JobSizeFilter
    {
        const int SuccessNoJobChange = 0;
        const int SuccessJobChanged = 1;
        const int FailureOpeningLogFile = -2;
        const int FailureParsingXml = -1;

        public static TextWriter logFile = null;
        static int Main(string[] args)
        {
            // Create the Log file for the filter.
            int retval;
            if ((retval = setupLogFile()) != 0)
            {
                return retval;
            }

            // Return a value of 0 by default to indicate that no job properties were changed.
            retval = SuccessNoJobChange;

            // Check that there is only one argument.
            if (args.Length != 1)
            {
                logFile.WriteLine("Takes exactly one parameter, ie the name of the job xml file");
            }

            String fileName = args[0];

            // Load the job file as an XmlDocument.
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(fileName);

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("hpc", "http://schemas.microsoft.com/HPCS2008R2/scheduler/");

                // Find the job node in the XML document.
                XmlNode job = doc.SelectSingleNode("/hpc:Job", nsMgr);

                if (job == null)
                {
                    throw new Exception("No job in the xml file");
                }

                // Find the UnitType attribute for the job.
                XmlAttributeCollection attrCol = job.Attributes;
                XmlAttribute unitTypeAttr = attrCol["UnitType"];


                if (unitTypeAttr != null)
                {
                    string unitType = unitTypeAttr.Value;

                    // Depending on the unit type, read in the maximum cores, sockets, or nodes specified in the job.
                    int numMaxUnits = 0;
                    XmlAttribute attrib = null;
                    switch (unitType)
                    {
                        case "Core":
                            attrib = attrCol["MaxCores"];
                            break;
                        case "Socket":
                            attrib = attrCol["MaxSockets"];
                            break;
                        case "Node":
                            attrib = attrCol["MaxNodes"];
                            break;
                        default:
                            throw new Exception("Invalid UnitType");
                    }

                    if (attrib != null)
                    {
                        numMaxUnits = Int32.Parse(attrib.Value);
                    }

                    // If the maximum number of units specified is more than 1, then change the job's properties.
                    if (numMaxUnits > 1)
                    {
                        // Set the job to use the LargeJobTemplate.
                        XmlAttribute templateAttr = attrCol["JobTemplate"];
                        templateAttr.Value = "LargeJobTemplate";


                        // Check if extended terms are already defined.
                        XmlNode extendedTerms = job["ExtendedTerms"];
                        if (extendedTerms == null)
                        {
                            // If extended terms are not defined, add an XML element to the job for extended terms.
                            extendedTerms = doc.CreateElement("ExtendedTerms");
                            job.AppendChild(extendedTerms);
                        }

                        // Create a term to add to the extended terms of this job.
                        XmlNode term = doc.CreateElement("Term");

                        // Create the name value pair for this extended term.
                        // The name/value pair is 
                        // <Name> JobClass </Name>
                        // <Value> MultipleUnit</Value>.
                        XmlNode name = doc.CreateElement("Name");
                        name.InnerText = "JobClass";

                        XmlNode value = doc.CreateElement("Value");
                        value.InnerText = "MultipleUnit";

                        // Add the name/value pair to the term.
                        term.AppendChild(name);
                        term.AppendChild(value);

                        // Add the term to the extended terms list.
                        extendedTerms.AppendChild(term);

                        // Save the changed job properties.
                        doc.Save(fileName);

                        // Return a value of 1 to indicate that the values were changed.
                        retval = SuccessJobChanged;
                    }
                }
            }
            catch (IOException e)
            {
                logFile.WriteLine("Error Loading the XmlFile");
                logFile.WriteLine(e.ToString());
                retval = FailureParsingXml;
            }
            catch (Exception e)
            {
                logFile.WriteLine("Error Parsing the XmlFile");
                logFile.WriteLine(e.ToString());
                retval = FailureParsingXml;
            }
            finally
            {
                logFile.Close();
            }

            return retval;
        }

        private static int setupLogFile()
        {
            try
            {
                String logFileName = "SubmissionFilter.log";
                logFile = new StreamWriter(logFileName, true);
                return 0;
            }
            catch (Exception)
            {
                return FailureOpeningLogFile;
            }
        }

    }
}