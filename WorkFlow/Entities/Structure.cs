using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Entities
{
    public class Structure
    {
        public IList<Condition> Conditions { get; set; }
        public IList<Node> WorkFlow { get; set; }
    }
}
