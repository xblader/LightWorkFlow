using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Visitors
{
    public interface IVisitor
    {
        void Visit(string status, Entities.Activity activity, string newstatus);
        object EndVisit();
    }
}
