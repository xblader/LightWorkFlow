using System;
using System.Collections.Generic;
using System.Text;
using WorkFlow;
using WorkFlow.Entities;
using System.Linq;
using WorkFlow.Exceptions;
using System.Data;
using WorkFlow.Visitors;
using WorkFlow.ControlAccess;
using WorkFlow.Business.Search;
using WorkFlow.Context;
//using WorkFlow.Business;
namespace WorkFlow.Business.BusinessProcess
{

    /// <summary>
    /// Classe para os processos do WorkFlow.    
    /// </summary>
    public class BpActivity 
    { 
        private List<string> transitions = new List<string>();
        private Structure node;

        public BpActivity()
        {
            node = WorkFlow.Singleton.WorkFlowSingleton.Instance().GetStructure();            
        }

        public Structure GetNode()
        {
            if (node == null) throw new WorkFlowNotFoundException("Fluxo não encontrado. Contate o administrador.");
            return node;
        }

        public string GetNextStatus(string area, string operation, string sourceState = null, WorkFlowContext context = null)
        {
            var retorno = (from w in GetNode().WorkFlow
                           from i in w.Transitions
                           where w.Area.Equals(area)
                           && IsOriginStateNotNeeded(w, sourceState)
                           && i.Operation.Equals(operation)
                           && CheckConditions(i.Condition, context)
                           select i.DestinyState).ToList();

            if (retorno.Count == 0) throw new StatusNotFoundException("Não foi encontrado um destino para esta origem.");
            if (retorno.Count > 1) throw new StatusNotFoundException("Foram encontrados 2 ou mais Status de destino.");

            return retorno[0].ToString();
        }

        private bool IsOriginStateNotNeeded(Node node, string sourceState)
        {
            if (string.IsNullOrEmpty(sourceState))
                return true;

            return (node.SourceState.Equals(sourceState) || node.SourceState.Equals("All"));
        }

        public IList<Activity> GetActivities(string sourceState, string area, WorkFlowContext context = null, IControlAccess access = null)
        {
            var atividades = from w in GetNode().WorkFlow
                             from i in w.Transitions
                             where w.Area.Equals(area)
                             && (w.SourceState.Equals(sourceState) || CheckExcludeAtivity(sourceState, w, i))
                             && CheckConditions(i.Condition, context)
                             select new Activity { Operation = i.Operation, Description = i.Description };

            return (access != null) ? access.checkAccessActivity(atividades.ToList()) : atividades.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origem"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool CheckExcludeAtivity(string sourceState, Node node, Transition trans)
        {
            if (node.SourceState.Equals("All") && trans.But == null)
            {
                return true;
            }

            return (node.SourceState.Equals("All") && !trans.But.Any(x => x.Equals(sourceState)));
        }

        public bool CheckConditions(string condition, WorkFlowContext context)
        {
            if (condition == null) return true;

            if (condition.StartsWith("!"))
            {
                Condition cond = node.Conditions.Where(x => x.Name.Equals(condition.Substring(1))).FirstOrDefault();
                return !CheckParameters(context, cond);
            }
            else
            {
                Condition cond = node.Conditions.Where(x => x.Name.Equals(condition)).FirstOrDefault();
                return CheckParameters(context, cond);
            }            
        }

        private static bool CheckParameters(WorkFlowContext context, Condition cond)
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

        public object Run(string state, string area, WorkFlowContext context, SearchMode mode ,IVisitor visitor = null)
        {
            if (mode == SearchMode.Depth)
            {
                RunInDepth(state, area, context, visitor);
            }
            else if (mode == SearchMode.Breadth)
            {
                RunInWidth(state, area, context, visitor);
            }

            return (visitor ?? new DefaultVisitor()).EndVisit();
        }

        private void RunInDepth(string state, string area, WorkFlowContext context, IVisitor visitor)
        {
            if (string.IsNullOrEmpty(state))
            {
                return;
            }
            else
            {
                foreach (var item in this.GetActivities(state, area, context).OrderBy(x => x.Operation))
                {
                    string novostatus = this.GetNextStatus(area, item.Operation, state, context);
                    string transicao = string.Format("{0},{1},{2}", state, item.Description, novostatus);

                    if (NotPresent(transicao))
                    {
                        (visitor ?? new DefaultVisitor()).Visit(state, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);
                        RunInDepth(novostatus, area, context, visitor);
                    }
                }
            }
        }

        private void RunInWidth(string state, string area, WorkFlowContext context, IVisitor visitor)
        {
            Queue<string> fila = new Queue<string>();
            List<string> mark = new List<string>();

            fila.Enqueue(state);
            mark.Add(state);

            while (fila.Count != 0)
            {
                string statusfila = fila.Dequeue();

                foreach (var item in this.GetActivities(statusfila, area, context).OrderBy(x => x.Operation))
                {
                    string novostatus = this.GetNextStatus(area, item.Operation, statusfila, context);
                    (visitor ?? new DefaultVisitor()).Visit(statusfila, new Activity { Operation = item.Operation, Description = item.Description }, novostatus);
                    
                    if (!mark.Contains(novostatus))
                    {
                        fila.Enqueue(novostatus);
                        mark.Add(novostatus);
                    }
                }
            }                
            
        }

        private bool NotPresent(string transicao)
        {
            if (!transitions.Any(x => x.Equals(transicao)))
            {
                transitions.Add(transicao);
                return true;
            }

            return false;
        }

        internal string GetInitialStatus(string area)
        {
            string retorno = (from i in GetNode().WorkFlow
                              where i.Area.Equals(area) && (!string.IsNullOrEmpty(i.StateOrder) && i.StateOrder.Equals("Initial"))
                             select i.SourceState).FirstOrDefault();

            return retorno;
        }

        internal IList<string> ListAreas()
        {
            return node.WorkFlow.Select(x => x.Area).Distinct().ToList();
        }

        internal string GetActivityDescription(string operation)
        {
            return (from i in GetNode().WorkFlow
                    from atc in i.Transitions
                    where atc.Operation.Equals(operation)
                    select atc.Description).FirstOrDefault();
        }
    }
}

