// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System.Collections.Generic ;
using NaughtyAttributes;

namespace game4automation
{
    public class LogicStep_WaitForDrivesAtTarget: LogicStep
    {
        [ReorderableList] public List<Drive> Drives;
      
        
        protected new bool NonBlocking()
        {
            return false;
        }
        
        protected override void OnStarted()
        {
            State = 50;
            if (Drives == null )
                NextStep();
        }
        
        
        private void FixedUpdate()
        {
            if (!StepActive)
                return;
            
            bool nextstep = true;
            foreach (var drive in Drives)
            {
                if (!drive.IsAtTarget)
                    nextstep = false;
            }
            if (nextstep)
                NextStep();
        }
    }

}

