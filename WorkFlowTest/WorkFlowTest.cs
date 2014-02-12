﻿using System;
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
            WorkFlowContext context = work.GetContext().AddArea("BarraBotoesPURCHASEASK")
                                                        .AddOperation("PedirAprovarPURCHASEASK")
                                                        .AddSourceState("EMRASCUNHO");
            string status = work.GetNextStatus(context);
            Assert.AreEqual("AGUARDANDOAPROVAÇÃO", status);
        }

        [TestMethod]
        public void GetNextStatusWithAll()
        {
            WorkFlowContext context = work.GetContext().AddArea("BarraBotoesPURCHASEASK")
                                                        .AddOperation("SOLICITAR_CANCELAMENTO")
                                                        .AddSourceState("EMITIDO");

            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            string status = work.GetNextStatus(context);
            Assert.AreEqual("CANCELAMENTOSOLICITADOPURCHASEASK", status);
        }

        [TestMethod]
        public void GetAtivities()
        {
            WorkFlowContext context = work.GetContext().AddArea("BarraBotoesPURCHASEASK")                                                        
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
            WorkFlowContext context = work.GetContext().Reset().AddArea("Automatico")
                                                               .AddOperation("ULTIMO_ITEM_CANCELADO_PURCHASEASK");

            string destiny = work.GetNextStatus(context);
            Assert.AreEqual("CANCELADOPURCHASEASK", destiny);
        }

        [TestMethod]
        public void GetAtivitiesGrid()
        {
            work.GetContext().AddElements("EMRASCUNHO", "GridPURCHASEASK");
            IList<Activity> act = work.GetActivities(work.GetContext());
            Assert.AreEqual(2, act.Count);
            Assert.IsTrue(act.Any(x => x.Operation.Equals("ALTERAR_ITEM_PURCHASEASK")));
            Assert.IsTrue(act.Any(x => x.Operation.Equals("EXCLUIR_ITEM_PURCHASEASK")));

            work.GetContext().AddSourceState("EMITIDO");
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

            context["Finalidade"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "1" };
            Assert.IsTrue(context.Match.CheckConditions("DESTRUICAO", context));
        }

        [TestMethod]
        public void CheckConditionsKeyNotMatch()
        {
            WorkFlowContext context = work.GetContext();

            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("DESTRUICAO", context));
        }

        [TestMethod]
        public void CheckConditionsKeyMatchButValueDoesnt()
        {
            WorkFlowContext context = work.GetContext();
            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            Assert.IsFalse(context.Match.CheckConditions("EXTINCAO", context));
        }

        [TestMethod]
        public void SameOriginForDifferentConditions()
        {
            WorkFlowContext context = work.GetContext();
            context.AddElements("EMITIDO", "BarraBotoesPURCHASEASK");
            context["Finalidade"] = new List<string> { "4" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("INICIO_ANALISE_EXPORTACAO")));
            /*testing another condition*/

            WorkFlowContext context2 = work.GetContext();
            context2.AddElements("EMITIDO", "BarraBotoesPURCHASEASK");
            context2["Finalidade"] = new List<string> { "2" };//deposito
            context2["Orgao"] = new List<string> { "1" };
            IList<Activity> atividades = work.GetActivities(context2);
            Assert.IsTrue(atividades.Count == 2 && atividades.Any(x => x.Operation.Equals("AssumirPURCHASEASK")));
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherWithStatusInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("AGUARDAMOVIMENTAÇÃO", "PURCHASEORDEROperation");
            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.AreEqual(0, lista.Count);
        }

        [TestMethod]
        public void ConditionAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("AGUARDAMOVIMENTAÇÃODESTINO", "PURCHASEORDEROperation");
            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 1 && lista.Any(x => x.Operation.Equals("LIBERAR_MOVIMENTACAO")));
        }

        [TestMethod]
        public void ConditionMatchAndStatusAllTogetherStatusNotInBut()
        {
            WorkFlowContext context = work.GetContext().AddElements("AGUARDAMOVIMENTAÇÃODESTINO", "PURCHASEORDEROperation");
            context["Finalidade"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "1" };
            IList<Activity> lista = work.GetActivities(context);
            Assert.IsTrue(lista.Count == 2 && lista.Any(x => x.Operation.Equals("LIBERAR_EXTINCAO")));
        }

        [TestMethod]
        public void ImprimePURCHASEORDERParaDeposito()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITIDO", "BarraBotoesPURCHASEORDER");
            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITIDO--[Autorizar Movimentação]-->MOVIMENTAÇÃOAUTORIZADA ,MOVIMENTAÇÃOAUTORIZADA--[Associar RT]-->None ,MOVIMENTAÇÃOAUTORIZADA--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,MOVIMENTAÇÃOAUTORIZADA--[Movimentar Material]-->AGUARDAMOVIMENTAÇÃO ,AGUARDAMOVIMENTAÇÃO--[Associar RT]-->None ,AGUARDAMOVIMENTAÇÃO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDAMOVIMENTAÇÃO--[Disponibilizar para Vistoria]-->EMVISTORIA ,EMVISTORIA--[Solicitar Apoio Técnico]-->AGUARDANDOIDENTIFICAÇÃO ,AGUARDANDOIDENTIFICAÇÃO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDANDOIDENTIFICAÇÃO--[Informar apoio técnico]-->EMVISTORIA ,EMVISTORIA--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,EMVISTORIA--[Informar Divergência]-->DIVERGENTE ,DIVERGENTE--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,DIVERGENTE--[Confirmar Acerto]-->EMVISTORIA ,EMVISTORIA--[Concluir Vistoria]-->AGUARDAMOVIMENTAÇÃODESTINO ,AGUARDAMOVIMENTAÇÃODESTINO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDAMOVIMENTAÇÃODESTINO--[Confirmar chegada]-->AGUARDARETORNO ,AGUARDARETORNO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDARETORNO--[Confirmar Retorno]-->RETORNOCONFIRMADO ,RETORNOCONFIRMADO--[Confirmação de Encerramento]-->ENCERRADO ,RETORNOCONFIRMADO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,DIVERGENTE--[Informar Justificativa]-->MOVIMENTAÇÃOAUTORIZADA ,PURCHASEORDEREMITIDO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEORDERParaDestruicao()
        {
            WorkFlowContext context = work.GetContext().AddElements("PURCHASEORDEREMITIDO", "BarraBotoesPURCHASEORDER");
            context["Finalidade"] = new List<string> { "3" };//destruicao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("PURCHASEORDEREMITIDO--[Autorizar Movimentação]-->MOVIMENTAÇÃOAUTORIZADA ,PURCHASEORDEREMITIDO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,MOVIMENTAÇÃOAUTORIZADA--[Associar RT]-->None ,MOVIMENTAÇÃOAUTORIZADA--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,MOVIMENTAÇÃOAUTORIZADA--[Movimentar Material]-->AGUARDAMOVIMENTAÇÃO ,AGUARDAMOVIMENTAÇÃO--[Associar RT]-->None ,AGUARDAMOVIMENTAÇÃO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDAMOVIMENTAÇÃO--[Disponibilizar para Vistoria]-->EMVISTORIA ,EMVISTORIA--[Solicitar Apoio Técnico]-->AGUARDANDOIDENTIFICAÇÃO ,EMVISTORIA--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,EMVISTORIA--[Informar Divergência]-->DIVERGENTE ,EMVISTORIA--[Concluir Vistoria]-->AGUARDAMOVIMENTAÇÃODESTINO ,AGUARDANDOIDENTIFICAÇÃO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDANDOIDENTIFICAÇÃO--[Informar apoio técnico]-->EMVISTORIA ,DIVERGENTE--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,DIVERGENTE--[Confirmar Acerto]-->EMVISTORIA ,DIVERGENTE--[Informar Justificativa]-->MOVIMENTAÇÃOAUTORIZADA ,AGUARDAMOVIMENTAÇÃODESTINO--[Protocolar Requerimento]-->PROTOCOLOREQUERIMENTO ,AGUARDAMOVIMENTAÇÃODESTINO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,PROTOCOLOREQUERIMENTO--[Aguardar Destruição]-->AGUARDANDODESTRUIÇÃO ,PROTOCOLOREQUERIMENTO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,AGUARDANDODESTRUIÇÃO--[Destruição Efetuada]-->ENCERRAMENTOPROCESSO ,AGUARDANDODESTRUIÇÃO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ,ENCERRAMENTOPROCESSO--[Encerrar Destruição]-->ENCERRADO ,ENCERRAMENTOPROCESSO--[Cancelar PURCHASEORDER]-->CANCELADOPURCHASEORDER ", transicoes);
        }

      

        [TestMethod]
        public void ImprimePURCHASEASKParaReexportacao()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "BarraBotoesPURCHASEASK");
            context["Finalidade"] = new List<string> { "4" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Depth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,AGUARDANDOAPROVAÇÃO--[Emitir PURCHASEASK]-->EMITIDO ,EMITIDO--[Assumir PURCHASEASK]-->ANALISEPURCHASEASKEXPORTAÇÃO ,ANALISEPURCHASEASKEXPORTAÇÃO--[Enviar Questionario]-->AGUARDANDOQUESTIONÁRIO ,AGUARDANDOQUESTIONÁRIO--[Questionario Preenchido]-->ANALISEPURCHASEASKEXPORTAÇÃO ,ANALISEPURCHASEASKEXPORTAÇÃO--[Retornar Análise]-->EMANALISE ,EMANALISE--[Associar PURCHASEORDER]-->PURCHASEORDERGERADO ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSOENCERRADO ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,CANCELAMENTOSOLICITADOPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Revisar PURCHASEASK]-->EMREVISÃO ,EMREVISÃO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,AGUARDANDOAPROVAÇÃO--[Não Aprovar PURCHASEASK]-->EMREVISÃO ,EMREVISÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,AGUARDANDOAPROVAÇÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMANALISE--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,ANALISEPURCHASEASKEXPORTAÇÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,AGUARDANDOQUESTIONÁRIO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMITIDO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDeposito()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "BarraBotoesPURCHASEASK");
            context["Finalidade"] = new List<string> { "2" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,AGUARDANDOAPROVAÇÃO--[Emitir PURCHASEASK]-->EMITIDO ,AGUARDANDOAPROVAÇÃO--[Não Aprovar PURCHASEASK]-->EMREVISÃO ,AGUARDANDOAPROVAÇÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMITIDO--[Assumir PURCHASEASK]-->EMANALISE ,EMITIDO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMREVISÃO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,EMREVISÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,CANCELAMENTOSOLICITADOPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associar PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->EMREVISÃO ,EMANALISE--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSOENCERRADO ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDepositoInWidth()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "BarraBotoesPURCHASEASK");
            context["Finalidade"] = new List<string> { "2" };//Reexportacao
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,AGUARDANDOAPROVAÇÃO--[Emitir PURCHASEASK]-->EMITIDO ,AGUARDANDOAPROVAÇÃO--[Não Aprovar PURCHASEASK]-->EMREVISÃO ,AGUARDANDOAPROVAÇÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMITIDO--[Assumir PURCHASEASK]-->EMANALISE ,EMITIDO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMREVISÃO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,EMREVISÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,CANCELAMENTOSOLICITADOPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associar PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->EMREVISÃO ,EMANALISE--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSOENCERRADO ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ", transicoes);
        }

        [TestMethod]
        public void ImprimePURCHASEASKParaDepositContext()
        {
            WorkFlowContext context = work.GetContext().AddElements("EMRASCUNHO", "BarraBotoesPURCHASEASK");
            context["Finalidade"] = new List<string> { "2" };//deposito
            context["Orgao"] = new List<string> { "2" };

            var lista = (List<string>)work.Run(context, SearchMode.Breadth, new ListVisitor());
            string transicoes = string.Join(",", lista.ToArray());
            Assert.AreEqual("EMRASCUNHO--[Apagar Rascunho]-->None ,EMRASCUNHO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,AGUARDANDOAPROVAÇÃO--[Emitir PURCHASEASK]-->EMITIDO ,AGUARDANDOAPROVAÇÃO--[Não Aprovar PURCHASEASK]-->EMREVISÃO ,AGUARDANDOAPROVAÇÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMITIDO--[Assumir PURCHASEASK]-->EMANALISE ,EMITIDO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,EMREVISÃO--[Solicitar Aprovação]-->AGUARDANDOAPROVAÇÃO ,EMREVISÃO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,CANCELAMENTOSOLICITADOPURCHASEASK--[Recusar Cancelamento]-->None ,EMANALISE--[Associar PURCHASEORDER]-->PURCHASEORDERGERADO ,EMANALISE--[Revisar PURCHASEASK]-->EMREVISÃO ,EMANALISE--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ,PURCHASEORDERGERADO--[Encerrando PURCHASEASK]-->PROCESSOENCERRADO ,PURCHASEORDERGERADO--[Solicitar Cancelamento]-->CANCELAMENTOSOLICITADOPURCHASEASK ", transicoes);
                   
        }
    }
}
