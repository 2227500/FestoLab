// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using UnityEngine;

namespace game4automation
{
    public class LogicStep_StartDriveSpeed : LogicStep
    {
        public Drive drive;
        public float Speed;
        
        protected new bool NonBlocking()
        {
            return true;
        }

        protected override void OnStarted()
        {
            if (drive != null)
            {
                drive.TargetSpeed = Mathf.Abs(Speed);
                if (Speed > 0)
                {
                    drive.JogForward = true;
                    drive.JogBackward = false;
                }

                if (Speed == 0)
                {
                    drive.JogForward = false;
                    drive.JogBackward = false;
                }

                if (Speed < 0)
                {
                    drive.JogForward = false;
                    drive.JogBackward = true;
                }
            }

            NextStep();
            
        }
        
    }

}

