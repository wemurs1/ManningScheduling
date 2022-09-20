using draw_pert_chart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        const int LEFT_INCREMENT = 20;
        const int TOP_INCREMENT = 20;
        const int TASK_WIDTH = 40;
        const int TASK_HEIGHT = 40;
        const int TEXT_WIDTH = 100;
        const int NORMAL_LINE_THICKNESS = 2;
        const int CRITIAL_LINE_THICKNESS = 4;

        Brush NormalTaskOutlineBrush = Brushes.Black;
        Brush NormalTaskFillBrush = Brushes.LightBlue;
        Brush GridOutlineBrush = Brushes.LightGray;
        Brush GridFillBrush = Brushes.White;
        Brush NormalLineBrush = Brushes.Black;
        Brush CriticalTaskOutlineBrush = Brushes.Red;
        Brush CriticalTaskFillBrush = Brushes.Pink;
        Brush CriticalLineBrush = Brushes.Red;
        Brush GridTextBrush = Brushes.Black;
        Brush TaskTextBrush = Brushes.Black;
        Brush TaskTextBackgroundBrush = Brushes.White;


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
                task.IsCritical = false;
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
            var taskComponents = line.Split(',', 4);
            if (taskComponents.Length != 4) return null;

            taskComponents[3] = taskComponents[3].Replace("[", "").Replace("]", "").Trim();

            var taskComponentsStrings = taskComponents[3].Split(',', StringSplitOptions.RemoveEmptyEntries);
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

            return new Task(int.Parse(taskComponents[0]), int.Parse(taskComponents[1]), taskComponents[2], preReqList);
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
                    task.SetTimes();
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
            if (Columns.Count == 0) return;
            var finishColumn = Columns[Columns.Count - 1];
            if (finishColumn.Count != 1) throw new Exception($"final column had {finishColumn.Count} entires");
            finishColumn[0].MarkIsCritical();
        }

        public void DrawPertChart(Canvas mainCanvas)
        {
            const int LEFT_INCREMENT = 20;
            const int TOP_INCREMENT = 20;
            const int TASK_WIDTH = 40;
            const int TASK_HEIGHT = 40;
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
                    foreach (var prereq in task.PrereqTasks)
                    {
                        // See if this link is critical to its end task.
                        double thickness = 1;
                        Brush brush = Brushes.Black;
                        if (prereq.EndTime == task.StartTime)
                        {
                            // This link is critical for task.
                            thickness = 3;

                            // See if this link is also critical to the Finish task.
                            if (task.IsCritical) brush = Brushes.Red;
                        }
                        mainCanvas.DrawLine(
                            new Point(prereq.CellBounds.X + TASK_WIDTH, prereq.CellBounds.Y + (0.5 * TASK_HEIGHT)),
                            new Point(task.CellBounds.X, task.CellBounds.Y + (0.5 * TASK_HEIGHT)),
                            brush,
                            thickness);
                    }
                }
            }

            foreach (var column in Columns)
            {
                foreach (var task in column)
                {
                    // Figure out which colors to use.
                    Brush rectangle_fill = Brushes.LightBlue;
                    Brush text_fill = Brushes.Black;
                    if (task.IsCritical)
                    {
                        rectangle_fill = Brushes.Pink;
                        text_fill = Brushes.Red;
                    }
                    mainCanvas.DrawRectangle(task.CellBounds, rectangle_fill, Brushes.DarkBlue, 1);
                    PlaceString(mainCanvas, task, 4, 1, text_fill, $"Task: {task.Index.ToString()}");
                    PlaceString(mainCanvas, task, 4, 2, text_fill, $"Dur: {task.Duration.ToString()}");
                    PlaceString(mainCanvas, task, 4, 3, text_fill, $"Start: {task.StartTime.ToString()}");
                    PlaceString(mainCanvas, task, 4, 4, text_fill, $"End: {task.EndTime.ToString()}");
                }
            }
        }
        public void DrawGanttChart(Canvas mainCanvas)
        {
            int left = LEFT_INCREMENT;
            int top = TOP_INCREMENT;
            var tempTaskList = new List<Task>();
            mainCanvas.Children.Clear();

            if (Columns == null) return;
            foreach (var column in Columns)
            {
                foreach (var task in column)
                {
                    tempTaskList.Add(task);
                }
            }
            SortedTasks = tempTaskList.OrderBy(task => task.Index).ToList();
            var taskNumber = 0;
            foreach (var task in SortedTasks)
            {
                task.TaskNumber = ++taskNumber;
            }

            DrawGrid(mainCanvas, left, top);

            foreach (var task in SortedTasks)
            {
                DrawTask(mainCanvas, task);
            }

            // seperate loop to ensure lines are on top of task rectangles
            foreach (var task in SortedTasks)
            {
                DrawLines(mainCanvas, task);
            }

        }

        private void DrawLines(Canvas mainCanvas, Task task)
        {
            var taskTop = TOP_INCREMENT + (task.TaskNumber * TASK_HEIGHT) + TASK_HEIGHT * 0.25;
            var taskBottom = TOP_INCREMENT + (task.TaskNumber * TASK_HEIGHT) + TASK_HEIGHT * 0.75;
            var taskLeft = LEFT_INCREMENT + TEXT_WIDTH + (task.StartTime * TASK_WIDTH);
            var taskOffset = 1;
            foreach (var prereqTask in task.PrereqTasks)
            {
                var prereqTaskRight = LEFT_INCREMENT + TEXT_WIDTH + (prereqTask.EndTime * TASK_WIDTH);
                var prereqTaskMid = TOP_INCREMENT + (prereqTask.TaskNumber * TASK_HEIGHT) + 0.5 * TASK_HEIGHT;
                var taskIncrement = TASK_WIDTH * 0.2 * taskOffset;
                var leftOffset = taskLeft + taskIncrement;
                mainCanvas.DrawLine(
                    new Point(prereqTaskRight, prereqTaskMid),
                    new Point(leftOffset, prereqTaskMid),
                    LineCriticalToFinish(task, prereqTask),
                    LineCriticalForTask(task, prereqTask)
                );
                var taskJoin = taskTop;
                if (taskTop < prereqTaskMid) taskJoin = taskBottom;
                mainCanvas.DrawLine(
                    new Point(leftOffset, prereqTaskMid),
                    new Point(leftOffset, taskJoin),
                    LineCriticalToFinish(task, prereqTask),
                    LineCriticalForTask(task, prereqTask)
                );
                taskOffset++;
            }
        }

        private void DrawTask(Canvas canvas, Task task)
        {
            var text = $"{task.Index}. {task.Name}";
            var textTop = TOP_INCREMENT + (task.TaskNumber * TASK_HEIGHT);
            var taskLeft = LEFT_INCREMENT + TEXT_WIDTH + (task.StartTime * TASK_WIDTH);
            var taskHeight = TASK_HEIGHT * 0.5;
            var taskTop = textTop + 0.25 * TASK_HEIGHT;
            var taskWidth = task.Duration == 0 ? NORMAL_LINE_THICKNESS : task.Duration * TASK_WIDTH;
            canvas.DrawLabel(
                new Rect(LEFT_INCREMENT, textTop, TEXT_WIDTH, TASK_HEIGHT),
                text,
                TaskTextBackgroundBrush,
                TaskTextBrush,
                HorizontalAlignment.Left,
                VerticalAlignment.Center,
                10,
                1
            );
            canvas.DrawRectangle(
                new Rect(taskLeft, taskTop, taskWidth, taskHeight),
                TaskFillColour(task),
                TaskOutlineColour(task),
                NORMAL_LINE_THICKNESS
            );
        }

        private Brush TaskFillColour(Task task)
        {
            return task.IsCritical ? CriticalTaskFillBrush : NormalTaskFillBrush;
        }

        private Brush TaskOutlineColour(Task task)
        {
            return task.IsCritical ? CriticalTaskOutlineBrush : NormalTaskOutlineBrush;
        }

        private int LineCriticalForTask(Task task, Task prereqTask)
        {
            return task.StartTime == prereqTask.EndTime ? CRITIAL_LINE_THICKNESS : NORMAL_LINE_THICKNESS;
        }

        private Brush LineCriticalToFinish(Task task, Task prereqTask)
        {
            if (task.StartTime == prereqTask.EndTime && task.IsCritical) return CriticalLineBrush;
            else return NormalLineBrush;
        }

        private Label PlaceString(Canvas canvas, Task task, int lines, int lineNo, Brush brush, string text)
        {
            if (lines <= 0 || lineNo <= 0 || lineNo > lines) throw new ArgumentException("Bad line parameters");
            var stringHeight = task.CellBounds.Height / lines;
            var top = task.CellBounds.Top + stringHeight * (lineNo - 1);
            var stringBounds = new Rect(task.CellBounds.X, top, task.CellBounds.Width, stringHeight);
            return canvas.RenderString(
                stringBounds,
                new Point(stringBounds.X + (0.5 * stringBounds.Width), stringBounds.Y + (0.5 * stringBounds.Height)),
                text,
                8,
                brush);

        }

        private void DrawGrid(Canvas canvas, int left, int top)
        {
            if (Columns == null) return;

            // determine number of days and number of tasks
            var days = Columns[Columns.Count - 1][0].EndTime;
            var tasks = 0;

            foreach (var column in Columns)
            {
                tasks += column.Count;
            }

            var currentY = top;

            for (int j = 0; j <= tasks; j++)
            {
                var currentX = left + TEXT_WIDTH;

                for (int i = 1; i <= days; i++)
                {
                    canvas.DrawRectangle(
                        new Rect(currentX, currentY, TASK_WIDTH, TASK_HEIGHT),
                        GridFillBrush,
                        GridOutlineBrush,
                        NORMAL_LINE_THICKNESS
                    );
                    if (j == 0) canvas.DrawString(
                        i.ToString(),
                        TASK_WIDTH,
                        TASK_HEIGHT,
                        new Point(currentX + (0.5 * TASK_WIDTH), currentY + (0.5 * TASK_HEIGHT)),
                        0,
                        10,
                        TaskTextBrush
                    );
                    currentX = currentX + TASK_HEIGHT;
                }
                currentY = currentY + TASK_HEIGHT;
            }
        }
    }
}
