using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Entities;

namespace WorkFlow.Context
{
    public interface IMatchCondition
    {
        void AddOperator(IEvaluatorCommand evaluator);
        bool IsOriginStateNotNeeded(Node node, string sourceState);
        bool CheckConditions(string p, WorkFlowContext context);
        bool CheckExcludeAtivity(string sourceState, Entities.Node w, Entities.Transition i);
    }
}
