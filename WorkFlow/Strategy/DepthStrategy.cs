using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Entities;
using WorkFlow.Visitors;

namespace WorkFlow.Strategy
{
    public class DepthStrategy : IRunnerStrategy
    {
        List<String> transitions = new List<string>();
        public void Run(Context.WorkFlowContext context, Visitors.IVisitor visitor)
        {
            if (string.IsNullOrEmpty(context.SourceState))
            {
                return;
            }
            else
            {
                foreach (var item in context.WorkFlow.GetActivities(context).OrderBy(x => x.Operation))
                {
                    string novostatus = context.WorkFlow.GetNextStatus(context);
                    string transicao = string.Format("{0},{1},{2}", context.SourceState, item.Description, novostatus);

                    if (NotPresent(transicao))
                    {
                        (visitor ?? new DefaultVisitor()).Visit(context.SourceState, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);
                        Run(context, visitor);
                    }
                }
            }
        }

        private bool NotPresent(string transition)
        {
            if (!transitions.Any(x => x.Equals(transition)))
            {
                transitions.Add(transition);
                return true;
            }

            return false;
        }
    }
}
