// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using NaughtyAttributes;
namespace game4automation
{
    public class LogicStep_StartDriveTo : LogicStep
    {
        public Drive drive;
        [OnValueChanged("EditorPosition")] public float Destination;
        public bool Relative = false;
        [OnValueChanged("LiveEditStart")] public bool LiveEdit = false;
        protected new bool NonBlocking()
        {
            return true;
        }
        
        private void LiveEditStart()
        {
            if (drive!=null)
                if (LiveEdit)
                {
               
                    drive.StartEditorMoveMode();
                    EditorPosition();
                }
                else
                    drive.EndEditorMoveMode();
        }
        

        private void EditorPosition()
        {
            if (drive != null)
            {
                if (LiveEdit)
                {
              
                    drive.SetPositionEditorMoveMode(Destination);
                }
            }
        }

        protected override void OnStarted()
        {
            State = 0;
            if (drive != null)
            {
                var des = Destination;
                if (Relative)
                    des = drive.CurrentPosition + Destination;
                drive.DriveTo(des);
            }
            
            NextStep();
          
        }
    }

}

