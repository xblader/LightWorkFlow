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
using WorkFlow.ConcreteCondition;

namespace WorkFlow.Impl
{
    /// <summary>
    /// Fornece implementação para a interface WorkFlow usada pelo sistema.
    /// </summary>
    public static class WorkFlowManager
    {
        public static IWorkFlow GetManager()
        {
            return new RunnerManager { Match = new MatchCondition() };
        }

        public static bool Validate(byte[] workflow)
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
