using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSharp
{
    public class WaitContinue : Wait
    {
        public WaitContinue(int timeoutSeconds, CanRunDecoratorDelegate runFunc, Composite child)
            : base(timeoutSeconds, runFunc, child)
        {
        }

        public WaitContinue(CanRunDecoratorDelegate runFunc, Composite child)
            : base(runFunc, child)
        {
        }

        public WaitContinue(int timeoutSeconds, Composite child)
            : base(timeoutSeconds, child)
        {
        }

        public override IEnumerable<RunStatus> Execute(object context)
        {
            while (DateTime.Now < _end)
            {
                if (Runner != null)
                {
                    if (Runner(context))
                    {
                        break;
                    }
                }
                else
                {
                    if (CanRun(context))
                    {
                        break;
                    }
                }

                yield return RunStatus.Running;
            }

            if (DateTime.Now < _end)
            {
                yield return RunStatus.Success;
                yield break;
            }

            DecoratedChild.Start(context);
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);
            if (DecoratedChild.LastStatus == RunStatus.Success)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            yield return RunStatus.Success;
            yield break;
        }
    }
}
