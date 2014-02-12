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
                .Setup(x => x.TypeName, "WorkFlow.Json.movimentacao.json , WorkFlowMachine");

            work = WorkFlowManager.GetManager();
        }

        [TestMethod]
        public void GetNextStatus()
        {
            WorkFlowContext context = work.GetContext().AddArea("AreaPURCHASEASK")
                                                        .AddOperation("PedirAprovarPURCHASEASK")
                                                        .AddSourceState("EMRASCUNHO");
            string status = work.GetNextStatus(context);
            Assert.AreEqual("WAITINGALLOW", status);
        }

        [TestMethod]
        public void GetNextStatusWithAll()
        {
            WorkFlowContext context = work.GetContext().AddArea("AreaPURCHASEASK")
                                                        .AddOperation("SOLICITAR_CANCEL")
                                                        .AddSourceState("EMITTED");

            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            string status = work.GetNextStatus(context);
            Assert.AreEqual("CANCELASKEDPURCHASEASK", status);
        }

        [TestMethod]
        public void GetAtivities()
        {
            WorkFlowContext context = work.GetContext().AddArea("AreaPURCHASEASK")                                                        
                                                        .AddSourceState("EMRASCUNHO");
            IList<Activity> act = work.GetActivities(context);
            Assert.AreEqual(2, act.Count);
        }

        /// <summary>
        /// Testando os status automáticos
        /// </summary>
        [TestMethod]
        public void GetDestinyWithNoOrigin()
        {
            WorkFlowContext context = work.GetContext().Reset().AddArea("Automatic")
                                                               .AddOperation("LAST_ITEM_CANCELLED_PURCHASEASK");

            string destiny = work.GetNextStatus(context);
            Assert.AreEqual("CANCELLEDPURCHASEASK", destiny);
        }

        [TestMethod]
        public void GetAtivitiesGrid()
        {
            work.GetContext().AddElements("EMRASCUNHO", "GridPURCHASEASK");
            IList<Activity> act = work.GetActivities(work.GetContext());
            Assert.AreEqual(2, act.Count);
            Assert.IsTrue(act.Any(x => x.Operation.Equals("ALTERAR_ITEM_PURCHASEASK")));
            Assert.IsTrue(act.Any(x => x.Operation.Equals("EXCLUIR_ITEM_PURCHASEASK")));

            work.GetContext().AddSourceState("EMITTED");
            IList<Activity> emitido = work.GetActivities(work.GetContext());
            Assert.AreEqual(0, emitido.Count);

            work.GetContext().AddSourceState("EMANALISE");
            IList<Activity> analise = work.GetActivities(work.GetContext());
            Assert.AreEqual(2, analise.Count);
            Assert.IsTrue(analise.Any(x => x.Operation.Equals("ALTERAR_ITEM_PURCHASEASK")));
            Assert.IsTrue(analise.Any(x => x.Operation.Equals("CANCELAR_ITEM_PURCHASEASK")));
        }

        [TestMethod]
        public void CheckConditionsKeyAndAtLeastOneValueMatch()
        {
            WorkFlowContext context = work.GetContext();

            context["Objective"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "1" };
            Assert.IsTrue(context.Match.CheckConditions("DESTRUCTION", context));
        }

        [TestMethod]
        public void CheckConditionsKeyNotMatch()
        {
            WorkFlowContext context = work.GetContext();

            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("DESTRUCTION", context));
        }

        [TestMethod]
        public void CheckConditionsKeyMatchButValueDoesnt()
        {
            WorkFlowContext context = work.GetContext();
            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("EXTINCION", context));
        }

        [TestMethod]
        public void SameOriginForDifferentConditions()
        {
            WorkFlowContext context = work.GetContext();
            context.AddElements("EMITTED", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "4" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("INICIO_ANALISE_EXPORTATION")));
            /*testing another condition*/

            WorkFlowContext context2 = work.GetContext();
            context2.AddElements("EMITTED", "AreaPURCHASEASK");
            context2["Objective"] = new List<string> { "2" };//deposito
            context2["Orgao"] = new List<string> { "1" };
            IList<Activity> atividades = work.GetActivities(context2);
            Assert.IsTrue(atividades.Count == 2 && atividades.Any(x => x.Operation.Equals("AssumirPURCHASEASK")));
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherWithStatusInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENT", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.AreEqual(0, lista.Count);
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENTDESTINY", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 1 && lista.Any(x => x.Operation.Equals("LIBERAR_MOVIMENTACAO")));
        }

        [TestMethod]
        public void ConditionMatchAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("WAITMOVEMENTDESTINY", "PURCHASEORDEROperation");
            context["Objective"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("LIBERAR_EXTINCION")));
        }

        [TestMethod]
        public void ImprimePURCHASEORDERParaDeposito()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITTED", "AreaPURCHASEORDER");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITTED--[Autorizar Movement]-->MOVEMENTALLOWED ,MOVEMENTALLOWED--[Associate RT]-->None ,MOVEMENTALLOWED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,MOVEMENTALLOWED--[Movimentar Material]-->WAITMOVEMENT ,WAITMOVEMENT--[Associate RT]-->None ,WAITMOVEMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENT--[Disponibilizar para Vistoria]-->ATCHECKING ,ATCHECKING--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Informar Divergência]-->DIFFERENT ,DIFFERENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,DIFFERENT--[Confirm Acerto]-->ATCHECKING ,ATCHECKING--[Concluir Vistoria]-->WAITMOVEMENTDESTINY ,WAITMOVEMENTDESTINY--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENTDESTINY--[Confirm chegada]-->WAITRETURN ,WAITRETURN--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITRETURN--[Confirm Return]-->RETURNCONFIRMED ,RETURNCONFIRMED--[Confirmation de Finishing]-->FINISHED ,RETURNCONFIRMED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Solicitar Apoio Técnico]-->WAITINGIDENTIFICATION ,WAITINGIDENTIFICATION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITINGIDENTIFICATION--[Informar apoio técnico]-->ATCHECKING ,DIFFERENT--[Informar Justificativa]-->MOVEMENTALLOWED ,PURCHASEORDEREMITTED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEORDERParaDestruicao()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITTED", "AreaPURCHASEORDER");
            context["Objective"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITTED--[Autorizar Movement]-->MOVEMENTALLOWED ,PURCHASEORDEREMITTED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,MOVEMENTALLOWED--[Associate RT]-->None ,MOVEMENTALLOWED--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,MOVEMENTALLOWED--[Movimentar Material]-->WAITMOVEMENT ,WAITMOVEMENT--[Associate RT]-->None ,WAITMOVEMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITMOVEMENT--[Disponibilizar para Vistoria]-->ATCHECKING ,ATCHECKING--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,ATCHECKING--[Informar Divergência]-->DIFFERENT ,ATCHECKING--[Concluir Vistoria]-->WAITMOVEMENTDESTINY ,ATCHECKING--[Solicitar Apoio Técnico]-->WAITINGIDENTIFICATION ,DIFFERENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,DIFFERENT--[Confirm Acerto]-->ATCHECKING ,DIFFERENT--[Informar Justificativa]-->MOVEMENTALLOWED ,WAITMOVEMENTDESTINY--[Protocolar Requerimento]-->PROTOCOLREQUIREMENT ,WAITMOVEMENTDESTINY--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITINGIDENTIFICATION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,WAITINGIDENTIFICATION--[Informar apoio técnico]-->ATCHECKING ,PROTOCOLREQUIREMENT--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,PROTOCOLREQUIREMENT--[Aguardar Destruição]-->WAITINGDESTRUCTION ,WAITINGDESTRUCTION--[Destruição Efetuada]-->FINISHEDPROCESS ,WAITINGDESTRUCTION--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ,FINISHEDPROCESS--[Encerrar Destruição]-->FINISHED ,FINISHEDPROCESS--[Cancel PURCHASEORDER]-->CANCELLEDPURCHASEORDER ", transicoes);
        }

      

        [TestMethod]
        public void ImprimePURCHASEASKParaReexportacao()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "4" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->WAITINGALLOW ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,EMITTED--[Assumir PURCHASEASK]-->ANALISEPURCHASEASKEXPORTAÇÃO ,ANALISEPURCHASEASKEXPORTAÇÃO--[Enviar Questionario]-->WAITINGQUESTIONÁRIO ,WAITINGQUESTIONÁRIO--[Questionario Preenchido]-->ANALISEPURCHASEASKEXPORTAÇÃO ,ANALISEPURCHASEASKEXPORTAÇÃO--[Retornar Análise]-->EMANALISE ,EMANALISE--[Associate PURCHASEORDER]-->PURCHASEORDERGERADO ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Revisar PURCHASEASK]-->ATREVISION ,ATREVISION--[Solicitar Aprovação]-->WAITINGALLOW ,WAITINGALLOW--[Não Aprovar PURCHASEASK]-->ATREVISION ,ATREVISION--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,WAITINGALLOW--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,EMANALISE--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,ANALISEPURCHASEASKEXPORTAÇÃO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,WAITINGQUESTIONÁRIO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,EMITTED--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDeposito()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->WAITINGALLOW ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprovar PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,EMITTED--[Assumir PURCHASEASK]-->EMANALISE ,EMITTED--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,ATREVISION--[Solicitar Aprovação]-->WAITINGALLOW ,ATREVISION--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associate PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->ATREVISION ,EMANALISE--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDepositoInWidth()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->WAITINGALLOW ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprovar PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,EMITTED--[Assumir PURCHASEASK]-->EMANALISE ,EMITTED--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,ATREVISION--[Solicitar Aprovação]-->WAITINGALLOW ,ATREVISION--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associate PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->ATREVISION ,EMANALISE--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDepositContext()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "AreaPURCHASEASK");
            context["Objective"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->WAITINGALLOW ,WAITINGALLOW--[Emit PURCHASEASK]-->EMITTED ,WAITINGALLOW--[Não Aprovar PURCHASEASK]-->ATREVISION ,WAITINGALLOW--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,EMITTED--[Assumir PURCHASEASK]-->EMANALISE ,EMITTED--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,ATREVISION--[Solicitar Aprovação]-->WAITINGALLOW ,ATREVISION--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,CANCELASKEDPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associate PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->ATREVISION ,EMANALISE--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSFINISHED ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELASKEDPURCHASEASK ", transicoes);
                   
        }
    }
}
