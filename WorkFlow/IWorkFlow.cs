using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Entities;
using WorkFlow.ControlAccess;
using WorkFlow.Visitors;
using WorkFlow.Business.Search;
using WorkFlow.Context;

namespace WorkFlow
{
    public interface IWorkFlow
    {
        string GetNextStatus(WorkFlowContext context);
        string GetInitialStatus(string area);
        IList<Activity> GetActivities(WorkFlowContext context, IControlAccess access = null);
        IList<string> ListAreas();
        IMatchCondition Match { get; set; }
        object Run(WorkFlowContext context, SearchMode mode, IVisitor visitor = null);
        string GetActivityDescription(string operation);
        WorkFlowContext GetContext();
    }
}
