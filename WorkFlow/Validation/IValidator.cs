using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorkFlow.Entities;

namespace WorkFlow.Validation
{
    public interface IValidator
    {
        void Validate(Structure structure);
    }
}
