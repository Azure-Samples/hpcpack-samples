using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;
using System;

namespace SubmitJobFast
{
    internal class Program
    {
        static readonly string commandLine = "echo Hello World";

        static void Main()
        {
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            using (IScheduler scheduler = new Scheduler())
            {
                scheduler.Connect(clusterName);

                // SubmitJobById / SubmitJob call will return when the job reaches the speficied state
                TestSubmitJobById(scheduler, JobState.Queued | JobState.Configuring);
                TestSubmitJob(scheduler, JobState.Queued | JobState.Configuring);

                scheduler.Close();
            }
        }

        public static void TestSubmitJobById(IScheduler scheduler, JobState jobState = 0)
        {
            for (int i = 0; i < 10; i++)
            {
                ISchedulerJob job = scheduler.CreateJob();
                job.Name = $"SubmitJobById {i}";
                Console.WriteLine("Creating job: {0}...", job.Name);
                ISchedulerTask task = job.CreateTask();
                task.CommandLine = commandLine;

                job.AddTask(task);
                scheduler.AddJob(job);

                if (jobState == 0)
                {
                    scheduler.SubmitJobById(job.Id, null, null);
                }
                else
                {
                    scheduler.SubmitJobById(job.Id, null, null, jobState);
                }
            }
            Console.WriteLine($"[TestSubmitJobById] Completed submitting jobs to the cluster");
        }

        public static void TestSubmitJob(IScheduler scheduler, JobState jobState = 0)
        {
            for (int i = 0; i < 10; i++)
            {
                ISchedulerJob job = scheduler.CreateJob();
                job.Name = $"SubmitJob {i}";
                Console.WriteLine("Creating job: {0}...", job.Name);
                ISchedulerTask task = job.CreateTask();
                task.CommandLine = commandLine;

                job.AddTask(task);
                scheduler.AddJob(job);

                if (jobState == 0)
                {
                    scheduler.SubmitJob(job, null, null);
                }
                else
                {
                    scheduler.SubmitJob(job, null, null, jobState);
                }
            }
            Console.WriteLine($"[TestSubmitJob] Completed submitting jobs to the cluster");
        }
    }
}