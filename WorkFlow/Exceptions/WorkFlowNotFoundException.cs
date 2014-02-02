using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Exceptions
{
    public class WorkFlowNotFoundException : Exception
    {
        private string message;

        public WorkFlowNotFoundException(string message)
            : base(message)
        {
            this.message = message;
        }
    }
}
