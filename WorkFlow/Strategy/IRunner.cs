using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Strategy
{
    public interface IRunnerStrategy
    {
        void Run(Context.WorkFlowContext context, Visitors.IVisitor visitor);
    }
}
