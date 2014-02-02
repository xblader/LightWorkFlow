using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Entities
{
    public class Node
    {
        public int Id { get; set; }
        public IList<Transition> Transitions { get; set; }
        public string SourceState { get; set; }
        public string StateOrder { get; set; }
        public string Area { get; set; }
    }
}
