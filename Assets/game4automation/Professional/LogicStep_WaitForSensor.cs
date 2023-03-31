// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

namespace game4automation
{
    public class LogicStep_WaitForSensor: LogicStep
    {
        public Sensor Sensor;
        public bool WaitForOccupied;

        private bool sensornotnull = false;
        
        protected new bool NonBlocking()
        {
            return false;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (sensornotnull==false)
                NextStep();
        }

     
        protected new void Start()
        {
            sensornotnull = Sensor != null;
            base.Start();
        }
        
        private void FixedUpdate()
        {
           if (sensornotnull && StepActive)
               if (Sensor.Occupied==WaitForOccupied)
                   NextStep();
        }
    }

}

