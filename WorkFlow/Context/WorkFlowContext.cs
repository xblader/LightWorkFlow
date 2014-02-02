using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Context
{
    public class WorkFlowContext
    {
        Dictionary<string, List<string>> parameter = new Dictionary<string, List<string>>();
        public IWorkFlow WorkFlow { get; set; }
        public string Area { get; set; }
        public string SourceState { get; set; }
        public string Operation { get; set; }
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
    }
}
