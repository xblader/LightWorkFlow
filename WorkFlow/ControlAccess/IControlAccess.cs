using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkFlow.ControlAccess
{
    public interface IControlAccess
    {
        IList<Entities.Activity> checkAccessActivity(IList<Entities.Activity> ativities);
    }
}
