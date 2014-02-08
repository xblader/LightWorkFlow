using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WorkFlow.Entities;
using WorkFlow.DAO;
using WorkFlow.Configuration;
using WorkFlow.Exceptions;

namespace WorkFlow.Singleton
{
    public class WorkFlowSingleton
    {
        private static WorkFlowSingleton _instance;
        private Structure structure;
        private IDAO dalc = (IDAO)Activator.CreateInstance(WorkFlowConfiguration.Binder.GetRepository());
        private static object syncRoot = new Object();

        private WorkFlowSingleton()
        {
            string json = dalc.GetJson();
            structure = JsonConvert.DeserializeObject<Structure>(json);
        }

        public static WorkFlowSingleton Instance()
        {            
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null) 
                        _instance = new WorkFlowSingleton();
                }
            }

            return _instance;
        }

        public static void DestroyInstance()
        {
            _instance = null;
        }

        public Structure GetStructure()
        {
            return structure;
        }        
    }
}

