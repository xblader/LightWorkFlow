using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Matching
{
    public interface IConditionMatch
    {
        bool CheckExcludeAtivity(string sourceState, Entities.Node node, Entities.Transition transition);
        bool CheckConditions(string conditionName, Context.WorkFlowContext context);
        bool IsOriginStateNotNeeded(Entities.Node node, string sourceState);
    }
}
