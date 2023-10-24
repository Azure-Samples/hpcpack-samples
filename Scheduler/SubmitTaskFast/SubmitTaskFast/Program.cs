using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;
using System;

namespace SubmitTaskFast
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

                ISchedulerJob job = scheduler.CreateJob();
                job.Name = "TestSubmitTask";
                ISchedulerTask task = job.CreateTask();
                task.Name = "StartTask";
                task.CommandLine = "ping localhost -n 10";
                job.AddTask(task);
                scheduler.SubmitJob(job, null, null);

                // SubmitTask / SubmitTaskById must be called after a job is submitted
                // SubmitTask / SubmitTaskById call will return when the task reaches the speficied state
                TestSubmitTask(job, TaskState.Configuring | TaskState.Queued);

                job = scheduler.CreateJob();
                job.Name = "TestSubmitTaskById";
                task = job.CreateTask();
                task.Name = "StartTask";
                task.CommandLine = "ping localhost -n 10";
                job.AddTask(task);
                scheduler.SubmitJob(job, null, null);

                TestSubmitTaskById(job, TaskState.Configuring | TaskState.Queued);

                scheduler.Close();
            }
        }

        public static void TestSubmitTask(ISchedulerJob job, TaskState taskState = 0)
        {
            for (int i = 0; i < 10; i++)
            {
                string name = "SubmitTask " + i;
                ISchedulerTask subTask = job.CreateTask();
                subTask.Name = name;
                subTask.CommandLine = commandLine;
                Console.WriteLine("Creating task: {0}...", subTask.Name);

                job.AddTask(subTask);

                if (taskState == 0)
                {
                    job.SubmitTask(subTask);
                }
                else
                {
                    job.SubmitTask(subTask, taskState);
                }
            }

            Console.WriteLine($"[TestSubmitTask] Completed submitting tasks to the cluster");
        }

        public static void TestSubmitTaskById(ISchedulerJob job, TaskState taskState = 0)
        {
            for (int i = 0; i < 10; i++)
            {
                string name = "SubmitTaskById " + i;
                ISchedulerTask subTask = job.CreateTask();
                subTask.Name = name;
                subTask.CommandLine = commandLine;
                Console.WriteLine("Creating task: {0}...", subTask.Name);

                job.AddTask(subTask);

                if (taskState == 0)
                {
                    job.SubmitTaskById(subTask.TaskId);
                }
                else
                {
                    job.SubmitTaskById(subTask.TaskId, taskState);
                }
            }

            Console.WriteLine($"[SubmitTaskById] Completed submitting tasks to the cluster");
        }
    }
}