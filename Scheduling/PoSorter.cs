using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Scheduling
{
    public class PoSorter
    {
        public List<Task> SortedTasks { get; set; }
        public List<Task> UnSortedTasks { get; set; }

        public PoSorter()
        {
            SortedTasks = new List<Task>();
            UnSortedTasks = new List<Task>();
        }

        public void TopoSort()
        {
            foreach (var task in UnSortedTasks)
            {
                task.Followers = new List<Task>();
            }

            foreach (Task task in UnSortedTasks)
            {
                // Add this task to the followers lists of its prereqs.
                foreach (var prereq in task.PrereqTasks)
                {
                    prereq.Followers.Add(task);
                }


                // Set the task's prereq count.
                task.PrereqCount = task.PrereqTasks.Count;
            }



            // Sort.

            // Create the ready list.
            List<Task> readyList = new List<Task>();


            // Move tasks with no prerequisites onto the ready list.
            foreach (var task in UnSortedTasks)
            {
                if (task.PrereqCount == 0)
                {
                    readyList.Add(task);
                }
            }


            // Make the sorted task list.


            // Process tasks until we can process no more.
            while (readyList.Count != 0)
            {
                // Move the first ready task to the sorted list.
                var workingTask = Pop(readyList);
                if (workingTask != null)
                {
                    SortedTasks.Add(workingTask);

                    // Update the task's followers.
                    foreach (var task in workingTask.Followers)
                    {
                        // Decrement the follower’s prereqs count.
                        task.PrereqCount--;
                        // If the follower now has no prereqs, add it to the ready list.
                        if (task.PrereqCount == 0) readyList.Add(task);
                    }
                }
            }
        }

        private Task? Pop(List<Task> list)
        {
            if (list.Count != 0)
            {
                var task = list[0];
                list.RemoveAt(0);
                return task;
            }

            return null;
        }

        public string VerifySort()
        {
            // Check each sorted task.
            for (int i = 0; i < SortedTasks.Count; i++)
            {
                Task task = SortedTasks[i];
                // Check each of this task's prereqs.
                foreach (Task prereq in task.PrereqTasks)
                {
                    // See if this prereq comes before the task in the list.
                    if (SortedTasks.IndexOf(prereq) >= i)
                        return string.Format("Task [{0}] does not come before [{1}]", prereq, task);
                }
            }

            // Indicate the number of tasks we successfully sorted.
            return string.Format("Successfully sorted {0} out of {1} tasks.", SortedTasks.Count, UnSortedTasks.Count);
        }

        public Task? ReadTask(StreamReader reader)
        {
            var line = reader.ReadLine();
            while (line != null)
            {
                if (line.Trim().Length != 0)
                {
                    return StringToTask(line);
                }
                line = reader.ReadLine();
            }
            return null;
        }

        private Task? StringToTask(string line)
        {
            var taskComponents = line.Split(',', 3);
            if (taskComponents.Length != 3) return null;

            taskComponents[2] = taskComponents[2].Replace("[", "").Replace("]", "").Trim();

            var taskComponentsStrings = taskComponents[2].Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<int> preReqList = new List<int>();
            if (taskComponentsStrings == null) return null;

            foreach (var preReq in taskComponentsStrings)
            {
                if (preReq != null)
                {
                    var preReqInt = int.Parse(preReq);
                    preReqList.Add(preReqInt);
                }
            }

            return new Task(int.Parse(taskComponents[0]), taskComponents[1], preReqList);
        }

        public void LoadPoFile(string filename)
        {
            UnSortedTasks = new List<Task>();

            try
            {
                using (var stream = File.OpenRead(filename))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        Task? task = null;
                        while ((task = ReadTask(reader)) != null)
                        {
                            UnSortedTasks.Add(task);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            foreach (var nextTask in UnSortedTasks)
            {
                nextTask.NumbersToTasks(UnSortedTasks);
            }
        }
    }
}
