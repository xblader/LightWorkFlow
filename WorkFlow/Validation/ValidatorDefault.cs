using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Exceptions;

namespace WorkFlow.Validation
{
    internal class ValidatorDefault : IValidator
    {
        public void Validate(Entities.Structure structure)
        {
            var totalgrouped = structure.WorkFlow.GroupBy(x => new { x.Area, x.SourceState }).Count();
            var totalnodes = structure.WorkFlow.Count();

            if (totalgrouped != totalnodes)
            {
                throw new DuplicatedNodeException("There are some duplicated nodes.");
            }            
        }
    }
}
