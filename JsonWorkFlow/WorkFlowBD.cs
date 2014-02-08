using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD
{
    public static class WorkFlowBD
    {
        /// <summary>
        /// Lendo do Banco de dados. Ele vem no formato texto.
        /// </summary>
        /// <returns></returns>
        public static string ReadJsonFromBD()
        {
            string result;
            using (Stream stream = typeof(WorkFlowBD).Assembly.GetManifestResourceStream("BD.Json.movimentacao.json"))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }

    }
}

