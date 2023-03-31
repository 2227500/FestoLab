// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using UnityEngine;

namespace game4automation
{
    [CreateAssetMenu(fileName = "CameraPos", menuName = "game4automation/Add Camera Position", order = 1)]
    //! Scriptable object for saving camera positions (user views)
    public class CameraPos : ScriptableObject
    {
        public string Description;
        public Vector3 CameraRot;
        public Vector3 TargetPos; 
        public float CameraDistance; 
        
  
        public void SaveCameraPosition(SceneMouseNavigation mousenav)
        {
            CameraRot = mousenav.currentRotation.eulerAngles;
            CameraDistance = mousenav.currentDistance; 
            TargetPos = mousenav.target.position;
        }
    }

}

