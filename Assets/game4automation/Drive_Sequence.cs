// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace game4automation
{
   
    //! Defines sequentially movement of drives which can set signals or be started by signals
    public class Drive_Sequence : BehaviorInterface
    {
        [System.Serializable]
        public class DriveSequence
        {
            public string Description;
            public Signal SignalToFalseOnStart;
            public Signal WaitForSignal;
            public Drive Drive;
            public float Destination;
            public bool NoWait;
            public float Speed;
            public float WaitAfterStep;
            public Signal FinishedSignal;
    
        }

        public bool StartAtBeginning = true;
        public bool ResetWaitForSignals = true;
        public bool StopAfterEachStep = false;
        
        [SerializeField]
        public List<DriveSequence> Sequence = new List<DriveSequence>();

        [ReadOnly] public int CurrentStep = -1;
        [ReadOnly] public Drive CurrentDrive;
        [ReadOnly] public float CurrentDestination;
        [ReadOnly] public Signal CurrentWaitForSignal;

        private bool waitforsignal = false;
        private bool waitfornextstepbutton = false;
        private bool waitafterstep = false;
        
        // Start is called before the first frame update
        void StartSequzence()
        {
            CurrentStep = -1;
            NextStep();
        }

        void NextStep()
        {
            waitafterstep = false;
            CurrentStep++;
            waitforsignal = false;
            if (CurrentStep > Sequence.Count-1)
            {
                CurrentStep = 0;
            }

            CurrentDrive = Sequence[CurrentStep].Drive;
          
            if (CurrentDrive == null)
                CurrentDrive = this.GetComponent<Drive>();
         
            CurrentDestination = Sequence[CurrentStep].Destination;
            
            if (Sequence[CurrentStep].SignalToFalseOnStart!=null)
            {
                Sequence[CurrentStep].SignalToFalseOnStart.SetValue(false);
            }   
            
            if (Sequence[CurrentStep].FinishedSignal!=null)
            {
                Sequence[CurrentStep].FinishedSignal.SetValue(false);
            }   
            
    
            if (Sequence[CurrentStep].WaitForSignal != null)
            {
                waitforsignal = true;
                CurrentWaitForSignal = Sequence[CurrentStep].WaitForSignal;
            }
            else
            {
                StartDrive();
                if (Sequence[CurrentStep].NoWait)
                    StepFinished();
            }
    
            
        }

        void StartDrive()
        {
            if (Sequence[CurrentStep].Speed!=0)
                CurrentDrive.TargetSpeed =  Sequence[CurrentStep].Speed;
            if (!(CurrentDestination==0 && Sequence[CurrentStep].Speed==0))
                 CurrentDrive.DriveTo(CurrentDestination);
        }
        
        void Start()
        {
            if (StartAtBeginning)
                StartSequzence();
        }

        [Button("Next Step")]
        void ButtonNextStep()
        {
            if (waitfornextstepbutton)
            {
                NextStep();
                waitfornextstepbutton = false;
            }

        }

        void StepFinished()
        {
            if (StopAfterEachStep != true)
            {
                NextStep();
            }
            else
                waitfornextstepbutton = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (CurrentStep > -1)
            {
                if (waitforsignal)
                {
                    if ((bool)CurrentWaitForSignal.GetValue() == true)
                    {
                        waitforsignal = false;
                        StartDrive();
                        if (ResetWaitForSignals)
                            CurrentWaitForSignal.SetValue(false);
                        return;
                    }
                }

                if (ReferenceEquals(CurrentDrive,null) )
                {
                    if (!waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        waitafterstep = true;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        Invoke("StepFinished", wait);
                    }
                }
                else
                {
                    if  (CurrentDrive.CurrentPosition == CurrentDestination && !waitafterstep)
                    {
                        var wait = Sequence[CurrentStep].WaitAfterStep;
                        if (Sequence[CurrentStep].FinishedSignal != null)
                            Sequence[CurrentStep].FinishedSignal.SetValue(true);
                        waitafterstep = true;
                        Invoke("StepFinished", wait);
                    }
                }
           
            }
        }
    }
}

