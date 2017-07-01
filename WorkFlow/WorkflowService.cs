using System;
using Classificador;

namespace Workflow
{
    public class WorkflowService
    {
        public WorkflowService()
        {
            
        }

        public Boolean ExecutaWorkflowEncerramento(IClassificador classe, string estadoAtual, string fato)
        {
            //PermissaoTrabalhoFato ptf = new PermissaoTrabalhoFato(classe);

            //return ptf.EncerrarPT(entidade, fato);
            return true;
        }

    }
}
