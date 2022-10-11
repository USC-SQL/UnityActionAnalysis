using UnityEngine;

namespace UnityActionAnalysis
{
    public class InstrInputSimulator : InputSimulator
    {
        public InstrInputSimulator(InputManagerSettings inputManagerSettings, MonoBehaviour context) :
            base(inputManagerSettings, context)
        {
            Reset();
        }
        
        public override void Reset()
        {
            InstrInput.StopSimulation();
            InstrInput.SetInputManagerSettings(inputManagerSettings);
            InstrInput.StartSimulation(context);
        }

        public override void SimulateDown(KeyCode keyCode)
        {
            InstrInput.SimulateKeyDown(keyCode);
        }

        public override void SimulateUp(KeyCode keyCode)
        {
            InstrInput.SimulateKeyUp(keyCode);
        }

        public override void Dispose()
        {
            InstrInput.StopSimulation();
        }
    }
}
