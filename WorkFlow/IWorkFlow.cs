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
        string GetInitialStatus(string tipo);
        IList<Activity> GetActivities(WorkFlowContext context, IControlAccess access = null);
        IList<string> ListAreas();
        object Run(WorkFlowContext context, SearchMode mode, IVisitor visitor = null);
        string GetActivityDescription(string operacao);
        WorkFlowContext GetContext();
    }
}
