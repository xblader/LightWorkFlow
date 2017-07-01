using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.Context;
using WorkFlow.Entities;
using WorkFlow.Evaluators;

namespace WorkFlow.ConcreteCondition
{
    public abstract class ConditionBase : WorkFlowContext , IMatchCondition
    {
        // Setup Chain of Responsibility
        private IEvaluatorCommand evaluator =
            new EqEvaluator {
                Successor = new CompEvaluator { Successor = new InEvaluator() }
            };

        public virtual bool CheckParameters(WorkFlowContext context, Condition cond)
        {
            var conditions = cond.Parameters.ToDictionary(x => x.Key, y => y.Value);
            //first verifying if parameters keys supply all conditions keys
            bool keysmatch = (context.Count >= conditions.Keys.Count) &&
                (context.Keys.Intersect(conditions.Keys).Count() == conditions.Keys.Count);

            if (!keysmatch) return false;

            foreach (var item in cond.Parameters)
            {
                if (!evaluator.Execute(item, context))
                    return false;
            }

            return true;
        }

        public abstract bool IsOriginStateNotNeeded(Node node, string sourceState);

        public abstract bool CheckConditions(string p, WorkFlowContext context);

        public abstract bool CheckExcludeAtivity(string sourceState, Entities.Node w, Entities.Transition i);

        public void AddOperator(IEvaluatorCommand evaluator)
        {
            this.evaluator = InsertEvaluator(this.evaluator, evaluator);
        }

        private IEvaluatorCommand InsertEvaluator(IEvaluatorCommand evaluatorSet, IEvaluatorCommand NewEvaluator)
        {
            if (evaluatorSet == null)
                return NewEvaluator;
            else
            {
                evaluatorSet.Successor = InsertEvaluator(evaluatorSet.Successor, NewEvaluator);
                return evaluatorSet;
            }
        }
    }
}
