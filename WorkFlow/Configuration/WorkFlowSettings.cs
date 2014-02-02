using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace WorkFlow.Configuration
{
    public class WorkFlowSettings
    {
        internal Type Repository { get; set; }
        internal WorkFlowSettingParameter Parameter { get; set; }

        public WorkFlowSettings()
        {
            Parameter = new WorkFlowSettingParameter();
        }

        public WorkFlowSettings Setup(Expression<Func<WorkFlowSettingParameter, object>> memberLamda, string value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(Parameter, value, null);
                }
            }
            return this;
        }
    }

    public class WorkFlowSettingParameter
    {
        public string ConnectionString { get; set; }
        public string Query { get; set; }
        public string TypeName { get; set; }
    }

}
