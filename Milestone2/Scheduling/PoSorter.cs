using draw_pert_chart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Scheduling
{
    public class PoSorter
    {
        public List<Task> SortedTasks { get; set; }
        public List<Task> UnSortedTasks { get; set; }
        public List<List<Task>>? Columns { get; set; }

        public PoSorter()
        {
            SortedTasks = new List<Task>();
            UnSortedTasks = new List<Task>();
            Columns = null;
        }

        public void TopoSort()
        {
            PrepareTasks();

            // Sort.

            // Create the ready list.
            Queue<Task> readyQueue = new Queue<Task>();


            // Move tasks with no prerequisites onto the ready list.
            foreach (var task in UnSortedTasks)
            {
                if (task.PrereqCount == 0)
                {
                    readyQueue.Enqueue(task);
                }
            }


            // Make the sorted task list.
            // Process tasks until we can process no more.
            while (readyQueue.Count != 0)
            {
                // Move the first ready task to the sorted list.
                var workingTask = readyQueue.Dequeue();
                if (workingTask != null)
                {
                    SortedTasks.Add(workingTask);

                    // Update the task's followers.
                    foreach (var task in workingTask.Followers)
                    {
                        // Decrement the follower’s prereqs count.
                        task.PrereqCount--;
                        // If the follower now has no prereqs, add it to the ready list.
                        if (task.PrereqCount == 0) readyQueue.Enqueue(task);
                    }
                }
            }
        }

        private void PrepareTasks()
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

        public void BuildPertChart()
        {
            PrepareTasks();

            // Sort.

            // Move tasks with no prerequisites onto a ready list.
            List<Task> readyTasks = new List<Task>();
            foreach (var task in UnSortedTasks)
            {
                if (task.PrereqCount == 0)
                {
                    readyTasks.Add(task);
                }
            }

            // Make the sorted task list.
            List<Task> sortedTasks = new List<Task>();

            // Make the columns list.
            Columns = new List<List<Task>>();

            // Process tasks until we can process no more.
            while (readyTasks.Count > 0)
            {
                // Make the next column entry.
                var columnEntryList = new List<Task>();
                Columns.Add(columnEntryList);

                // Process all tasks in the ready list.
                List<Task> newReadyTasks = new List<Task>();
                while (readyTasks.Count > 0)
                {
                    // Add this task to the new column and
                    // the sorted task list.
                    var task = readyTasks[0];
                    readyTasks.RemoveAt(0);
                    columnEntryList.Add(task);
                    sortedTasks.Add(task);
                    

                    // Update the task's followers.
                    foreach (Task follower in task.Followers)
                    {
                        // If the follower now has no prereqs,
                        // add it to the new ready list.
                        follower.PrereqCount -= 1;
                        if (follower.PrereqCount == 0) newReadyTasks.Add(follower);
                    }
                }

                // The ready list is empty.
                // Move newReadyTasks to readyTasks.
                readyTasks = newReadyTasks;
                newReadyTasks = new List<Task>();

            }
        }

        public void DrawPertChart(Canvas mainCanvas)
        {
            const int LEFT_INCREMENT = 20;
            const int TOP_INCREMENT = 20;
            const int TASK_WIDTH = 20;
            const int TASK_HEIGHT = 20;
            int left = LEFT_INCREMENT;
            int top = TOP_INCREMENT;
            mainCanvas.Children.Clear();

            if (Columns == null) return;

            foreach (var column in Columns)
            {
                foreach (var task in column)
                {
                    var cellBounds = task.CellBounds;
                    cellBounds.X = left;
                    cellBounds.Y = top;
                    cellBounds.Width = TASK_WIDTH;
                    cellBounds.Height = TASK_HEIGHT;
                    task.CellBounds = cellBounds;
                    top = top + TASK_HEIGHT + TOP_INCREMENT;
                }
                left = left + TASK_WIDTH + LEFT_INCREMENT;
                top = TOP_INCREMENT;
            }

            foreach (var column in Columns)
            {
                foreach (var task in column)
                {
                    foreach (var follower in task.Followers)
                    {
                        mainCanvas.DrawLine(
                            new Point(follower.CellBounds.X, follower.CellBounds.Y + (0.5 * TASK_HEIGHT)),
                            new Point(task.CellBounds.X + TASK_WIDTH, task.CellBounds.Y + (0.5 * TASK_HEIGHT)),
                            Brushes.Black,
                            1);
                    }
                }
            }

            foreach (var column in Columns)
            {
                foreach (var task in column)
                {
                    mainCanvas.DrawRectangle(task.CellBounds, Brushes.White, Brushes.DarkBlue, 1);
                    mainCanvas.DrawString(
                        task.Index.ToString(),
                        TASK_WIDTH,
                        TASK_HEIGHT,
                        new Point(task.CellBounds.X + (0.5 * TASK_WIDTH), task.CellBounds.Y + (0.5 * TASK_HEIGHT)),
                        0,
                        10,
                        Brushes.DarkBlue);
                }
            }
        }
    }
}
