using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Exceptions
{
    public class RepositoryNotFoundException : Exception
    {
        private string message;

        public RepositoryNotFoundException(string message)
            : base(message)
        {
            this.message = message;
        }
    }
}
