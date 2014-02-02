using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Business;
using WorkFlow.Entities;
using WorkFlow.Visitors;
using WorkFlow.Business.Search;
using WorkFlow.Context;
using Newtonsoft.Json;
using WorkFlow.Utils;
using WorkFlow.Exceptions;
using WorkFlow.Matching;
using WorkFlow.Strategy;

namespace WorkFlow.Movimentacao
{
    /// <summary>
    /// Fornece implementação para a interface WorkFlow usada pelo sistema.
    /// </summary>
    public class WorkFlowImpl : IWorkFlow
    {
        public IConditionMatch Match { get; set; }
        private Structure node;
        public IRunnerStrategy RunnerStrategy { get; set; }
        public WorkFlowImpl()
        {
            node = WorkFlow.Singleton.WorkFlowSingleton.Instance().GetStructure();            
        }

        public Structure GetNode()
        {
            if (node == null) throw new WorkFlowNotFoundException("Fluxo não encontrado. Contate o administrador.");
            return node;
        }

        public string GetInitialStatus(string area)
        {
            string retorno = (from i in GetNode().WorkFlow
                              where i.Area.Equals(area) && (!string.IsNullOrEmpty(i.StateOrder) && i.StateOrder.Equals("Initial"))
                              select i.SourceState).FirstOrDefault();

            return retorno;        
        }

        public string GetNextStatus(WorkFlowContext context)
        {            
            var retorno = (from w in GetNode().WorkFlow
                           from i in w.Transitions
                           where w.Area.Equals(context.Area)
                           && Match.IsOriginStateNotNeeded(w, context.SourceState)
                           && i.Operation.Equals(context.Operation)
                           && Match.CheckConditions(i.Condition, context)
                           select i.DestinyState).ToList();

            if (retorno.Count == 0) throw new StatusNotFoundException("Não foi encontrado um destino para esta source.");
            if (retorno.Count > 1) throw new StatusNotFoundException("Foram encontrados 2 ou mais Status de destino.");

            return retorno[0].ToString();                 
        }

        public IList<Activity> GetActivities(WorkFlowContext context, ControlAccess.IControlAccess access = null)
        {
            var atividades = from w in GetNode().WorkFlow
                             from i in w.Transitions
                             where w.Area.Equals(context.Area)
                             && (w.SourceState.Equals(context.SourceState) || Match.CheckExcludeAtivity(context.SourceState, w, i))
                             && Match.CheckConditions(i.Condition, context)
                             select new Activity { Operation = i.Operation, Description = i.Description };

            return (access != null) ? access.checkAccessActivity(atividades.ToList()) : atividades.ToList();
        }

        public IList<string> ListAreas()
        {
            return node.WorkFlow.Select(x => x.Area).Distinct().ToList();
        }

        public object Run(WorkFlowContext context, SearchMode mode, IVisitor visitor = null)
        {
            RunnerStrategy.Run(context, visitor);

            return (visitor ?? new DefaultVisitor()).EndVisit();
        }


        public string GetActivityDescription(string operation)
        {
            return (from i in GetNode().WorkFlow
                    from atc in i.Transitions
                    where atc.Operation.Equals(operation)
                    select atc.Description).FirstOrDefault();
        }

        public bool Validate(byte[] workflow)
        {
            try
            {
                byte[] copy = new byte[workflow.Length];
                workflow.CopyTo(copy, 0);
                string json = string.Join("", WorkFlowUtils.BinaryToStrings(copy, 1024));
                Structure structure = JsonConvert.DeserializeObject<Structure>(json);
                return (structure != null);
            }
            catch (JsonReaderException)
            {
                //TODO need to write some code here to log
                return false;
            }
        }     
    }
}
