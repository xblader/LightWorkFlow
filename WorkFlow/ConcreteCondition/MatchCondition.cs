using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Context;
using WorkFlow.Entities;
using WorkFlow.Validation;

namespace WorkFlow.ConcreteCondition
{
    public class MatchCondition : ConditionBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origem"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public override bool CheckExcludeAtivity(string sourceState, Node node, Transition trans)
        {
            if (node.SourceState.Equals("All") && trans.But == null)
            {
                return true;
            }

            return (node.SourceState.Equals("All") && !trans.But.Any(x => x.Equals(sourceState)));
        }

        public override bool IsOriginStateNotNeeded(Node node, string sourceState)
        {
            if (string.IsNullOrEmpty(sourceState))
                return true;

            return (node.SourceState.Equals(sourceState) || node.SourceState.Equals("All"));
        }

        public override bool CheckConditions(string condition, WorkFlowContext context, IList<ValidationResult> results = null)
        {
            if (condition == null) return true;

            if (condition.StartsWith("!"))
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition.Substring(1))).FirstOrDefault();
                return !CheckParameters(context, cond, results);
            }
            else
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition)).FirstOrDefault();
                return CheckParameters(context, cond, results);
            }
        }              
    }
}
