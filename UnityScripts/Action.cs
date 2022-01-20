using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitySymexCrawler
{
    public class Action
    {
        public readonly ISet<InputCondition> condition;

        public Action(ISet<InputCondition> condition)
        {
            this.condition = condition;
        }

        public bool CanPerform()
        {
            foreach (InputCondition cond in condition)
            {
                if (!cond.CanPerformInput())
                {
                    return false;
                }
            }
            return true;
        }

        public void Perform(InputSimulator sim)
        {
            foreach (InputCondition cond in condition)
            {
                cond.PerformInput(sim);
            }
        }

        public override string ToString()
        {
            return string.Join(" && ", condition);
        }
    }
}