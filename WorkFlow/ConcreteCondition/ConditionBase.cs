using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Context;
using WorkFlow.Entities;

namespace WorkFlow.ConcreteCondition
{
    public abstract class ConditionBase : WorkFlowContext , IMatchCondition
    {
        public virtual bool CheckParameters(WorkFlowContext context, Condition cond)
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

        public abstract bool IsOriginStateNotNeeded(Node node, string sourceState);

        public abstract bool CheckConditions(string p, WorkFlowContext context);

        public abstract bool CheckExcludeAtivity(string sourceState, Entities.Node w, Entities.Transition i);
    }
}
