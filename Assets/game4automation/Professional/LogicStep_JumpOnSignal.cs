// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

namespace game4automation
{
    public class LogicStep_JumpOnSignal: LogicStep
    {
        public string JumpToStep;
        public Signal Signal;
        public bool JumpOn;

        protected new bool NonBlocking()
        {
            return true;
        }
        
        protected override void OnStarted()
        {
            if (Signal != null && (bool) Signal.GetValue() == JumpOn)
                NextStep(JumpToStep);
            else
                NextStep();
        }
    }

}

