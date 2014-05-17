using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Context;

namespace WorkFlow.Command
{
    public interface IWorkFlowCommand
    {
        void Execute(WorkFlowContext context);
    }
}
