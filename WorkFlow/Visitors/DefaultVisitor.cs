using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Visitors
{
    public class DefaultVisitor : IVisitor
    {

        private IList<string> transitions = new List<string>();

        public virtual void Visit(string status, Entities.Activity activity, string novostatus)
        {
            transitions.Add(string.Format("{0}=={1}==>{2}", status, activity.Operation, novostatus));
        }

        public virtual object EndVisit()
        {
            return transitions;
        }
    }
}
