using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Exceptions
{
    public class StatusNotFoundException : Exception
    {
        private string message;

        public StatusNotFoundException(string message):base(message)
        {
            this.message = message;
        }
    }
}
