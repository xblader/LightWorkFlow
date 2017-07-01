using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Entities
{
    public class Condition
    {
        public string Name { get; set; }
        public IList<Evaluate> Parameters { get; set; }
    }
}
