using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Scheduling
{
    public class Task
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public List<Task> Followers { get; set; }
        public int PrereqCount { get; set; }
        public Rect CellBounds { get; set; }
        public bool IsCritical { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int TaskNumber { get; set; }

        public Task(int index, int duration, string name, List<int> prereqNumbers)
        {
            Index = index;
            Name = name;
            Duration = duration;
            PrereqNumbers = prereqNumbers;
            PrereqTasks = new List<Task>();
            Followers = new List<Task>();
            CellBounds = new Rect();
        }

        public override string ToString()
        {
            return Name;
        }

        public void NumbersToTasks(List<Task> tasks)
        {
            int numPrereqs = PrereqNumbers.Count;
            PrereqTasks = new List<Task>();
            for (int i = 0; i < numPrereqs; i++)
            {
                PrereqTasks.Add(tasks[PrereqNumbers[i]]);
            }
        }


        public void SetTimes()
        {
            StartTime = 0;
            foreach (var prereq in PrereqTasks)
            {
                if (prereq.EndTime > StartTime) StartTime = prereq.EndTime;
            }

            EndTime = StartTime + Duration;
        }

        public void MarkIsCritical()
        {
            IsCritical = true;
            foreach (var prereq in PrereqTasks)
            {
                if (prereq.EndTime == StartTime) prereq.MarkIsCritical();
            }
        }
    }
}
