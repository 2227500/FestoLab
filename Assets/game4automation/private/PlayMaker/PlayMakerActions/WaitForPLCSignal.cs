// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using game4automation;

#if GAME4AUTOMATION_PLAYMAKER

namespace HutongGames.PlayMaker.Actions
{

	[ActionTarget(typeof(Drive), "gameObject")]              
	[ActionCategory("Game4Automation")]
	[Tooltip("Wait for a PLC signal")]
public class WaitForPLCSignal : FsmStateAction
{
		public FsmOwnerDefault Signal;
		public FsmBool WaitforBool;
		public FsmFloat WaitforFloat;
		public FsmInt WaitforInt;

		private Signal _signal;
		
		public override void Reset()
		{
			base.Reset();
			if (this.State != null)
			this.State.ColorIndex = 4;
		}


		public override string ErrorCheck()
		{
			string error = "";
      
			if (Fsm.GetOwnerDefaultTarget(Signal)==null)
			{
				error = "Game4Automation no Signal component selected";
			}
			else
			{
				if (Fsm.GetOwnerDefaultTarget(Signal).GetComponent<Signal>()==null)
				{
					error = "Game4Automation Signnal component missing at this GameObject";
				}
			}

			return error;

		}
		
		public override void OnEnter()
		{
		
			_signal = Fsm.GetOwnerDefaultTarget(Signal).GetComponent<Signal>();
		
		}

		public override void OnUpdate()
		{
			if (_signal is PLCOutputInt || _signal is PLCInputInt)
			{
				if ((int) _signal.GetValue() == WaitforInt.Value)		
					Finish();
			}

			if (_signal is PLCOutputFloat || _signal is PLCInputFloat)
			{
				if ((float) _signal.GetValue() == WaitforFloat.Value)		
					Finish();
			}

			if (_signal is PLCOutputBool || _signal is PLCInputBool)
			{
				if ((bool) _signal.GetValue() == WaitforBool.Value)		
					Finish();
			}
			
		}
}

}
#endif
