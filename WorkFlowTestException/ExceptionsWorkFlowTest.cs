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
               .Setup(x => x.TypeName, "WorkFlowTestException.Json.workflowexception.json , WorkFlowTestException");

            work = WorkFlowManager.GetManager();
        }
    }

    public class ValidatorCustom : IValidator
    {
        public void Validate(Structure structure)
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
