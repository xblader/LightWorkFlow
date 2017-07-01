using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Context;
using WorkFlow.Entities;

namespace WorkFlow.Command
{
    public interface IEvaluatorCommand
    {
        IEvaluatorCommand Successor { get; set; }
        bool Execute(Evaluate item, WorkFlowContext context);
    }
}
