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
using WorkFlow.Validation;
using WorkFlow.Visitors;

namespace WorkFlowClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //put this line in a global.asax for web
            WorkFlowConfiguration.Binder.SetRepository(typeof(DAOEmbeddedResource))
                .Setup(x => x.TypeName, "WorkFlow.Json.workflow.json , LightWorkFlow");

            IWorkFlow work = WorkFlowManager.GetManager();//getting instance of workflow. 
            
 /*
 {
 "Id": 36,     
 "SourceState": "ATDRAFT",
 "StateOrder": "Initial",
 "Transitions": [{ Operation:"AskAprovePURCHASEASK", Description:"Ask Aproving" , "DestinyState": "WAITINGALLOW"},
					  { Operation:"CancelDraft", Description:"Erase Draft" , "DestinyState": "None"}],        
              
 "Area": "AreaPURCHASEASK"
    },
*/
            //1) Getting Initial State

            string initialstate = work.GetInitialStatus("AreaPURCHASEASK");//ATDRAFT            
//======================================================================================================
            //2) listing operations that can be done from a specific state.

            WorkFlowContext workoperation = work.GetContext()
               .SetArea("AreaPURCHASEASK")
               .SetSourceState("ATDRAFT");

            IList<Activity> operations = work.GetActivities(workoperation);//AskAprovePURCHASEASK, CancelDraft

            string description = work.GetActivityDescription("AskAprovePURCHASEASK");//Ask Aproving
//=======================================================================================================
            //3) Get Next Status

            WorkFlowContext workgetnextstatus = work.GetContext()
               .SetArea("AreaPURCHASEASK")
               .SetSourceState("ATDRAFT")
               .SetOperation("AskAprovePURCHASEASK");

            string nextstatus = work.GetNextStatus(workgetnextstatus);//WAITINGALLOW
//========================================================================================================
            //4)Runner - walk throughout workflow in depth and breadth

            //setting area and initial state from which runner will start to look at.
            WorkFlowContext workc = work.GetContext()
                .SetArea("AreaPURCHASEASK")
                .SetSourceState("ATDRAFT");

            var depth = work.Run(workc, SearchMode.Depth);//all transitions printed in a depth search mode.

            work.GetContext()
                .SetArea("AreaPURCHASEASK")
                .SetSourceState("ATDRAFT");

            var breadth = (IList<string>)work.Run(workc, SearchMode.Breadth);//all transitions printed in a breadth search mode.


            //4.1 configuring visitor

            WorkFlowContext workvisitor = work.GetContext()
               .SetArea("AreaPURCHASEASK")
               .SetSourceState("ATDRAFT");

            var workvisitordepth = work.Run(workc, SearchMode.Depth, new CustomVisitor());//ok

//========================================================================================================
            //5) setting conditions
            /*
             * {
                  "Name": "EXTINTION",
                  "Parameters": [
                    {
                      "Key": "Objective",
                      "Value": [
                        "4",
                        "3",
                        "7",
                        "5",
                        "9",
                        "8"
                      ]
                    }
                  ]
                },
            {
             "Id": 10,      
             "SourceState": "WAITMOVEMENTDESTINY",
             "Transitions": [{ Condition:"!EXTINTION", Operation:"PURCHASEORDER_CONFIRM_ARRIVE_DESTINY", Description:"Confirm chegada" , "DestinyState": "WAITRETURN"},					  
                            { Condition: "EXTINTION", Operation:"PROTOCOLR_REQUIREMENT", Description:"Protocolar Requerimento" , "DestinyState": "PROTOCOLREQUIREMENT"}],    
             "Area": "AreaPURCHASEORDER"
             },
                        */

            WorkFlowContext workoperationcond = work.GetContext()
               .SetArea("AreaPURCHASEORDER")
               .SetSourceState("WAITMOVEMENTDESTINY");

            workoperationcond["Objective"] = new List<string> { "4" };

            IList<Activity> operationsExtintion = work.GetActivities(workoperationcond);
            //PROTOCOLR_REQUIREMENT,PURCHASEORDER_CANCEL

            workoperationcond["Objective"] = new List<string> { "1" };

            IList<Activity> operationsNoExtintion = work.GetActivities(workoperationcond);
            //PURCHASEORDER_CONFIRM_ARRIVE_DESTINY,PURCHASEORDER_CANCEL


            //PURCHASEORDER_CANCEL shows up here because SourceState is configured with "All" in the area "AreaPURCHASEORDER".
            //if you dont want this operation shows up in a set of states, use "But" inside operation.

            //6) Setting a new condition match class

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
        public override bool CheckConditions(string condition, WorkFlowContext context, IList<ValidationResult> results = null)
        {
            if (condition == null) return true;

            if (condition.StartsWith("!"))
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition.Substring(1))).FirstOrDefault();
                return !CheckParameters(context, cond, results);
            }
            else
            {
                Condition cond = GetNode().Conditions.Where(x => x.Name.Equals(condition)).FirstOrDefault();
                return CheckParameters(context, cond, results);
            }
        }
    }

    public class CustomVisitor : IVisitor
    {
        public void Visit(string status, Activity activity, string newstatus)
        {
            Console.WriteLine(string.Format("{0}==>{1}==>{2}", status, activity.Description, newstatus));
        }

        public object EndVisit()
        {
            return "ok";
        }
    }
}
