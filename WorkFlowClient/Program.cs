using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow;
using WorkFlow.Business.Search;
using WorkFlow.ConcreteCondition;
using WorkFlow.Configuration;
using WorkFlow.Context;
using WorkFlow.DAO;
using WorkFlow.Entities;
using WorkFlow.Impl;

namespace WorkFlowClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //put this line in a global.asax for web
            WorkFlowConfiguration.Binder.SetRepository(typeof(DAOEmbeddedResource))
                .Setup(x => x.TypeName, "WorkFlow.Json.movimentacao.json , WorkFlowMachine");

            IWorkFlow work = WorkFlowManager.GetManager();
            WorkFlowContext workc = work.GetContext()
                .AddArea("AreaPURCHASEASK")
                .AddSourceState("ATDRAFT");

            IList<string> depth = (IList<string>)work.Run(workc, SearchMode.Depth);

            IList<string> breadth = (IList<string>)work.Run(workc, SearchMode.Breadth);

            WorkFlowContext context = new WorkFlowContext {                 
                 Area = "AreaPURCHASEORDER",
                 Operation = "PURCHASEORDER_CONFIRM_ARRIVE_DESTINY",
                 SourceState = "WAITMOVEMENTDESTINY"
            }.SetCondition(typeof(NewMatch));

            context["Objective"] = new List<string> { "2" };
           
            string state = work.GetNextStatus(context);
        }
    }

    public class NewMatch : MatchCondition
    {
        public override bool CheckConditions(string condition, WorkFlowContext context)
        {
            if (condition == null) return true;

            if (condition.StartsWith("!"))
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition.Substring(1))).FirstOrDefault();
                return !CheckParameters(context, cond);
            }
            else
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition)).FirstOrDefault();
                return CheckParameters(context, cond);
            }
        }
    }
}
