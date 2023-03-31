// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

      using System;
      using NaughtyAttributes;
using UnityEngine;
      using Object = UnityEngine.Object;

      namespace game4automation
{
    //! Transport Surface - this class is needed together with Drives to model conveyor systems. The transport surface is transporting
    //! rigid bodies which are colliding with its surface
    [HelpURL("https://game4automation.com/documentation/current/transportsurface.html")]
    public class TransportSurface : BaseTransportSurface
    {
        #region Public Variables

        public Vector3 TransportDirection; //!< The direction in local coordinate system of Transport surface - is initialized normally by the Drive
        public float TextureScale = 10; //!< The texture scale what influcences the texture speed - needs to be set manually 
        
        public RigidbodyConstraints ConstraintsEnter;
        public RigidbodyConstraints ConstraintsExit;
        public bool Radial = false;
        [HideInInspector] public Drive UseThisDrive;
        public float speed = 0; //!< the current speed of the transport surface - is set by the drive 
        [InfoBox("Standard Setting for layer is g4a Transport")]
        [OnValueChanged("RefreshReferences")] 
        public string Layer = "g4a Transport";
        [InfoBox("For Best performance unselect UseMeshCollider, for good transfer between conveyors select this")]
        [OnValueChanged("RefreshReferences")] 
        public bool UseMeshCollider = false;
        public Drive ParentDrive; //!< needs to be set to true if transport surface moves based on another drive - transport surface and it's drive are not allowed to be a children of the parent drive.
        public delegate void
            OnEnterExitDelegate(Collision collission,TransportSurface surface); //!< Delegate function for GameObjects entering the Sensor.

        public event OnEnterExitDelegate OnEnter;
        public event OnEnterExitDelegate OnExit;


        #endregion

        #region Private Variables
        private MeshRenderer _meshrenderer;
        private Collider _collider;
        private Rigidbody _rigidbody;
        private bool _isMeshrendererNotNull;
        private bool parentdrivenotnull;
        private Transform _parent;
        private Vector3 parentposbefore;
        private Quaternion parentrotbefore;
        private Quaternion parenttextrotbefore;
        #endregion

        #region Public Methods

        //! Gets a center point on top of the transport surface
        public Vector3 GetMiddleTopPoint()
        {
            
            var collider = gameObject.GetComponent<Collider>();
            if (collider!=null)
            {
                var vec = new Vector3(collider.bounds.center.x, collider.bounds.center.y + collider.bounds.extents.y,
                    collider.bounds.center.z);
                return vec;
            }
            else
                return Vector3.zero;
        }

        //! Sets the speed of the transport surface (used by the drive)
        public void SetSpeed(float _speed)
        {
            speed = _speed;
        }

        #endregion

        #region Private Methods

        private void OnCollisionEnter(Collision other)
        {
          // Global.DebugArrow(other.rigidbody.transform.position, other.rigidbody.velocity, Color.green, 0.5f);
            other.rigidbody.constraints = ConstraintsEnter;
            var mu = other.gameObject.GetComponentInParent<MU>();
            mu.TransportSurfaces.Add(this);
            if (OnEnter!=null)
             OnEnter.Invoke(other,this);
        }

        private void OnCollisionExit(Collision other)
        {
         //   Global.DebugArrow(other.rigidbody.transform.position,other.rigidbody.velocity,Color.red,0.5f);
         var mu = other.gameObject.GetComponentInParent<MU>();
            if (mu.TransportSurfaces.Count==0)
                other.rigidbody.constraints = ConstraintsExit;
            if (OnExit!=null)
               OnExit.Invoke(other,this);
        }

        private void RefreshReferences()
        {
            var _mesh = GetComponent<MeshCollider>();
            var _box = GetComponent<BoxCollider>();
            if (UseMeshCollider)
            {
                if (_box!=null)
               
                    DestroyImmediate(_box);
                if (_mesh==null)
                {
                    _mesh = gameObject.AddComponent<MeshCollider>();
                }
            }
            else
            {
                if (_mesh!=null)
                    DestroyImmediate(_mesh);
                if (_box==null)
                {
                    _box= gameObject.AddComponent<BoxCollider>();
                }
            }
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            _collider = gameObject.GetComponent<Collider>();
            _meshrenderer = gameObject.GetComponent<MeshRenderer>();

        }


        private void Reset()
        {
            gameObject.layer = LayerMask.NameToLayer(Layer);
            RefreshReferences();

            // Add transport surface to drive if a drive is existing in this or an upper object
            if (UseThisDrive != null)
            {
                UseThisDrive.AddTransportSurface(this);
                return;
            }

            var drive = gameObject.GetComponentInParent<Drive>();
            if (drive != null)
                drive.AddTransportSurface(this);
        }
        
        [Button("Destroy Transport Surface")]
        private void DestroyTransportSurface()
        {
            var drive = gameObject.GetComponentInParent<Drive>();
            if (drive != null)
                drive.RemoveTransportSurface(this);
            Object.DestroyImmediate(this);
        }
        

        void Start()
        {
            RefreshReferences();
            _isMeshrendererNotNull = _meshrenderer != null;
            Reset();
            SetSpeed(speed);
            parentposbefore = Vector3.zero;
            parentrotbefore = Quaternion.identity;
            parenttextrotbefore  = Quaternion.identity;
            parentdrivenotnull = ParentDrive != null;
        }
      

        void Update()
        {
            if (speed != 0) 
            {
                Vector3 mov = Vector3.zero;
                if (parentdrivenotnull)
                {
                    if (parenttextrotbefore == Quaternion.identity)
                    {
                        parenttextrotbefore = ParentDrive.transform.rotation;
                    }
                    var parentrot = ParentDrive.transform.rotation;
                    var deltarot = parentrot * Quaternion.Inverse(parenttextrotbefore);
                    var newrot = deltarot * _rigidbody.rotation;
                    mov = newrot * TransportDirection * TextureScale* Time.time * speed *
                        Game4AutomationController.SpeedOverride  / Game4AutomationController.Scale;
                }
                else
                {
                    mov = TextureScale * TransportDirection * Time.time * speed *
                        Game4AutomationController.SpeedOverride  / Game4AutomationController.Scale;
                }
                
                Vector2 vector2 =  new Vector2();
                if (!Radial)
                {
                    var globalrot = this.transform.rotation.eulerAngles;
                    Vector3 vector3 = new Vector3(mov.x, mov.z, 0);
                    var textdir = Quaternion.Euler(0, 0, globalrot.y) * vector3;
                    vector2 = new Vector2(textdir.x, textdir.y);
                }
                else
                {
                    vector2 = new Vector2(mov.x, mov.y);
                }
                if (parentdrivenotnull)
                   parenttextrotbefore = ParentDrive.transform.rotation;
                if (_isMeshrendererNotNull)
                {
                    _meshrenderer.material.mainTextureOffset = vector2; }
            }
        }
     
        void FixedUpdate()
        {

                if (!Radial)
                {
                    Vector3 newpos, mov;
                    newpos = _rigidbody.position;
                    
                    // Linear Conveyor
                    if (parentdrivenotnull)
                    {
                        if (parentposbefore == Vector3.zero)
                        {
                            parentposbefore = ParentDrive.transform.position;
                        }
                        if (parentrotbefore == Quaternion.identity)
                        {
                            parentrotbefore = ParentDrive.transform.rotation;
                        }
                        var dir = TransportDirection;
                        var parentpos = ParentDrive.transform.position;
                        var deltaparent = parentpos - parentposbefore;
                        var parentrot = ParentDrive.transform.rotation;
                        
                        mov = parentrot*dir * Time.fixedDeltaTime * speed *
                              Game4AutomationController.SpeedOverride /
                              Game4AutomationController.Scale;
                        
                        var dirtotal = mov + deltaparent;   // ParentDrive separate
                        var dirback = -mov;                // ParentDrive separate
                       
                        _rigidbody.position = (_rigidbody.position + dirback );
                        Physics.SyncTransforms();
                        _rigidbody.MovePosition(_rigidbody.position + dirtotal);
                        _rigidbody.MoveRotation(parentrot.normalized);
                        
                        parentposbefore = ParentDrive.transform.position;
                        parentrotbefore = ParentDrive.transform.rotation;
                    }
                    else
                    {
                        if (speed != 0)
                        {
                            mov = TransportDirection * Time.fixedDeltaTime * speed *
                                  Game4AutomationController.SpeedOverride /
                                  Game4AutomationController.Scale;
                            _rigidbody.position = (_rigidbody.position - mov);
                            Physics.SyncTransforms();
                            _rigidbody.MovePosition(_rigidbody.position + mov);
                        }
                    }
                }
                else
                {
                    Quaternion nextrot;
                    // Radial Conveyor
                    if (ParentDrive!=null)
                    {
                        Error("Not implemented!");
                    }
                    else
                    {
                        if (speed != 0)
                        {
                            _rigidbody.rotation = _rigidbody.rotation * Quaternion.AngleAxis(
                                -speed * Time.fixedDeltaTime *
                                Game4AutomationController.SpeedOverride, transform.InverseTransformVector(TransportDirection));
                            nextrot = _rigidbody.rotation * Quaternion.AngleAxis(
                                +speed * Time.fixedDeltaTime * Game4AutomationController.SpeedOverride,
                                transform.InverseTransformVector(TransportDirection));
                            _rigidbody.MoveRotation(nextrot);
                        }
                    }
                }
           
        }
        #endregion
    }
}