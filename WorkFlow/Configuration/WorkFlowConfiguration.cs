using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.DAO;
using System.Linq.Expressions;
using System.Reflection;

namespace WorkFlow.Configuration
{
    public class WorkFlowConfiguration
    {
        private static WorkFlowConfiguration _instance;
        private static object syncRoot = new Object();
        internal WorkFlowSettings Setting { get; set; }

        public static WorkFlowConfiguration Binder {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new WorkFlowConfiguration();
                    }
                }

                return _instance;
            }
        }       

        public WorkFlowSettings SetRepository(Type repository)
        {
            Setting = new WorkFlowSettings();
            Setting.Repository = repository;
            return Setting;
        }

        internal Type GetRepository()
        {
            return Setting.Repository;
        }
    }    
}
