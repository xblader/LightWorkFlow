using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Context;
using WorkFlow.Entities;

namespace WorkFlow.Evaluators
{
    public class EqEvaluator : IEvaluatorCommand
    {
        public IEvaluatorCommand Successor
        {
            get; set;
        }
        public bool Execute(Evaluate item, WorkFlowContext context)
        {
            bool result = false;

            if(item.Operator == "eq")
            {
                if (item.Value.Count != context[item.Key].Count) return false;
                if (item.Value.All(context[item.Key].Contains))
                {
                    return true;
                }
            }
            else
            {
                result = Successor.Execute(item, context);
            }

            return result;
        }
    }
}
