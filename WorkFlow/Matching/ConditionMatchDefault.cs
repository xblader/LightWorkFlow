using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Entities;

namespace WorkFlow.Matching
{
    public class ConditionMatchDefault : IConditionMatch
    {
        public virtual bool CheckExcludeAtivity(string sourceState, Entities.Node node, Entities.Transition transition)
        {
            if (node.SourceState.Equals("All") && transition.But == null)
            {
                return true;
            }

            return (node.SourceState.Equals("All") && !transition.But.Any(x => x.Equals(sourceState)));
        }

        public virtual bool CheckConditions(string conditionName, Context.WorkFlowContext context)
        {
            if (conditionName == null) return true;

            if (conditionName.StartsWith("!"))
            {
                Condition cond = context.WorkFlow.GetNode().Conditions.Where(x => x.Name.Equals(conditionName.Substring(1))).FirstOrDefault();
                return !CheckParameters(context, cond);
            }
            else
            {
                Condition cond = context.WorkFlow.GetNode().Conditions.Where(x => x.Name.Equals(conditionName)).FirstOrDefault();
                return CheckParameters(context, cond);
            }   
        }

        private static bool CheckParameters(Context.WorkFlowContext context, Condition cond)
        {
            var conditions = cond.Parameters.ToDictionary(x => x.Key, y => y.Value);
            //first verifying if parameters keys supply all conditions keys
            bool keysmatch = (context.Count >= conditions.Keys.Count) &&
                (context.Keys.Intersect(conditions.Keys).Count() == conditions.Keys.Count);
            //verifying if parameter values are in condition values
            int numbervaluesmatches = context.Keys
                .Where(k => conditions.ContainsKey(k) && conditions[k].Intersect(context[k]).Count() == context[k].Count)
                .Count();

            return keysmatch && (numbervaluesmatches == conditions.Count);
        }

        public virtual bool IsOriginStateNotNeeded(Entities.Node node, string sourceState)
        {
            if (string.IsNullOrEmpty(sourceState))
                return true;

            return (node.SourceState.Equals(sourceState) || node.SourceState.Equals("All"));      
        }
    }
}
