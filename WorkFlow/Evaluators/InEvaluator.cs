using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Context;
using WorkFlow.Entities;

namespace WorkFlow.Evaluators
{
    public class InEvaluator : IEvaluatorCommand
    {
        public IEvaluatorCommand Successor
        {
            get; set;
        }
        public bool Execute(Evaluate item, WorkFlowContext context)
        {
            bool result = false;

            if (item.Operator == "in" || string.IsNullOrEmpty(item.Operator))
            {
                if (item.Value.Intersect(context[item.Key]).Count() == context[item.Key].Count())
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
