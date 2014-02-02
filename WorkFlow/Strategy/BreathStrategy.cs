using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Entities;
using WorkFlow.Visitors;

namespace WorkFlow.Strategy
{
    public class BreathStrategy : IRunnerStrategy
    {
        public void Run(Context.WorkFlowContext context, Visitors.IVisitor visitor)
        {
            Queue<string> fila = new Queue<string>();
            List<string> mark = new List<string>();

            fila.Enqueue(context.SourceState);
            mark.Add(context.SourceState);

            while (fila.Count != 0)
            {
                string statusfila = fila.Dequeue();

                foreach (var item in context.WorkFlow.GetActivities(context).OrderBy(x => x.Operation))
                {
                    string novostatus = context.WorkFlow.GetNextStatus(context);
                    (visitor ?? new DefaultVisitor()).Visit(statusfila, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);

                    if (!mark.Contains(novostatus))
                    {
                        fila.Enqueue(novostatus);
                        mark.Add(novostatus);
                    }
                }
            }                
        }
    }
}
