using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace FlexLM
{
    // Information obtained from the FlexLM server is kept as a collection of Licenses
    public struct License
    {
        public string Name { get; set; }
        public int InUse { get; set; }
        public int Total { get; set; }

        public License(string Name, int InUse, int Total)
            : this()
        {
            this.Name = Name;
            this.InUse = InUse;
            this.Total = Total;

            if (InUse < 0)
            {
                throw new ArgumentException("License " + Name + " InUse licenses is negative.");
            }

            if (Total < 0)
            {
                throw new ArgumentException("License " + Name + " Total licenses is negative.");
            }

            if (InUse > Total)
            {
                throw new ArgumentException("License " + Name + " has more InUse licenses than the Total number of licenses available.");
            }
        }
    }

    // When the Activation filter starts a Job, it keeps a reservation for a period of time
    // so that the Application has time to obtain the licenses before this scheduler tries to
    // re-use the licenses for another job.
    public struct Reservation
    {
        public DateTime ReservationTime;
        public string Name;
        public int Count;

        public Reservation(DateTime Time, string Name, int Count)
            : this()
        {
            this.ReservationTime = Time;
            this.Name = Name;
            this.Count = Count;
        }
    }

    // Activation filter uses SerializableCachedData to save state between
    // jobs in the same Scheduler Pass and in the case of Reservations, between
    // Scheduler Passes.
    [Serializable()]
    public class SerializableCachedData
    {
        public int Pass;
        public bool PollServerFailed;
        public List<License> LicenseList;
        public List<Reservation> ReservationList;

        public SerializableCachedData()
        {
            LicenseList = new List<License>();
            ReservationList = new List<Reservation>();
            PollServerFailed = false;
        }
        public SerializableCachedData(CachedData cd)
            : base()
        {
            Pass = cd.Pass;
            PollServerFailed = cd.PollServerFailed;
            foreach (KeyValuePair<string, License> kvp in cd.LicenseDirectory)
            {
                LicenseList.Add(kvp.Value);
            }
            ReservationList = cd.ReservationList;
        }
    }

    public class CachedData
    {
        public int Pass;
        public bool PollServerFailed;
        public Dictionary<string, License> LicenseDirectory;
        public List<Reservation> ReservationList;

        public CachedData()
        {
            LicenseDirectory = new Dictionary<string, License>();
            ReservationList = new List<Reservation>();
            PollServerFailed = false;
        }

        public CachedData(SerializableCachedData scd)
            : base()
        {
            Pass = scd.Pass;
            PollServerFailed = scd.PollServerFailed;
            foreach (License l in scd.LicenseList)
            {
                LicenseDirectory.Add(l.Name, l);
            }
        }
    }

    class Cache
    {
        // The cache is an XML file that is used to hold information from the FlexLM server
        // during a Scheduler pass.

        public static CachedData licenseInfo = new CachedData();

        // The CacheFile specified in the config file can be relative to CCP_DATA
        // or a fully qualified path. Path should specify a location where the file
        // is protected from tampering by users.
        private static string Path
        {
            get
            {
                return System.IO.Path.Combine(
                        Environment.GetEnvironmentVariable("CCP_DATA"),
                        ConfigurationManager.AppSettings["CacheFile"]);
            }
        }

        public static void Clear()
        {
            // Delete the cache to protect make sure we don't use a cache from a
            // previous run of the FlexLM activation filter
            try
            {
                File.Delete(Path);
            }
            catch (DirectoryNotFoundException)
            {
                // It is expected that on first run or possibly on scheduler restart that
                // that the file may not exist
            }
        }

        /// <summary>
        /// Save License information to cache file.
        /// </summary>
        public static void Save()
        {
            // Avoid problem where serialization of IDictionary is not supported. Use IList instead.
            SerializableCachedData serializableLicenseInfo = new SerializableCachedData(licenseInfo);

            XmlSerializer x = new XmlSerializer(serializableLicenseInfo.GetType());
            TextWriter WriteFileStream = new StreamWriter(Path);
            x.Serialize(WriteFileStream, serializableLicenseInfo);
            WriteFileStream.Close();
        }

        /// <summary>
        /// Load information from cache file or FlexLM server
        /// </summary>
        /// <param name="SchedulerIndex"></param>
        /// <param name="JobIndex"></param>
        public static void Load(int SchedulerIndex, int JobIndex)
        {
            bool loaded = false;

            try
            {
                LoadFromCacheFile();
                if (licenseInfo.Pass == SchedulerIndex)
                {
                    loaded = true;
                }
            }
            catch (Exception)
            {
            }

            if ((loaded == false) &&
                (licenseInfo.PollServerFailed == false))
            {
                try
                {
                    PollLicenseServer();
                }
                catch (Exception ex)
                {
                    // Record in the cache file that we have failed to obtain license information
                    // on this Scheduler pass. This handles the following scenario:
                    // job 1 ask for license a but we get an error from the server so we do not schedule job 1
                    // job 2 in the same pass asks for license a but when we retry the server we find there is a
                    // license "a" and we let job 2 run ahead of job 1.
                    licenseInfo = new CachedData();
                    licenseInfo.Pass = SchedulerIndex;
                    licenseInfo.PollServerFailed = true;
                    Save();
                    throw new Exception("polling server:" + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Replace an existing license in the cache
        /// </summary>
        /// <param name="UpdatedLicense"></param>
        private static void UpdateCachedLicense(License UpdatedLicense)
        {
            licenseInfo.LicenseDirectory.Remove(UpdatedLicense.Name);
            licenseInfo.LicenseDirectory.Add(UpdatedLicense.Name, UpdatedLicense);
        }

        /// <summary>
        /// Reserve Count licenses to give time for application to obtain licenses
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Count"></param>
        public static void Reserve(string Name, int Count)
        {
            if (Count == 0)
            {
                // This can occur when all the licenses are already in use
                return;
            }
            int total = Cache.licenseInfo.LicenseDirectory[Name].Total;
            int inUse = Cache.licenseInfo.LicenseDirectory[Name].InUse;
            UpdateCachedLicense(new License(Name, inUse + Count, total));
            licenseInfo.ReservationList.Add(new Reservation(DateTime.Now, Name, Count));
        }

        private static void LoadFromCacheFile()
        {
            // Avoid problem where serialization of IDictionary is not supported. Use IList instead.
            SerializableCachedData serializableLicenseInfo;

            XmlSerializer x = new XmlSerializer(typeof(SerializableCachedData));
            TextReader ReadFileStream = new StreamReader(Path);
            serializableLicenseInfo = (SerializableCachedData)x.Deserialize(ReadFileStream);
            ReadFileStream.Close();

            licenseInfo = new CachedData(serializableLicenseInfo);
            ProcessReservationList(licenseInfo.ReservationList);
        }

        /// <summary>
        /// Run the command specified in the config file, PollCommandName, using
        /// the arguments, PollCommandArguments.
        /// </summary>

        private static void PollLicenseServer()
        {
            // Run the command and get the exit code.
            Process application = new Process();

            application.StartInfo.FileName = ConfigurationManager.AppSettings["PollCommandName"];
            if (String.IsNullOrEmpty(application.StartInfo.FileName))
            {
                throw new Exception("No executable specified in the config file for PollCommandName");
            }

            application.StartInfo.Arguments = ConfigurationManager.AppSettings["PollCommandArguments"];
            if (application.StartInfo.Arguments == null)
                application.StartInfo.Arguments = "";
            application.StartInfo.RedirectStandardOutput = true;
            application.StartInfo.RedirectStandardError = true;
            application.StartInfo.UseShellExecute = false;
            application.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            application.Start();

            String stdout = application.StandardOutput.ReadToEnd();
            String stderr = application.StandardError.ReadToEnd();
            application.WaitForExit();

            Int32 exitcode = application.ExitCode;

            /* TODO: compile up the event stuff
            FlexLM_Activation_Sample.EventWritePollServer(
                application.StartInfo.WorkingDirectory,
                application.StartInfo.FileName,
                application.StartInfo.Arguments,
                exitcode,
                stdout.Substring(0, Math.Min(stdout.Length, 256)), // Avoid filling the log with long string
                stderr.Substring(0, Math.Min(stderr.Length, 256)), // Avoid filling the log with long string
                application.StartTime,
                application.ExitTime);

                */

            ParseFlexlmOutput(stdout);
        }

        /// <summary>
        /// Parse the output of the flexlm utility to find all the currently executing license daemons
        /// and discover the total available licenses and the total licenses.
        /// </summary>
        /// <param name="output">
        /// The output of the flexlm utility.
        /// </param>
        public static void ParseFlexlmOutput(string output)
        {
            string matchPattern = @"Users of (\w+):.*Total of (\d+) licenses issued;.*Total of (\d+) licenses in use";

            MatchCollection theMatches = Regex.Matches(output, matchPattern);

            // Save the reservations from the past
            List<Reservation> SavedReservations = new List<Reservation>();

            foreach (Reservation l in licenseInfo.ReservationList)
            {
                SavedReservations.Add(l);
            }

            // Remove all licenses from the previous pass
            licenseInfo.LicenseDirectory.Clear();

            // Add licenses from FlexLM server
            foreach (Match m in theMatches)
            {
                Int32 total = 0;
                Int32 used = 0;
                Int32 value = 0;
                total = Int32.TryParse(m.Groups[2].Value, out value) ? value : 0;
                used = Int32.TryParse(m.Groups[3].Value, out value) ? value : 0;
                licenseInfo.LicenseDirectory.Add(m.Groups[1].Value, new License(m.Groups[1].Value, used, total));
            }

            if (licenseInfo.LicenseDirectory.Count == 0)
            {
                licenseInfo.PollServerFailed = true;
            }
            else
            {
                // Remove reservations from past so that we give applications some time to acquire their licenses
                ProcessReservationList(SavedReservations);
            }
        }

        /// <summary>
        /// Subtract Reservations from the available license total to take into account the applications
        /// that are in the process of starting that have not yet contacted the FlexLM server and obtained
        /// their licenses.
        /// </summary>
        /// <param name="Reservations"></param>
        public static void ProcessReservationList(List<Reservation> Reservations)
        {
            licenseInfo.ReservationList.Clear();

            if (Reservations.Count != 0)
            {
                int ApplicationStartupTimeInSeconds = 0;
                ApplicationStartupTimeInSeconds = int.Parse(ConfigurationManager.AppSettings["ApplicationStartupTimeInSeconds"]);
                DateTime OldestValidReservation = DateTime.Now - TimeSpan.FromSeconds(ApplicationStartupTimeInSeconds);

                foreach (Reservation r in Reservations)
                {
                    if (r.ReservationTime > OldestValidReservation)
                    {
                        // Try to add the reservation to the InUse value. Handle errors such as feature deleted,
                        // application has updated the FlexLM server etc.
                        if (licenseInfo.LicenseDirectory.ContainsKey(r.Name))
                        {
                            int total = licenseInfo.LicenseDirectory[r.Name].Total;
                            int inUse = licenseInfo.LicenseDirectory[r.Name].InUse;

                            if ((inUse + r.Count) > total)
                            {
                                inUse = total;  // Do not exceed total
                            }
                            else
                            {
                                inUse += r.Count;
                            }

                            UpdateCachedLicense(new License(r.Name, inUse, total));

                            // Retain the reservation so that it is available for the next call
                            licenseInfo.ReservationList.Add(r);
                        }
                    }
                }
            }

        }
    }
}

