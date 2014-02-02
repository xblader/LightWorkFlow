using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Exceptions
{
    public class StatusException : Exception
    {
        private string message;

        public StatusException(string message)
        {
            this.message = message;
        }
    }
}
