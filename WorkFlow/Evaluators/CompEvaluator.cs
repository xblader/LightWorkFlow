using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Context;
using WorkFlow.Entities;

namespace WorkFlow.Evaluators
{
    public class CompEvaluator : IEvaluatorCommand
    {
        public IEvaluatorCommand Successor
        {
            get;set;
        }

        public bool Execute(Evaluate item, WorkFlowContext context)
        {
            bool result = false;

            string[] operators = new string[] { "lt", "le", "gt", "ge" };

            if (operators.Any(x => x == item.Operator))
            {
                decimal valuejson, valuecontext;
                if (decimal.TryParse(item.Value.FirstOrDefault(), out valuejson) &&
                    decimal.TryParse(context[item.Key].FirstOrDefault(), out valuecontext))
                {
                    switch (item.Operator)
                    {
                        case "lt":
                            if (valuecontext < valuejson)
                            {
                                return true;
                            }
                            break;
                        case "le":
                            if (valuecontext <= valuejson)
                            {
                                return true;
                            }
                            break;
                        case "gt":
                            if (valuecontext > valuejson)
                            {
                                return true;
                            }
                            break;
                        case "ge":
                            if (valuecontext >= valuejson)
                            {
                                return true;
                            }
                            break;
                    }
                }
                else
                {
                    return false;
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
