using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Entities;
using System.Linq;
using WorkFlow.Exceptions;
using Newtonsoft.Json;
using WorkFlow.Visitors;
using WorkFlow.ControlAccess;

namespace WorkFlow
{
    public class BpActivity
    {
        private List<string> transitions = new List<string>();
        private Structure node = WorkFlow.Singleton.WorkFlowSingleton.Instance().GetStructure();

        public string GetNextStatus(string area, string operacao, string statusOrigem = null, Dictionary<string, List<string>> parameters = null)
        {
            var retorno = (from w in node.WorkFlow
                           from i in w.Transitions
                           where w.Area.Equals(area)
                           && IsOriginStateNotNeeded(w, statusOrigem)                          
                           && i.Operacao.Equals(operacao)
                           && CheckConditions(i.Condicao, parameters)
                           select i.StatusDestino).ToList();

            if (retorno.Count == 0) throw new StatusException("Não foi encontrado um destino para esta origem.");
            if (retorno.Count > 1) throw new StatusException("Foram encontrados 2 ou mais Status de destino.");

            return retorno[0].ToString();
        }

        private bool IsOriginStateNotNeeded(Node node, string statusOrigem)
        {
            if (string.IsNullOrEmpty(statusOrigem))
                return true;

            return (node.StatusOrigem.Equals(statusOrigem) || node.StatusOrigem.Equals("All"));
        }

        public IList<Activity> GetActivities(string origem, string area, Dictionary<string, List<string>> parameters = null, IControlAccess access = null)
        {
            var atividades = from w in node.WorkFlow
                             from i in w.Transitions
                             where w.Area.Equals(area)
                             && (w.StatusOrigem.Equals(origem) || CheckExcludeAtivity(origem, w, i))
                             && CheckConditions(i.Condicao, parameters)
                             select new Activity { Operacao = i.Operacao, Descricao = i.Descricao };

            return (access != null) ? access.checkAccessActivity(atividades.ToList()) : atividades.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origem"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public bool CheckExcludeAtivity(string origem, Node node, Transition trans)
        {
            if (node.StatusOrigem.Equals("All") && trans.But == null)
            {
                return true;
            }

            return (node.StatusOrigem.Equals("All") && !trans.But.Any(x => x.Equals(origem)));
        }

        public bool CheckConditions(string condition, Dictionary<string, List<string>> parameters)
        {
            if (condition == null) return true;

            Condition cond = node.Conditions.Where(x => x.Name.Equals(condition)).FirstOrDefault();            

            var conditions = cond.Parameters.ToDictionary(x => x.Key, y => y.Value);
            //first verifying if parameters keys supply all conditions keys
            bool keysmatch = (parameters.Keys.Count >= conditions.Keys.Count) && 
                (parameters.Keys.Intersect(conditions.Keys).Count() == conditions.Keys.Count);
            //verifying if parameter values are in condition values
            int numbervaluesmatches = parameters.Keys
                .Where(k => conditions.ContainsKey(k) && conditions[k].Intersect(parameters[k]).Count() == parameters[k].Count)
                .Count();

            return keysmatch && (numbervaluesmatches == conditions.Count);
        }

        public object RunInDepth(string status, string area, Dictionary<string, List<string>> parameters, IVisitor visitor = null)
        {
            Run(status, area, parameters, visitor);
            return (visitor ?? new DefaultVisitor()).EndVisit();
        }

        private void Run(string status, string area, Dictionary<string, List<string>> parameters, IVisitor visitor)
        {
            if (string.IsNullOrEmpty(status))
            {
                return;
            }
            else
            {
                foreach (var item in this.GetActivities(status, area, parameters).OrderBy(x => x.Operacao))
                {
                    string novostatus = this.GetNextStatus(area, item.Operacao, status, parameters);
                    string transicao = string.Format("{0},{1},{2}", status, item.Descricao, novostatus);

                    if (NotPresent(transicao))
                    {
                        (visitor ?? new DefaultVisitor()).Visit(status, new Activity { Operacao = item.Operacao, Descricao = item.Descricao }, novostatus);
                        RunInDepth(novostatus, area, parameters, visitor);
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
    }    
}
