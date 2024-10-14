using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree
{
    public class CompositeTask : Task
    {
        protected int currentChild = 0;
        public List<Task> children { get; set; }

        // When a child behavior is complete and returns its status code the Composite decides whether to continue through its
        // children or whether to stop there and then and return a value
        public CompositeTask(List<Task> tasks)
        {
            this.children = tasks;
        }

        public CompositeTask() { }

        public override void Reset() {
            currentChild = 0;
            foreach (Task child in children) {
                child.Reset();
            }
        }

    }
}
