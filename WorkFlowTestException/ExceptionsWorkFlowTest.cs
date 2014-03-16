using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkFlow;
using WorkFlow.Entities;
using System.Linq;
using WorkFlow.Exceptions;
using System.IO;
using WorkFlow.Business.BusinessProcess;
using WorkFlow.Business.Search;
using WorkFlow.Context;
using WorkFlow.DAO;
using WorkFlow.Configuration;
using WorkFlow.Impl;
using WorkFlow.Validation;

namespace WorkFlowTest
{
    [TestClass]
    public class ExceptionsWorkFlowTest
    {
        private static IWorkFlow work;       

        [TestMethod]
        [ExpectedException(typeof(DuplicatedNodeException))]
        public void RaiseExceptionWhenSourceStateAndAreaAreDuplicated()
        {
            WorkFlowConfiguration.Binder
                .SetRepository(typeof(DAOEmbeddedResource))
                .SetValidator(typeof(ValidatorCustom))
               .Setup(x => x.TypeName, "WorkFlowTestException.Json.sourceAreaException.json , WorkFlowTestException");

            work = WorkFlowManager.GetManager();
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicatedNodeException))]
        public void RaiseExceptionWhenIdsAreDuplicated()
        {
            WorkFlowConfiguration.Binder
                .SetRepository(typeof(DAOEmbeddedResource))                
               .Setup(x => x.TypeName, "WorkFlowTestException.Json.idException.json , WorkFlowTestException");

            work = WorkFlowManager.GetManager();
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicatedNodeException))]
        public void RaiseExceptionWhenConditionsAreDuplicated()
        {
            WorkFlowConfiguration.Binder
                .SetRepository(typeof(DAOEmbeddedResource))                
               .Setup(x => x.TypeName, "WorkFlowTestException.Json.conditionException.json , WorkFlowTestException");

            work = WorkFlowManager.GetManager();
        }
    }

    public class ValidatorCustom : IValidator
    {
        public void Validate(Structure structure)
        {           
            ValidateDuplicatedSourceAndArea(structure);            
        }
        private void ValidateDuplicatedSourceAndArea(Structure structure)
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
