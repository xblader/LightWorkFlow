using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Command;
using WorkFlow.ConcreteCondition;
using WorkFlow.Entities;

namespace WorkFlow.Context
{
    public class WorkFlowContext
    {
        Dictionary<string, List<string>> parameter = new Dictionary<string, List<string>>();

        private Structure node;
        private IWorkFlowCommand commandtype;
        public IMatchCondition Match { get; set; }

        public string Area { get; set; }
        public string Operation { get; set; }
        public string SourceState { get; set; }

        public WorkFlowContext()
        {
            node = WorkFlow.Singleton.WorkFlowSingleton.Instance().GetStructure();
        }

        /// <summary>
        /// Reset WorkFlow Context
        /// </summary>
        /// <returns></returns>
        public WorkFlowContext Reset()
        {
            this.Area = String.Empty;
            this.Operation = String.Empty;
            this.SourceState = String.Empty;
            parameter.Clear();
            return this;
        }

        public WorkFlowContext SetArea(string area)
        {
            this.Area = area;
            return this;
        }

        public WorkFlowContext SetSourceState(string source)
        {
            this.SourceState = source;
            return this;
        }

        public WorkFlowContext SetOperation(string operation)
        {
            this.Operation = operation;
            return this;
        }

        public Structure GetNode()
        {
            return node;
        }

        public List<string> this[string index]
        {
            get
            {
                return parameter[index];
            }

            set
            {
                parameter[index] = value;
            }
        }

        public int Count
        {
            get { return parameter.Count; }
        }

        public Dictionary<string, List<string>>.KeyCollection Keys
        {
            get {
                return parameter.Keys;
            }
        }

        public WorkFlowContext SetCondition(Type type)
        {
            this.Match = (IMatchCondition)Activator.CreateInstance(type);
            return this;
        }

        public WorkFlowContext SetCondition(IMatchCondition matchCondition)
        {
            this.Match = matchCondition;
            return this;
        }

        public WorkFlowContext AddElements(string sourceState, string area)
        {
            this.SourceState = sourceState;
            this.Area = area;
            return this;
        }

        public WorkFlowContext SetCommand(Type type)
        {
            this.commandtype = (IWorkFlowCommand)Activator.CreateInstance(type);
            return this;
        }

        public WorkFlowContext Execute()
        {
            commandtype.Execute(this);
            return this;
        }
    }
}
