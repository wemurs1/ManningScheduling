using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        }

        public bool VerifySort()
        {
            return true;
        }

        public Task? ReadTask(StreamReader reader)
        {
            Task? task;
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

            taskComponents[2] = taskComponents[2].Trim();
            // first character must be [, last character must be ]
            if (taskComponents[2][0] != '[' || taskComponents[2][taskComponents[2].Length - 1] != ']') return null;
            taskComponents[2] = taskComponents[2].Substring(1, taskComponents[2].Length - 2);

            var taskComponentsStrings = taskComponents[2].Split(',');
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
;
            return new Task(int.Parse(taskComponents[0]), taskComponents[1], preReqList);
        }

        public void LoadPoFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    Task? task;
                    while ((task = ReadTask(reader)) != null)
                    {
                        UnSortedTasks.Add(task);
                    }

                    foreach (var nextTask in UnSortedTasks)
                    {
                        nextTask.NumbersToTasks(UnSortedTasks);
                    }
                }
            }
        }
    }
}
