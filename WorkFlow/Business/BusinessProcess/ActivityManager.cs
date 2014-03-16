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
    public abstract class ActivityManager : WorkFlowContext, IWorkFlow
    {
        private List<string> transitions = new List<string>();

        public string GetNextStatus(WorkFlowContext context)
        {
            var retorno = (from w in GetNode().WorkFlow
                           from i in w.Transitions
                           where w.Area.Equals(context.Area)
                           && context.Match.IsOriginStateNotNeeded(w, context.SourceState)
                           && i.Operation.Equals(context.Operation)
                           && context.Match.CheckConditions(i.Condition, context)
                           select i.DestinyState).ToList();

            if (retorno.Count == 0) throw new StatusNotFoundException("Não foi encontrado um destino para esta origem.");
            if (retorno.Count > 1) throw new StatusNotFoundException("Foram encontrados 2 ou mais Status de destino.");

            return retorno[0].ToString();
        }

        public virtual IList<Activity> GetActivities(WorkFlowContext context, IControlAccess access = null)
        {
            var atividades = from w in GetNode().WorkFlow
                             from i in w.Transitions
                             where w.Area.Equals(context.Area)
                             && (w.SourceState.Equals(context.SourceState) || context.Match.CheckExcludeAtivity(context.SourceState, w, i))
                             && context.Match.CheckConditions(i.Condition, context)
                             select new Activity { Operation = i.Operation, Description = i.Description };

            return (access != null) ? access.checkAccessActivity(atividades.ToList()) : atividades.ToList();
        }

        public virtual string GetInitialStatus(string area)
        {
            string retorno = (from i in GetNode().WorkFlow
                              where i.Area.Equals(area) && (!string.IsNullOrEmpty(i.StateOrder) && i.StateOrder.Equals("Initial"))
                              select i.SourceState).FirstOrDefault();

            return retorno;
        }

        public virtual IList<string> ListAreas()
        {
            return GetNode().WorkFlow.Select(x => x.Area).Distinct().ToList();
        }

        public virtual string GetActivityDescription(string operation)
        {
            return (from i in GetNode().WorkFlow
                    from atc in i.Transitions
                    where atc.Operation.Equals(operation)
                    select atc.Description).FirstOrDefault();
        }

        public abstract object Run(WorkFlowContext context, SearchMode mode, IVisitor visitor = null);


        public virtual WorkFlowContext GetContext()
        {
            return this;
        }
    }
}

