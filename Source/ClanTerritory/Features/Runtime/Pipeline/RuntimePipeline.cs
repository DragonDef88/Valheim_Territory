using System.Collections.Generic;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Runtime.Pipeline
{
    internal sealed class RuntimePipeline
    {
        private readonly List<IRuntimeStep> _steps;

        public RuntimePipeline()
        {
            _steps = new List<IRuntimeStep>();
        }

        public void AddStep(IRuntimeStep step)
        {
            if (step == null)
                return;

            _steps.Add(step);
        }

        public RuntimeState Execute(RuntimeState currentState)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                IRuntimeStep step = _steps[i];

                if (step.InputState != currentState)
                    continue;

                ModLog.Info(
                    "[Runtime Pipeline] Executing step: " +
                    step.GetType().Name);

                step.Execute();

                return step.OutputState;
            }

            return currentState;
        }
    }
}