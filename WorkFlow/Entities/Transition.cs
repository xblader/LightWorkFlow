using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.Entities
{
    public class Transition
    {        
        public string Condition { get; set; }        
        public string[] But { get; set; }
        public string DestinyState { get; set; }
        public string Operation { get; set; }
        public string Description { get; set; }        
    }
}



