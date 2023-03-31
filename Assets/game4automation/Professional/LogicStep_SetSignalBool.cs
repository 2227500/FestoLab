// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

namespace game4automation
{
    public class LogicStep_SetSignalBool : LogicStep
    {
        public Signal Signal;
        public bool SetToTrue;

        private bool signalnotnull = false;
        
        protected new bool NonBlocking()
        {
            return true;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (signalnotnull)
                Signal.SetValue((bool)SetToTrue);
            NextStep();
        }
        
        protected new void Start()
        {
            signalnotnull = Signal != null;
            base.Start();
        }
        
      
    }

}

