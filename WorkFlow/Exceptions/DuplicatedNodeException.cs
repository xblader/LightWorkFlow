using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Exceptions
{
    public class DuplicatedNodeException : Exception
    {
        private string message;

        public DuplicatedNodeException(string message)
            : base(message)
        {
            this.message = message;
        }
    }
}
