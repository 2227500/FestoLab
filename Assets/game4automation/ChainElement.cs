using UnityEngine;

using game4automation;
using NaughtyAttributes;


namespace game4automation
{
    [HelpURL("https://game4automation.com/documentation/current/chainelement.html")]
    [SelectionBase]
//! An element which is transported by a chain (moving along the spline on the chain)
    public class ChainElement : Game4AutomationBehavior
    {
        [Header(("Settings"))]
     
        public bool
            AlignWithChain = true; //!< true if the chainelement needs to align with the chain tangent while moving

        public bool
            MoveRigidBody = true; //!< needs to be set to true if chainelements has colliders which should make parts move physically
        
        [ShowIf("AlignWithChain")]
        [InfoBox("Z of object to tangent, AlignVector or AlignObjectZ = up")]
        public Vector3 AlignVector = new Vector3(1, 0, 0); //!< additinal rotation for the alignment
        [ShowIf("AlignWithChain")]
        public GameObject AlignObjectLocalZUp;
        [ShowIf("AlignWithChain")][InfoBox("Debug Green = Tangent, Red = Up")]
        public bool DebugDirections;
        
        [NaughtyAttributes.ReadOnly] public Drive ConnectedDrive; //!< Drive where the chain is connected to
        [NaughtyAttributes.ReadOnly] public float StartPosition; //!< Start position of this chain element
        [NaughtyAttributes.ReadOnly] public Chain Chain; //!< Chain where this chainelement belongs to
        [NaughtyAttributes.ReadOnly] public float Position; //!< Current position of this chain element
        [NaughtyAttributes.ReadOnly] public float RelativePosition; //!< Relative position of this chain element

        private Vector3 _targetpos;
        private Quaternion targetrotation;
        private Vector3 tangentforward;
        private Game4AutomationController game4Automation;
        private bool chainnotnull = false;
        private bool alignobjectnotnull = false;
        
        private Rigidbody _rigidbody;
        
        public void SetPosition()
        {
            RelativePosition = Position / Chain.Length;
            var positon  = Chain.GetPosition(RelativePosition);

            if (MoveRigidBody)
                _rigidbody.MovePosition(positon);
            else
                transform.position = positon;
            _targetpos = transform.position;
            if (AlignWithChain)
            {
                Quaternion rotation = new Quaternion();
                var globaltangent = Chain.GetTangent(RelativePosition);
                Vector3 align = AlignVector;
                
                if (alignobjectnotnull)
                {
                    align = AlignObjectLocalZUp.transform.forward;
                }

                if (DebugDirections)
                {
                    Debug.DrawRay(transform.position, globaltangent, Color.green);
                    Debug.DrawRay(transform.position, align, Color.red);
                }

                rotation = Quaternion.LookRotation(globaltangent, align);
                if (MoveRigidBody)
                    _rigidbody.MoveRotation(rotation);
                else
                    transform.rotation = rotation;
            }
        }

        public void UpdatePosition(float deltaTime)
        {
            Position = ConnectedDrive.CurrentPosition + StartPosition;

            if (Position > Chain.Length)
            {
                var rounds = Position / Chain.Length;
                Position = (Position % Chain.Length);
            }

            SetPosition();
        }

     

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            alignobjectnotnull = AlignObjectLocalZUp != null;
           
            if (Chain != null)
            {
                chainnotnull = true;
                SetPosition();
            }
            else
                chainnotnull = false;

        }

        private void Update()
        {
            if (chainnotnull)
                UpdatePosition(Time.deltaTime);
        }
    }
}