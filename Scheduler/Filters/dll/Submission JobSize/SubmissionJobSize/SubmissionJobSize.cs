// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Xml;
using System.IO;
using System.Reflection;
using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace SubmissionJobSize
{
    public class JobSizeFilter : ISubmissionFilter, IFilterLifespan  // note: IFilterLifespan is optional
    {
        public TextWriter logFile = null;
        private int someInternalState = 0;

        public SubmissionFilterResponse FilterSubmission(Stream jobXmlIn, out Stream jobXmlModified)
        {
            someInternalState++;

            // Create the Log file for the filter.
            SubmissionFilterResponse retval = SubmissionFilterResponse.SuccessNoJobChange;

            jobXmlModified = null;

            if ((retval = SetupLogFile()) != 0)
            {
                return retval;
            }

            // Load the job file as an XmlDocument.
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(jobXmlIn);

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("hpc", "http://schemas.microsoft.com/HPCS2008R2/scheduler/");

                // Find the job node in the XML document.
                XmlNode job = doc.SelectSingleNode("/hpc:Job", nsMgr) ?? throw new Exception("No job in the xml file");

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

                        jobXmlModified = new MemoryStream();

                        doc.Save(jobXmlModified);

                        // Return a value of 1 to indicate that the values were changed.
                        retval = SubmissionFilterResponse.SuccessJobChanged;
                    }
                }
            }
            catch (IOException e)
            {
                logFile.WriteLine("Error Loading the XmlFile");
                logFile.WriteLine(e.ToString());

                throw;
            }
            catch (Exception e)
            {
                logFile.WriteLine("Error Parsing the XmlFile");
                logFile.WriteLine(e.ToString());

                throw;
            }
            finally
            {
                logFile.Close();
            }

            return retval;
        }

        private SubmissionFilterResponse SetupLogFile()
        {
            try
            {
                string assemblyPathInclusive = Assembly.GetExecutingAssembly().Location;
                string assemblyPath = Path.GetDirectoryName(assemblyPathInclusive);
                string logFileName = Path.Combine(assemblyPath, "SubmissionFilter.log");
                logFile = new StreamWriter(logFileName, true);
                return SubmissionFilterResponse.SuccessNoJobChange;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating log file: {0}", ex.Message);
                return SubmissionFilterResponse.FailJob;
            }
        }

        public void RevertSubmission(Stream jobXml)
        {
            // any cancelation code here (release licenses, etc)
        }

        // note that IFilterLifespan is optional.  If you do not need setup/teardown code executed
        // do not declare or implement IFilterLifespan 

        public void OnFilterLoad()
        {
            // setup code here
        }

        public void OnFilterUnload()
        {
            // teardown code here
        }
    }
}
