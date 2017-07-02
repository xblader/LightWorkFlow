using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkFlow.Entities;

namespace WorkFlow.Validation
{
    public class ValidationResult
    {
        private Condition cond;
        private Evaluate parameter;

        public ValidationResult(Condition cond, Evaluate parameter)
        {
            this.cond = cond;
            this.parameter = parameter;
        }
    }
}
