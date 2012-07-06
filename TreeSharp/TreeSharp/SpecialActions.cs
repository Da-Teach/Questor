using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSharp
{
    public class ActionAlwaysSucceed : Composite
    {
        public override IEnumerable<RunStatus> Execute(object context)
        {
            yield return RunStatus.Success;
        }
    }

    public class ActionAlwaysFail : Composite
    {
        public override IEnumerable<RunStatus> Execute(object context)
        {
            yield return RunStatus.Failure;
        }
    }
}
