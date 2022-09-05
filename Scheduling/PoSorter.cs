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
            // Prepare the tasks.

            // Give each task an empty followers list.

            // Prepare the task prereqs.

            foreach (Task task in UnSortedTasks)
            {
                // Add this task to the followers lists of its prereqs.



                // Set the task's prereq count.

            }



            // Sort.

            // Create the ready list.



            // Move tasks with no prerequisites onto the ready list.


            // Make the sorted task list.


            // Process tasks until we can process no more.
            while (true)
            {
                // Move the first ready task to the sorted list.

                // Update the task's followers.
                {
                    // Decrement the follower’s prereqs count.

                    // If the follower now has no prereqs, add it to the ready list.

                }
            }
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
