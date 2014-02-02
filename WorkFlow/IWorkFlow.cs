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
        string GetNextStatus(string area, string operacao, string statusOrigem = null, WorkFlowContext context = null);
        string GetInitialStatus(string tipo);
        IList<Activity> GetActivities(string origem, string area, WorkFlowContext context = null, IControlAccess access = null);
        IList<string> ListAreas();
        object Run(string area, WorkFlowContext context, SearchMode mode, IVisitor visitor = null);
        string GetActivityDescription(string operacao);
        bool Validate(byte[] workflow);
    }
}
