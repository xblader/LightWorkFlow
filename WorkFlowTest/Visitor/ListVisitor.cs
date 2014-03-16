using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Visitors;
using System.IO;

namespace WorkFlowTest.Visitor
{
    public class ListVisitor : IVisitor
    {        
        private IList<string> transitions = new List<string>();

        public ListVisitor()
        {      
           
        }

        public void Visit(string status, WorkFlow.Entities.Activity activity, string newstatus)
        {
            string transition = string.Format("{0}--[{1}]-->{2} ", status, activity.Description, newstatus);
            transitions.Add(transition);
        }

        public object EndVisit()
        {
            return transitions;
        }
    }
}
