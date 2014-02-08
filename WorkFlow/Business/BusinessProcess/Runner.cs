using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Business.Search;
using WorkFlow.Context;
using WorkFlow.Entities;
using WorkFlow.Visitors;

namespace WorkFlow.Business.BusinessProcess
{
    public class RunnerManager : ActivityManager
    {
        private List<string> transitions = new List<string>();

        public override object Run(WorkFlowContext context, SearchMode mode, IVisitor visitor = null)
        {
            if (visitor == null)
                visitor = new DefaultVisitor();

            if (mode == SearchMode.Depth)
            {
                RunInDepth(context, visitor);
            }
            else if (mode == SearchMode.Breadth)
            {
                RunInWidth(context, visitor);
            }

            return visitor.EndVisit();
        }     

        private void RunInDepth(WorkFlowContext context, IVisitor visitor)
        {
            if (string.IsNullOrEmpty(context.SourceState))
            {
                return;
            }
            else
            {
                foreach (var item in this.GetActivities(context).OrderBy(x => x.Operation))
                {
                    context.Operation = item.Operation;
                    string novostatus = this.GetNextStatus(context);
                    string laststate = context.SourceState;
                    string transicao = string.Format("{0},{1},{2}", context.SourceState, item.Description, novostatus);

                    if (NotPresent(transicao))
                    {
                        visitor.Visit(context.SourceState, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);
                        context.SourceState = novostatus;                        
                        RunInDepth(context, visitor);
                        context.SourceState = laststate;
                    }
                }
            }
        }

        private void RunInWidth(WorkFlowContext context, IVisitor visitor)
        {
            Queue<string> fila = new Queue<string>();
            List<string> mark = new List<string>();

            fila.Enqueue(context.SourceState);
            mark.Add(context.SourceState);

            while (fila.Count != 0)
            {
                string statusfila = fila.Dequeue();
                context.SourceState = statusfila;
                foreach (var item in this.GetActivities(context).OrderBy(x => x.Operation))
                {
                    context.Operation = item.Operation;
                    string novostatus = this.GetNextStatus(context);
                    visitor.Visit(statusfila, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);

                    if (!mark.Contains(novostatus))
                    {
                        fila.Enqueue(novostatus);
                        mark.Add(novostatus);
                    }
                }
            }

        }

        private bool NotPresent(string transicao)
        {
            if (!transitions.Any(x => x.Equals(transicao)))
            {
                transitions.Add(transicao);
                return true;
            }

            return false;
        }        
    }
}
