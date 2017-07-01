using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Entities
{
    public class Evaluate
    {
        public string Key { get; set; }
        public string Operator { get; set; }
        public IList<string> Value { get; set; }
    }
}
