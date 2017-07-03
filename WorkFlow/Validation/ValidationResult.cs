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
        public Condition Condition { get; set; }
        public Evaluate Parameter { get; set; }

        public ValidationResult(Condition condition, Evaluate parameter)
        {
            this.Condition = condition;
            this.Parameter = parameter;
        }
    }
}
