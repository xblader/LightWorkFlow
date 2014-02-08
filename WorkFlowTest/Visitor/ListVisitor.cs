using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Visitors;
using System.IO;

namespace WorkFlowTest.Visitor
{
    public class ListVisitor : IVisitor
    {        
        private IList<string> transicoes = new List<string>();

        public ListVisitor()
        {      
           
        }

        public void Visit(string status, WorkFlow.Entities.Activity activity, string novostatus)
        {
            string transicao = string.Format("{0}--[{1}]-->{2} ", status, activity.Description, novostatus);            
            transicoes.Add(transicao);
        }

        public object EndVisit()
        {           
            return transicoes;
        }
    }
}
