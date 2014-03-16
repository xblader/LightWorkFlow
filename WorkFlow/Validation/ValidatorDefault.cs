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
            ValidateDuplicatedId(structure);

            ValidateDuplicatedSourceAndArea(structure);

            ValidateDuplicatedConditions(structure);
        }

        private void ValidateDuplicatedConditions(Entities.Structure structure)
        {
            var totalgrouped = structure.Conditions
                .GroupBy(x => x.Name)
                .Where(grp => grp.Count() > 1)
                .Select(x => new { Key = x.Key, List = x.ToList() });

            foreach (var item in totalgrouped)
            {
                string message = string.Join(",", item.List.Select(x => x.Name).ToList());
                throw new DuplicatedNodeException(string.Format("There are duplicated entries for Condition: {0}", message));
            }
        }

        private void ValidateDuplicatedId(Entities.Structure structure)
        {
            var totalgrouped = structure.WorkFlow
                .GroupBy(x => x.Id)
                .Where(grp => grp.Count() > 1)
                .Select(x => new { Key = x.Key, List = x.ToList() });

            foreach (var item in totalgrouped)
            {
                string message = item.Key.ToString();
                throw new DuplicatedNodeException(string.Format("There are duplicated entries for Id: {0}", message));
            }
        }

        private void ValidateDuplicatedSourceAndArea(Entities.Structure structure)
        {
            var totalgrouped = structure.WorkFlow
                .GroupBy(x => new { x.Area, x.SourceState })
                .Where(grp => grp.Count() > 1)
                .Select(x => new { Key = x.Key, List = x.ToList() });

            foreach (var item in totalgrouped)
            {
                string message = string.Join(",", item.List.Select(x => x.Id).ToList());
                throw new DuplicatedNodeException(string.Format("There are duplicated entries for Ids: {0}", message));
            }
        }
    }
}
