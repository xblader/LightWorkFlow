using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Entities;
using WorkFlow.Validation;

namespace WorkFlow.Context
{
    public interface IMatchCondition
    {
        void AddOperator(IEvaluatorCommand evaluator);
        bool IsOriginStateNotNeeded(Node node, string sourceState);
        bool CheckConditions(string p, WorkFlowContext context, IList<ValidationResult> results = null);
        bool CheckExcludeAtivity(string sourceState, Entities.Node w, Entities.Transition i);
    }
}
