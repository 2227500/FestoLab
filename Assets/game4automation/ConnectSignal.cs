// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using UnityEngine;

namespace game4automation
{
    //! Behavior model which is just connecting an PLCOutput to an PLCInput
    public class ConnectSignal : BehaviorInterface
    {
        public Signal ConnectedSiganl;
     
        private bool connectedsignalnotnull;

        private Signal thissignal;
    
        // Start is called before the first frame update
        void Start()
        {
            thissignal = GetComponent<Signal>();
            connectedsignalnotnull = thissignal != null;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (connectedsignalnotnull)
            {
                thissignal.SetValue(ConnectedSiganl.GetValue());
            }
            
        }
    }
}

