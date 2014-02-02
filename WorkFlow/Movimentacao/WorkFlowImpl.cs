using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Business;
using WorkFlow.Entities;
using WorkFlow.Business.BusinessProcess;
using WorkFlow.Visitors;
using WorkFlow.Business.Search;
using WorkFlow.Context;
using Newtonsoft.Json;
using WorkFlow.Utils;

namespace WorkFlow.Movimentacao
{
    /// <summary>
    /// Fornece implementação para a interface WorkFlow usada pelo sistema.
    /// </summary>
    public class WorkFlowImpl : IWorkFlow
    {
        BpActivity bpwork = new BpActivity();      

        public string GetInitialStatus(string area)
        {
            return bpwork.GetInitialStatus(area);            
        }

        public string GetNextStatus(string area, string operation, string sourceState = null, WorkFlowContext context = null)
        {
            return bpwork.GetNextStatus(area, operation, sourceState, context);            
        }

        public IList<Activity> GetActivities(string sourceState, string area, WorkFlowContext context = null, ControlAccess.IControlAccess access = null)
        {
            return bpwork.GetActivities(sourceState, area, context, access);
        }

        public IList<string> ListAreas()
        {
            return bpwork.ListAreas();
        }

        public object Run(string area, WorkFlowContext context, SearchMode mode, IVisitor visitor = null)
        {
            string initialstatus = bpwork.GetInitialStatus(area);
            return bpwork.Run(initialstatus, area, context, mode, visitor);
        }


        public string GetActivityDescription(string operation)
        {
            return bpwork.GetActivityDescription(operation);
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
