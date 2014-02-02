using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Exceptions
{
    public class TypeNameNotFoundException : Exception
    {
        private string message;

        public TypeNameNotFoundException(string message)
            : base(message)
        {
            this.message = message;
        }
    }
}
