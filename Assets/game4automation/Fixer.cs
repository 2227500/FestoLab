// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  


using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace game4automation
{
    [SelectionBase]
    //! The Fixer is able to fix Mus as Subcomponents to any Gameobject where the Fixer is attached. As soon as the free moving Mus are colliding or as soon as a Gripper is releasing
    //! the MUs the Fixer will fix the MU. MUs fixed by the Fixer have no Gravity and are purely kinematic.
    public class Fixer : BehaviorInterface,IFix
    {
        public bool UseRayCast; //!< Use Raycasts instead of Box Collider for detecting parts
        [Tooltip("Raycast direction")] [ShowIf("UseRayCast")] public Vector3 RaycastDirection = new Vector3(1, 0, 0); //!< Raycast direction
        [Tooltip("Length of Raycast in mm, Scale is considered")] [ShowIf("UseRayCast")] public float RayCastLength = 100; //!<  Length of Raycast in mm
        [Tooltip("Raycast Layers")] [ShowIf("UseRayCast")] public List<string> RayCastLayers = new List<string>(new string[] {"g4a MU","g4A SensorMU",}); //!< Raycast Layers
        [Tooltip("MU should be fixed")] public bool FixMU; //!< true if MU should be fixed
        [Tooltip("True if MU should be fixed or aligned when Distance between MU and Fixer is minimum (distance is increasing again)")] public bool AlignAndFixOnMinDistance;  //!< true if MU should be fixed or aligned when Distance between MU and Fixer is minimum (distance is increasing again)
        [Tooltip("Releases the fixer if something which is not an MU is colliding with the Fixer")] [ShowIf("FixMU")]public bool ReleaseOnCollissionNonMU = false;  //!< Releases the fixer if something which is not an MU is colliding with the Fixer
        [Tooltip("Align pivot point of MU to Fixer pivot point")] public bool AlignMU; //!< true if pivot Points of MU and Fixer should be aligned
        [Tooltip("Display status of Raycast or BoxCollider")] public bool ShowStatus; //! true if Status of Collider or Raycast should be displayed
        [Tooltip("Opacity of Mesh in case of status display")] [ShowIf("ShowStatus")] [HideIf("UseRayCast")] public float StatusOpacity = 0.2f; //! Opacity of Mesh in case of status display
        [Tooltip("PLCSignal for releasing current MUs and turning Fixer off")]  public PLCOutputBool FixerRelease; //! PLCSignal for releasing current MUs and turning Fixer off
        [Tooltip("PLCSignal for fixing current MUs and turning Fixer off")]  public PLCOutputBool FixerFix; 
        
        private bool nextmunotnull; 
        public List<MU> MUSEntered;
        public List<MU> MUSFixed;
        private MeshRenderer meshrenderer;
        private int layermask;
        private  RaycastHit[] hits;

        private bool fixerreleasenotnull;
        private bool fixerfixnotnull;
        
        // Trigger Enter and Exit from Sensor
        public void OnTriggerEnter(Collider other)
        {
            var mu = other.gameObject.GetComponentInParent<MU>();
            if (mu != null)
            {
                if (!MUSFixed.Contains(mu))
                {
                    MUSEntered.Add(mu);
                    mu.FixerLastDistance = -1;
                }
            }
            if (ReleaseOnCollissionNonMU)
                if (mu == null)
                    Release();
        }
        
        public void OnTriggerExit(Collider other)
        {
            var mu = other.gameObject.GetComponentInParent<MU>();
            if (MUSEntered.Contains(mu))
                MUSEntered.Remove(mu);
        }

        
        
         void Reset()
         {
             if (GetComponent<BoxCollider>())
                 UseRayCast = false;
             else
                 UseRayCast = true;
         }
        
        public void Release()
        {
            var mus = MUSFixed.ToArray();
            for (int i = 0; i < mus.Length; i++)
            {
                Unfix(mus[i]);
            } 
        }
        
        public void Unfix(MU mu)
        {
        
            if (FixMU)
            {
                mu.Unfix();
            }
            MUSFixed.Remove(mu);
            if (ShowStatus && !UseRayCast)
                 meshrenderer.material.color = new Color(1,0,0,StatusOpacity);
        }

        public void Fix(MU mu)
        {
            MUSEntered.Remove(mu);
            MUSFixed.Add(mu);
            if (AlignMU)
            {
                mu.transform.position = transform.position;
                mu.transform.rotation = transform.rotation;
            }
            if (FixMU)
            {
                mu.Fix(this.gameObject);
            }
            else
            {
                Release();
            }
        }

        
        private void AtPosition(MU mu)
        {
            /// Only fix if another fixer element has fixed it - don't fix it if it is fixed by a gripper

            var fixedby = mu.FixedBy;
            Fixer fixedbyfixer = null;
            if (fixedby != null)
                fixedbyfixer = mu.FixedBy.GetComponent<Fixer>();
            if (mu.FixedBy == null || (fixedbyfixer != null && fixedbyfixer != this))
            {
                if (ShowStatus && !UseRayCast)
                       meshrenderer.material.color = new Color(0, 1, 0, StatusOpacity);
                Fix(mu);
            }
        }

        private new void Awake()
        {
            if (!UseRayCast)
            {
                if (GetComponent<BoxCollider>()==null)
                    Error("Fixer neeeds a Box Collider attached to if no Raycast is used!");
            }
            base.Awake(); 
            layermask = LayerMask.GetMask(RayCastLayers.ToArray());
        }
        
        private void Start()
        {
            meshrenderer = GetComponent<MeshRenderer>();
            if (meshrenderer != null && !UseRayCast)
            {
                if (ShowStatus)
                    meshrenderer.material.color = new Color(1, 0, 0, StatusOpacity);
            }

            fixerreleasenotnull = FixerRelease != null;
            fixerfixnotnull = FixerFix != null;
        }
        
        private void Raycast()
        {
            var scale = Game4AutomationController.Scale;
            var globaldir = transform.TransformDirection(RaycastDirection);
            var display = Vector3.Normalize(globaldir) * RayCastLength / scale;
            hits = Physics.RaycastAll(transform.position, globaldir, RayCastLength/scale, layermask,
                QueryTriggerInteraction.UseGlobal);
            if (hits.Length>0)
            {
             
                if (ShowStatus) Debug.DrawRay(transform.position, display ,Color.red,0,true);
            }
            else
            {
                if (ShowStatus) Debug.DrawRay(transform.position, display, Color.yellow,0,true);
            
            }
    
        }

        private float GetDistance(MU mu)
        {
            return Vector3.Distance(mu.gameObject.transform.position, this.transform.position);
        }


        void CheckEntered()
        {
            var entered = MUSEntered.ToArray();
            for (int i = 0; i < entered.Length; i++)
            {
                AtPosition(entered[i]);
            }
        }
        
        void FixedUpdate()
        {
            if (fixerreleasenotnull)
            {
                if (FixerRelease.Value == true)
                {
                    if (MUSFixed.Count > 0)
                    {
                        Release();
                    }
                }
            }
            
            
            if (fixerreleasenotnull)
                if (FixerRelease.Value == true)
                    return;
            
            
            if (fixerfixnotnull)
            {
                if (FixerFix.Value == !true)
                {
                    if (MUSFixed.Count > 0)
                    {
                        Release();
                    }
                }
            }
            
            
            if (fixerfixnotnull)
                if (FixerFix.Value == !true)
                    return;
            
            if (UseRayCast)
            {
                Raycast();
                if (hits.Length > 0)
                {
                    MUSEntered.Clear();
                    foreach (var hit in hits)
                    {
                        var mu = hit.collider.GetComponentInParent<MU>();
                        if (mu != null)
                        {
                            if (!MUSFixed.Contains(mu))
                            {
                                MUSEntered.Add(mu);
                            }
                        }
                    }
                }
            }

            if (AlignAndFixOnMinDistance)
            {
                foreach (var mu in MUSEntered)
                {
                    var distance = GetDistance(mu);
                    if (distance > mu.FixerLastDistance && mu.FixerLastDistance != -1)
                    {
                        AtPosition(mu);
                    }
                    mu.FixerLastDistance = distance;
                }
            }
            else
            {
                  CheckEntered();
            }
        }

     
    }
}