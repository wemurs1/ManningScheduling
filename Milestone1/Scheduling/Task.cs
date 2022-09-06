using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduling
{
    public class Task
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<int> PrereqNumbers { get; set; }
        public List<Task> PrereqTasks { get; set; }
        public List<Task> Followers { get; set; }
        public int PrereqCount { get; set; }

        public Task(int index, string name, List<int> prereqNumbers)
        {
            Index = index;
            Name = name;
            PrereqNumbers = prereqNumbers;
            PrereqTasks = new List<Task>();
            Followers = new List<Task>();
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
    }
}
