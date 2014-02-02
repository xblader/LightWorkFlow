using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WorkFlow.Configuration;
using WorkFlow.Exceptions;
using System.Reflection;

namespace WorkFlow.DAO
{
    public class DAOEmbeddedResource : IDAO
    {
        public string GetJson()
        {
            string result;
            string typeName = WorkFlowConfiguration.Binder.Setting.Parameter.TypeName;
            if (typeName == null) { throw new TypeNameNotFoundException("TypeName was not found. Check configuration of the workflow."); }

            Assembly assembly = Assembly.Load(typeName.Split(',')[1].Trim());

            if (assembly == null) throw new Exception("An error occurred in attempting to load assembly.");

            using (Stream stream = assembly.GetManifestResourceStream(typeName.Split(',')[0].Trim()))
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
