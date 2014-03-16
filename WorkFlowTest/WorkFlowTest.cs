using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkFlow;
using WorkFlow.Entities;
using System.Linq;
using WorkFlow.Exceptions;
using System.IO;
using WorkFlowTest.Visitor;
using WorkFlow.Business.BusinessProcess;
using WorkFlow.Business.Search;
using WorkFlow.Context;
using WorkFlow.DAO;
using WorkFlow.Configuration;
using WorkFlow.Impl;
using WorkFlow.Visitors;

namespace WorkFlowTest
{
    [TestClass]
    public class WorkFlowTest
    {
        private static IWorkFlow work;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            WorkFlowConfiguration.Binder.SetRepository(typeof(DAOEmbeddedResource))
                .Setup(x => x.TypeName, "WorkFlow.Json.workflow.json , LightWorkFlow");

            work = WorkFlowManager.GetManager();
        }

        [TestMethod]
        public void GeInitialStatus()
        {
            string initial = work.GetInitialStatus("AreaPURCHASEASK");
            Assert.AreEqual("ATDRAFT", initial);
        }

        [TestMethod]
        public void GetNextStatus()
        {
            WorkFlowContext context = work.GetContext().SetArea("AreaPURCHASEASK")
                                                        .SetOperation("AskAprovePURCHASEASK")
                                                        .SetSourceState("ATDRAFT");
            string status = work.GetNextStatus(context);
            Assert.AreEqual("WAITINGALLOW", status);
        }

        [TestMethod]
        public void GetNextStatusWithAll()
        {
            WorkFlowContext context = work.GetContext().SetArea("AreaPURCHASEASK")
                                                        .SetOperation("SOLICITAR_CANCEL")
                                                        .SetSourceState("EMITTED");

            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "1" };
            string status = work.GetNextStatus(context);
            Assert.AreEqual("CANCELASKEDPURCHASEASK", status);
        }

        [TestMethod]
        public void GetAtivities()
        {
            WorkFlowContext context = work.GetContext().SetArea("AreaPURCHASEASK")                                                        
                                                        .SetSourceState("ATDRAFT");
            IList<Activity> act = work.GetActivities(context);
            Assert.AreEqual(2, act.Count);
        }

        /// <summary>
        /// Testando os status automáticos
        /// </summary>
        [TestMethod]
        public void GetDestinyWithNoOrigin()
        {
            WorkFlowContext context = work.GetContext().Reset().SetArea("Automatic")
                                                               .SetOperation("LAST_ITEM_CANCELLED_PURCHASEASK");

            string destiny = work.GetNextStatus(context);
            Assert.AreEqual("CANCELLEDPURCHASEASK", destiny);
        }

        [TestMethod]
        public void GetAtivitiesGrid()
        {
            work.GetContext().AddElements("ATDRAFT", "GridPURCHASEASK");
            IList<Activity> act = work.GetActivities(work.GetContext());
            Assert.AreEqual(2, act.Count);
            Assert.IsTrue(act.Any(x => x.Operation.Equals("ALTER_ITEM_PURCHASEASK")));
            Assert.IsTrue(act.Any(x => x.Operation.Equals("EXCLUDE_ITEM_PURCHASEASK")));

            work.GetContext().SetSourceState("EMITTED");
            IList<Activity> emitido = work.GetActivities(work.GetContext());
            Assert.AreEqual(0, emitido.Count);

            work.GetContext().SetSourceState("ATANALYSIS");
            IList<Activity> analise = work.GetActivities(work.GetContext());
            Assert.AreEqual(2, analise.Count);
            Assert.IsTrue(analise.Any(x => x.Operation.Equals("ALTER_ITEM_PURCHASEASK")));
            Assert.IsTrue(analise.Any(x => x.Operation.Equals("CANCEL_ITEM_PURCHASEASK")));
        }

        [TestMethod]
        public void CheckConditionsKeyAndAtLeastOneValueMatch()
        {
            WorkFlowContext context = work.GetContext();

            context["Objective"] = new List<string> { "3" };//destruction
            context["Departament"] = new List<string> { "1" };
            Assert.IsTrue(context.Match.CheckConditions("DESTRUCTION", context));
        }

        [TestMethod]
        public void CheckConditionsKeyNotMatch()
        {
            WorkFlowContext context = work.GetContext();

            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("DESTRUCTION", context));
        }

        [TestMethod]
        public void CheckConditionsKeyMatchButValueDoesnt()
        {
            WorkFlowContext context = work.GetContext();
            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("EXTINTION", context));
        }

        [TestMethod]
        public void SameOriginForDifferentConditions()
        {
            WorkFlowContext context = work.GetContext();
            context.AddElements("EMITTED", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "4" };//deposito
            context["Departament"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("INICIO_ANALYSIS_EXPORTATION")));
            /*testing another condition*/

            WorkFlowContext context2 = work.GetContext();
            context2.AddElements("EMITTED", "AreaPURCHASEASK");
            context2["Objective"] = new List<string> { "2" };//deposito
            context2["Departament"] = new List<string> { "1" };
            IList<Activity> atividades = work.GetActivities(context2);
            Assert.IsTrue(atividades.Count == 2 && atividades.Any(x => x.Operation.Equals("TakePURCHASEASK")));
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherWithStatusInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENT", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.AreEqual(0, lista.Count);
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENTDESTINY", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 1 && lista.Any(x => x.Operation.Equals("RELEASE_MOVIMENTACAO")));
        }

        [TestMethod]
        public void ConditionMatchAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENTDESTINY", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "3" };//destruction
            context["Departament"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("RELEASE_EXTINTION")));
        }

        [TestMethod]
        public void PrintPURCHASEORDERParaDeposit()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITTED", "AreaPURCHASEORDER");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITTED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,PURCHASEORDEREMITTED--[Autorizar Movement]-->MOVEMENTALLOWED ,MOVEMENTALLOWED--[Associate RT]-->None ,MOVEMENTALLOWED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,MOVEMENTALLOWED--[Move Material]-->WAITMOVEMENT ,WAITMOVEMENT--[Associate RT]-->None ,WAITMOVEMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENT--[Disponibilizar para Vistoria]-->ATCHECKING ,ATCHECKING--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Concluir Vistoria]-->WAITMOVEMENTDESTINY ,WAITMOVEMENTDESTINY--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENTDESTINY--[Confirm chegada]-->WAITRETURN ,WAITRETURN--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITRETURN--[Confirm Return]-->RETURNCONFIRMED ,RETURNCONFIRMED--[Confirmation Finishing]-->FINISHED ,RETURNCONFIRMED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Informar Divergência]-->DIFFERENT ,DIFFERENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,DIFFERENT--[Confirm Acerto]-->ATCHECKING ,ATCHECKING--[Ask Apoio Técnico]-->WAITINGIDENTIFICATION ,WAITINGIDENTIFICATION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITINGIDENTIFICATION--[Informar apoio técnico]-->ATCHECKING ,DIFFERENT--[Informar Justificativa]-->MOVEMENTALLOWED ", transicoes);
        }

        [TestMethod]
        public void PrintPURCHASEORDERParaDestruction()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITTED", "AreaPURCHASEORDER");
            context["Objective"] = new List<string> { "3" };//destruction
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITTED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,PURCHASEORDEREMITTED--[Autorizar Movement]-->MOVEMENTALLOWED ,MOVEMENTALLOWED--[Associate RT]-->None ,MOVEMENTALLOWED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,MOVEMENTALLOWED--[Move Material]-->WAITMOVEMENT ,WAITMOVEMENT--[Associate RT]-->None ,WAITMOVEMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENT--[Disponibilizar para Vistoria]-->ATCHECKING ,ATCHECKING--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Concluir Vistoria]-->WAITMOVEMENTDESTINY ,ATCHECKING--[Informar Divergência]-->DIFFERENT ,ATCHECKING--[Ask Apoio Técnico]-->WAITINGIDENTIFICATION ,WAITMOVEMENTDESTINY--[Protocolar Requerimento]-->PROTOCOLREQUIREMENT ,WAITMOVEMENTDESTINY--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,DIFFERENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,DIFFERENT--[Confirm Acerto]-->ATCHECKING ,DIFFERENT--[Informar Justificativa]-->MOVEMENTALLOWED ,WAITINGIDENTIFICATION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITINGIDENTIFICATION--[Informar apoio técnico]-->ATCHECKING ,PROTOCOLREQUIREMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,PROTOCOLREQUIREMENT--[Aguardar Destruição]-->WAITINGDESTRUCTION ,WAITINGDESTRUCTION--[Destruição Efetuada]-->FINISHEDPROCESS ,WAITINGDESTRUCTION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,FINISHEDPROCESS--[Encerrar Destruição]-->FINISHED ,FINISHEDPROCESS--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ", transicoes);
        }

      

        [TestMethod]
        public void PrintPURCHASEASKParaReexportation()
        {
            WorkFlowContext context = work.GetContext().AddElements("ATDRAFT", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "4" };//Reexportation
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("ATDRAFT--[Ask Aproving]-->WAITINGALLOW ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,EMITTED--[Take PURCHASEASK]-->ANALYSISPURCHASEASKEXPORTAÇÃO ,ANALYSISPURCHASEASKEXPORTAÇÃO--[Enviar Questionario]-->WAITINGQUESTIONÁRIO ,WAITINGQUESTIONÁRIO--[Questionario Preenchido]-->ANALYSISPURCHASEASKEXPORTAÇÃO ,ANALYSISPURCHASEASKEXPORTAÇÃO--[Retornar Análise]-->ATANALYSIS ,ATANALYSIS--[Associate PURCHASEORDER]-->PURCHASEORDERCREATED ,PURCHASEORDERCREATED--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERCREATED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelling]-->None ,ATANALYSIS--[Correct PURCHASEASK]-->ATREVISION ,ATREVISION--[Ask Aproving]-->WAITINGALLOW ,WAITINGALLOW--[Não Aprove PURCHASEASK]-->ATREVISION ,ATREVISION--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,WAITINGALLOW--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,ATANALYSIS--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,ANALYSISPURCHASEASKEXPORTAÇÃO--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,WAITINGQUESTIONÁRIO--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,ATDRAFT--[Erase Draft]-->None ", transicoes);
        }

        [TestMethod]
        public void PrintPURCHASEASKParaDeposit()
        {
            WorkFlowContext context = work.GetContext().AddElements("ATDRAFT", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//Reexportation
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("ATDRAFT--[Ask Aproving]-->WAITINGALLOW ,ATDRAFT--[Erase Draft]-->None ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprove PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Take PURCHASEASK]-->ATANALYSIS ,ATREVISION--[Ask Aproving]-->WAITINGALLOW ,ATREVISION--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelling]-->None ,ATANALYSIS--[Associate PURCHASEORDER]-->PURCHASEORDERCREATED ,ATANALYSIS--[Correct PURCHASEASK]-->ATREVISION ,ATANALYSIS--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERCREATED--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERCREATED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void PrintPURCHASEASKParaDepositInWidth()
        {
            WorkFlowContext context = work.GetContext().AddElements("ATDRAFT", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//Reexportation
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("ATDRAFT--[Ask Aproving]-->WAITINGALLOW ,ATDRAFT--[Erase Draft]-->None ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprove PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Take PURCHASEASK]-->ATANALYSIS ,ATREVISION--[Ask Aproving]-->WAITINGALLOW ,ATREVISION--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelling]-->None ,ATANALYSIS--[Associate PURCHASEORDER]-->PURCHASEORDERCREATED ,ATANALYSIS--[Correct PURCHASEASK]-->ATREVISION ,ATANALYSIS--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERCREATED--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERCREATED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void PrintPURCHASEASKParaDepositContext()
        {
            WorkFlowContext context = work.GetContext().AddElements("ATDRAFT", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Departament"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("ATDRAFT--[Ask Aproving]-->WAITINGALLOW ,ATDRAFT--[Erase Draft]-->None ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprove PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Take PURCHASEASK]-->ATANALYSIS ,ATREVISION--[Ask Aproving]-->WAITINGALLOW ,ATREVISION--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelling]-->None ,ATANALYSIS--[Associate PURCHASEORDER]-->PURCHASEORDERCREATED ,ATANALYSIS--[Correct PURCHASEASK]-->ATREVISION ,ATANALYSIS--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERCREATED--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERCREATED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ", transicoes);
                   
        }

        [TestMethod]
        public void PrintRunnerStrategy()
        {
            IWorkFlow workteste = WorkFlowManager.GetManager(typeof(RunnerCustom));

            WorkFlowContext context = workteste
                                    .GetContext()                                    
                                    .AddElements("ATDRAFT", "AreaPURCHASEASK");

            var lista = (List<string>)workteste.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("ATDRAFT--[Ask Aproving]-->WAITINGALLOW ,ATDRAFT--[Erase Draft]-->None ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprove PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,EMITTED--[Take PURCHASEASK]-->ATANALYSIS ,ATREVISION--[Ask Aproving]-->WAITINGALLOW ,ATREVISION--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelling]-->None ,ATANALYSIS--[Associate PURCHASEORDER]-->PURCHASEORDERCREATED ,ATANALYSIS--[Correct PURCHASEASK]-->ATREVISION ,ATANALYSIS--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERCREATED--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERCREATED--[Ask Cancelling]-->CANCELASKEDPURCHASEASK ", transicoes);

        }   
    }

    public class RunnerCustom : RunnerManager
    {
        public override object Run(WorkFlowContext context, SearchMode mode, WorkFlow.Visitors.IVisitor visitor = null)
        {
            if (visitor == null)
                visitor = new DefaultVisitor();

            Queue<string> fila = new Queue<string>();
            List<string> mark = new List<string>();

            fila.Enqueue(context.SourceState);
            mark.Add(context.SourceState);

            while (fila.Count != 0)
            {
                string statusfila = fila.Dequeue();
                context.SourceState = statusfila;
                foreach (var item in this.GetActivities(context).OrderBy(x => x.Operation))
                {
                    context.Operation = item.Operation;
                    string newstatus = this.GetNextStatus(context);
                    visitor.Visit(statusfila, new Activity { Operation = item.Operation, Description = item.Description }, newstatus);

                    if (!mark.Contains(newstatus))
                    {
                        fila.Enqueue(newstatus);
                        mark.Add(newstatus);
                    }
                }
            }

            return visitor.EndVisit();
        }
    }
}
