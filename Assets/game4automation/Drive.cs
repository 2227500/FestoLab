// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

#if GAME4AUTOMATION_INTERACT
using XdeEngine.Core;
using XdeEngine.Core.Monitoring;
#endif

namespace game4automation
{
    [SelectionBase]
    //! The drive is moving components including all sub components along the local axis of the game object.
    //! Rotational and linear movements are possible. A drive can be enhanced by DriveBehaviours which are adding special
    //! behaviours as well as Input and Output signals to drives.
    [HelpURL("https://game4automation.com/documentation/current/drive.html")]
    public class Drive : BaseDrive
    {
        #region PublicVariables

        [Header("Settings")] [OnValueChanged("CalculateVectors")]
        public DIRECTION
            Direction; //!< The direction in local coordinate system of the GameObject where the drive is attached to.

        [OnValueChanged("CalculateVectors")]
        public bool ReverseDirection; //!< Set to *true* if Direction needs to be inverted.

        [Tooltip("Offeset for defining another Drive 0 position. Drive will start at simulation start at Offset.")]
        public float Offset; //!< Start offset of the drive from zero position in millimeters or degrees.

        public float StartPosition; //!< Start Position off the Drive 

        public float
            SpeedScaleTransportSurface =
                1; //!< Scale of the Speed for radial transportsurfaces to feed in mm/s on radius

        [Tooltip(
            "Should be normally turned off. If set to true the RigidBodies are moved. Use it if moving part has attached colliders. If false the transforms are moved")]
        public bool
            MoveThisRigidBody =
                false; //!< If set to true the RigidBodies are moved (use it if moving) part has attached colliders, if false the transforms are moved

        [NaughtyAttributes.ReadOnly] public bool EditorMoveMode = false;

        [ReorderableList] public List<TransportSurface>
            TransportSurfaces; //!< The transport surface the drive is controlling. Is null if drive is not controlling a transport surface.

        [BoxGroup("Limits")] public bool UseLimits;

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float LowerLimit = 0; //! Lower Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public float UpperLimit = 1000; //! Lower Drive Limit, Upper and Lower = 0 if this should not be used

        [ShowIf("UseLimits")] [BoxGroup("Limits")]
        public bool JumpToLowerLimitOnUpperLimit = false;

        [ShowIf("UseLimits")]
        [BoxGroup("Limits")]
        [Tooltip("If assigned the Raycast measurment is the basis for the drive Limits")]
        public Sensor LimitRayCast;

        [Space(10)] [BoxGroup("Acceleration")]
        public bool UseAcceleration = false; //!< If set to true the drive uses the acceleration

        [BoxGroup("Acceleration")] [ShowIf("UseAcceleration")]
        public bool
            SmoothAcceleration = false; //!< if set to true the drive uses smooth acceleration with a sinoide function

        [ShowIf("UseAcceleration")] [BoxGroup("Acceleration")]
        public float Acceleration = 100; //!< The acceleration in millimeter per second^2


        [Header("Drive IO's")] public bool JogForward = false; //!< A jog bit for jogging forward with the Target Speed
        public bool JogBackward = false; //!< A jog bit for jogging backwards with the Target Speed
        public float TargetPosition; //!< The target position of the Drive
        public float TargetSpeed = 100; //!< The target speed in millimeter / second of the Drive

        public bool
            TargetStartMove = false; //!< When changing to true the Drive is starting to move to the TargetPosition

        [HideInInspector]
        public bool
            BlockDestination =
                true; //!< If Block Drive is true it will not drive to its Target Positon, Jogging is possible

        public bool ResetDrive = false; //!< Resets the Drive to the zero position and stops all movements
        public bool _StopDrive = false; //!< Stops the Drive at the current position
        [ReadOnly] public float CurrentSpeed; //!< The current speed of the drive
        [ReadOnly] public float CurrentPosition; //!< The current position of the drive
        [ReadOnly] public bool IsStopped = false; //!< Is true if Drive is stopped
        [ReadOnly] public bool IsRunning = false; //!< Is true if Drive is running
        [ReadOnly] public bool IsAtTargetSpeed = false; //!< Is true if Drive is running
        [ReadOnly] public bool IsAtTarget = false; //!< Is true if Drive is at target position
        [ReadOnly] public bool IsAtLowerLimit = false; //!< Is true if Drive is jogging and reaching lower Limit
        [ReadOnly] public bool IsAtUpperLimit = false; //!< Is true if Drive is jogging and reaching upper Limit 
        [HideInInspector] public bool HideGizmos = false;

        // XDE Integration
#if GAME4AUTOMATION_INTERACT
        [HideInInspector]
        public XdeUnitJointMonitor jointmonitor;
        [HideInInspector]
        public XdeUnitJointPDController jointcontroller;
#endif
#if !GAME4AUTOMATION_INTERACT
        [HideInInspector]
#endif
        public bool UseInteract = false;

        [HideInInspector] public bool IsRotation = false;

        #endregion

        #region Private Variables

        private bool _jogactive;
        private float _lastspeed;
        private float _currentdestination;
        private float _timestartacceleration;
        private double _currentacceleration;
        private bool _laststartdrivetotarget;
        private bool _isdrivingtotarget = false;
        private bool _drivetostarted = false;
        private float _lastcurrentposition;
        private bool _istransportsurface = false;
        private bool _lastisattarget = false;
        private float _currentstoppos;
        private bool _stopjogging = false;

        private Vector3 _localdirection;
        private Vector3 _positiondirection;
        private Vector3 _globaldirection;
        private Vector3 _localdirectionscale;
        private Vector3 _localstartpos;
        private Vector3 _localstartrot;
        private float _localscale;
        private Rigidbody _rigidbody;
        private Vector3 _rotationpoint;
        private TransportSurface[] _transportsurfaces;
        private Vector3 _globalpos;
        private Quaternion _globalrot;
        private bool _lastjog;
        private bool _limitraycastnotnull;

        private bool articulatedbodynotnull;
        private ArticulationBody articulatedbody;

        private bool _accelerationstarted = false;
        private bool _decelerationstarted = false;

        #endregion

        #region Public Events

        public delegate void
            OnAtPositionEvent(Drive drive); //!< Delegate function for the Drive reaching the desitionationPosition

        public event OnAtPositionEvent OnAtPosition;

        public delegate void OnJumpToLowerLimitEvent(Drive drive);

        public event OnJumpToLowerLimitEvent OnJumpToLowerLimit;

        #endregion

        #region Public Methods

#if GAME4AUTOMATION_INTERACT
        [Button("Kinematize (Interact)")]
        public void Kinematize()
        {
            CalculateVectors();
            Game4AutomationPhysics.Kinematize(gameObject);
        }

        [Button("Unkinematize (Interact)")]
        public void Uninematize()
        {
            Game4AutomationPhysics.UnKinematize(gameObject);
        }
#endif

        //! Starts the drive to move forward with the target speed.
        public void Forward()
        {
            JogForward = true;
            JogBackward = false;
        }

        //! Starts the drive to move forward with the target speed.
        public void Backward()
        {
            JogForward = false;
            JogBackward = true;
        }

        //! Starts the drive to drive to the given Target with the target speed.
        public void DriveTo(float Target)
        {
            BlockDestination = false;
            TargetPosition = Target;
            _currentdestination = TargetPosition;
            TargetStartMove = true;
            IsAtTarget = false;
            _drivetostarted = true;
            /*_StopDrive = false;*/
        }

        //! Starts the drive - it will speed up with sinoide if turned on

        public void Accelerate()
        {
            _accelerationstarted = true;
            IsAtTarget = false;
            _decelerationstarted = false;
            _timestartacceleration = Time.time;
            _StopDrive = false;
            IsStopped = false;
        }


        public void Decelerate()
        {
            _decelerationstarted = true;
            _accelerationstarted = false;
            _timestartacceleration = Time.time;
            IsAtTarget = false;
            _StopDrive = false;
            IsStopped = false;
        }

        //! Stops the drive at the current position
        public void Stop()
        {
            TargetStartMove = false;
            _decelerationstarted = false;
            _accelerationstarted = false;
            _currentacceleration = 0;
            IsRunning = false;
            JogForward = false;
            JogBackward = false;
            CurrentSpeed = 0;
            _StopDrive = false;
            IsStopped = true;
        }

        //! Gets the axis vector of the drive
        public Vector3 GetLocalDirection()
        {
            return _localdirection;
        }

        public void StartEditorMoveMode()
        {
            CalculateVectors();
            EditorMoveMode = true;
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, true);
#endif
        }

        public void SetPositionEditorMoveMode(float editorposition)
        {
            if (EditorMoveMode)
            {
                if (Game4AutomationController == null)
                    Game4AutomationController = UnityEngine.Object.FindObjectOfType<Game4AutomationController>();
                CurrentPosition = editorposition;
                SetPosition();
            }
        }

        public void EndEditorMoveMode()
        {
#if UNITY_EDITOR
            Global.SetLockObject(this.gameObject, false);
#endif
            CurrentPosition = 0;
            SetPosition();
            EditorMoveMode = false;
        }


        //! Gets the start position of the drive in local scale
        public Vector3 GetStartPos()
        {
            return _localstartpos;
        }

        //! Gets the start position of the drive in local scale
        public Vector3 GetStartRot()
        {
            return _localstartrot;
        }

        //! Sets the position of the drive to 0
        public void SetDriveTransformToZero()
        {
            CalculateVectors();
            if (!IsRotation)
            {
                transform.localPosition = _localdirection * 0;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(_localdirection * 0);
            }
        }

        //! Sets the position of the drive to the given value
        //! @param value the position the drive in millimeters or degrees
        public void SetDriveTransformToValue(float value)
        {
            CalculateVectors();
            if (!IsRotation)
            {
                transform.localPosition = _localdirection * value;
            }
            else
            {
                transform.localRotation = Quaternion.Euler(_localdirection * value);
            }
        }


        //! Adds a Transport Surface to the Drive
        public void AddTransportSurface(TransportSurface trans)
        {
            if (TransportSurfaces == null)
                TransportSurfaces = new List<TransportSurface>();

            if (!TransportSurfaces.Contains(trans))
                TransportSurfaces.Add(trans);
        }

        //! Removes a Transport Surface fron the Drive
        public void RemoveTransportSurface(TransportSurface trans)
        {
            if (TransportSurfaces != null)
                if (TransportSurfaces.Contains(trans))
                    TransportSurfaces.Remove(trans);
        }

        #endregion

        #region PrivateMethods

        public void CalculateVectors()
        {
            _localdirection = DirectionToVector(Direction);
            _globaldirection = transform.TransformDirection(_localdirection);
            if (!ReferenceEquals(transform.parent, null))
            {
                _positiondirection = transform.parent.transform.InverseTransformDirection(_globaldirection);
            }
            else
            {
                _positiondirection = _globaldirection;
            }

            if (transform.parent != null)
                _localscale = GetLocalScale(transform.parent.transform, Direction);
            else
                _localscale = 1;

            _localstartpos = transform.localPosition;
            _localstartrot = transform.localEulerAngles;
            if (ReverseDirection)
            {
                _globaldirection = -_globaldirection;
                _localdirection = -_localdirection;
                _positiondirection = -_positiondirection;
            }

            IsRotation = false;
            if (Direction == DIRECTION.RotationX || Direction ==
                DIRECTION.RotationY || Direction == DIRECTION.RotationZ)
            {
                IsRotation = true;
            }

#if GAME4AUTOMATION_INTERACT
            if (UseInteract && !Application.isPlaying)
                Game4AutomationPhysics.Kinematize(gameObject);
#endif
        }


        private void SetPosition()
        {
            if (Direction == DIRECTION.Virtual)
            {
                float scale = 1;
                if (articulatedbodynotnull)
                {
                    if (articulatedbody.jointType != ArticulationJointType.RevoluteJoint)
                        scale = 1/Game4AutomationController.Scale;
                    ArticulationDrive currentDrive = articulatedbody.xDrive;
                    if (CurrentPosition > currentDrive.upperLimit)
                    {
                        currentDrive.target = currentDrive.upperLimit;
                    }
                    else if (CurrentPosition < currentDrive.lowerLimit)
                    {
                        currentDrive.target = currentDrive.lowerLimit;
                    }
                    else
                    {
                        currentDrive.target = CurrentPosition*scale;
                    }
                    articulatedbody.xDrive = currentDrive;
                }
                return;
            }
            
            if (!UseInteract)
            {
                if (!_istransportsurface)
                {
                    if (!IsRotation)
                    {
                        
                        Vector3 localpos = _localstartpos +
                                           _positiondirection *
                                           ((CurrentPosition + Offset) / Game4AutomationController.Scale) /
                                           _localscale;

                        if (MoveThisRigidBody)
                        {
                            if (!ReferenceEquals(transform.parent, null))
                                _globalpos = transform.parent.TransformPoint(localpos);
                            else
                                _globalpos = localpos;
                            _rigidbody.MovePosition(_globalpos);
                        }
                        else
                        {
                            transform.localPosition = localpos;
                        }
                    }
                    else
                    {
                        Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));
                        Quaternion localrot =
                            Quaternion.Euler(_localstartrot + _localdirection * (CurrentPosition + Offset));
                        if (MoveThisRigidBody)
                        {
                            if (!ReferenceEquals(transform.parent, null))
                            {
                                _globalrot = transform.parent.rotation * localrot;
                                _globalpos = transform.parent.TransformPoint(_localstartpos);
                                _rigidbody.MovePosition(_globalpos);
                            }
                            else
                            {
                                _globalrot = localrot;
                            }

                            _rigidbody.MoveRotation(_globalrot);
                        }
                        else
                            transform.localRotation = localrot;
                    }
                }
            }
            else
            {
#if GAME4AUTOMATION_INTERACT
                   Game4AutomationPhysics.SetPosition(this,CurrentPosition);
#endif
            }
        }


#if GAME4AUTOMATION_INTERACT

        public override void AwakeAlsoDeactivated()
        {
            Game4AutomationPhysics.EnableDrive(this,this.enabled);
        }
        
#endif
        
        private new void Awake()
        {
            IsAtTarget = true;
            BlockDestination = true;
            _limitraycastnotnull = LimitRayCast != null;
            base.Awake();
        }

        // When Script is added or reset ist pushed
        private void Reset()
        {
#if GAME4AUTOMATION_INTERACT
            Game4AutomationPhysics.InitDrive(this);
#endif

            if (!UseInteract)
            {
                /// Automatically create RigidBody if not there
                _rigidbody = gameObject.GetComponent<Rigidbody>();
                if (_rigidbody == null)
                {
                    _rigidbody = gameObject.AddComponent<Rigidbody>();
                }

                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Automatically add Transportsurface if one is existent in this or any sub object and no other drive is in between
            var surfaces = gameObject.GetComponentsInChildren<TransportSurface>();
            foreach (var surface in surfaces)
            {
                // check if this drive is directly upwards to surface
                if (surface.GetComponentInParent<Drive>() == this)
                {
                    AddTransportSurface(surface);
                }
            }
        }

        // Simulation Scripts - Start, Update ....
        private void Start()
        {
            // Use Articulated bodies
            articulatedbody = GetComponent<ArticulationBody>();
            if (articulatedbody != null)
                articulatedbodynotnull = true;
            
            if (EditorMoveMode)
            {
                EndEditorMoveMode();
            }

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            
            if (UseInteract)
            {
#if GAME4AUTOMATION_INTERACT
                jointmonitor = GetComponent<XdeUnitJointMonitor>();
                jointcontroller = GetComponent<XdeUnitJointPDController>();
#endif
#if !GAME4AUTOMATION_INTERACT
                Error("INTERACT is not installed or not enabled - please check Game4Automation main menu and enable INTERACT");
#endif
            }

            CalculateVectors();
            _transportsurfaces = new TransportSurface[TransportSurfaces.Count];
            // Init Transportscripts
            // At TransportScript to Transport Surfaces
            for (int i = 0; i < TransportSurfaces.Count; i++)
            {
                if (TransportSurfaces[i] == null)
                {
                    ErrorMessage(
                        "Transportsurface Script needs to be assigned to Drive in Array [Transport Surfaces] at Position " +
                        i.ToString());
                }
                else
                {
                    _transportsurfaces[i] = TransportSurfaces[i];
                    _transportsurfaces[i].TransportDirection = _globaldirection;
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);
                }
            }

            if (_transportsurfaces.Length > 0)
            {
                _istransportsurface = true;
            }

            CurrentPosition = StartPosition;
        }

        public void DriveReset()
        {
            CurrentPosition = Offset;
            CurrentSpeed = 0;
            IsRunning = false;
            BlockDestination = true;
        }

        private void FixedUpdate()
        {
            if (ResetDrive)
            {
                DriveReset();
            }

            if (_StopDrive)
            {
                Stop();
            }

            // Jog stopped
            if (_lastjog && !JogBackward && !JogForward && !UseAcceleration)
            {
                Stop();
            }

            if (_lastjog && !JogBackward && !JogForward && UseAcceleration)
            {
                _stopjogging = true;
                if (CurrentSpeed > 0)
                    _currentacceleration = -Acceleration;
                else
                    _currentacceleration = Acceleration;
            }

            // Drive Decellerated totally - stop drive
            if (_decelerationstarted && CurrentSpeed < 0)
            {
                Stop();
            }


            var newtarget = false;

            // New Target Position
            if (_laststartdrivetotarget != TargetStartMove && TargetStartMove)
            {
                IsStopped = false;
                _stopjogging = false;
                BlockDestination = false;
                _currentdestination = TargetPosition;
                _currentacceleration = Acceleration;
                _isdrivingtotarget = true;
                _timestartacceleration = Time.time;
                IsAtTarget = false;
                //CurrentSpeed = 0;
                _StopDrive = false;
                if (_drivetostarted)
                {
                    TargetStartMove = false;
                    _drivetostarted = false;
                }

                newtarget = true;
            }

            // Calculate Position if Speed > 0 
            if (!IsStopped)
                if (!ResetDrive && (CurrentSpeed != 0) && !_StopDrive)
                {
                    CurrentPosition = CurrentPosition +
                                      CurrentSpeed * Game4AutomationController.SpeedOverride * Time.deltaTime;
                }


            // Need to slow down - negative acceleration
            if (_isdrivingtotarget && !_StopDrive && !ResetDrive && !JogBackward && !JogForward)
            {
                if (UseAcceleration)
                {
                    if (CurrentSpeed > 0)
                    {
                        _currentstoppos = CurrentPosition + (CurrentSpeed * CurrentSpeed) / (2 * Acceleration);
                    }
                    else
                    {
                        _currentstoppos = CurrentPosition - (CurrentSpeed * CurrentSpeed) / (2 * Acceleration);
                    }
                }
                else
                {
                    _currentstoppos = CurrentPosition;
                }
            }

            if (JogBackward || JogForward)
                IsStopped = false;

            // Calculate Acceleration
            if (!IsStopped)
                if ((_accelerationstarted || _decelerationstarted) ||
                    ((!IsAtTarget && _isdrivingtotarget) && !_StopDrive && !ResetDrive &&
                     UseAcceleration && !JogBackward && !JogForward &&
                     !_stopjogging))
                {
                    if (SmoothAcceleration == false)
                    {
                        if (!_accelerationstarted && !_decelerationstarted)
                        {
                            if (_currentdestination > _currentstoppos)
                            {
                                _currentacceleration = Acceleration;
                            }
                            else
                            {
                                _currentacceleration = -Acceleration;
                            }
                        }
                        else
                        {
                            if (_accelerationstarted)
                                _currentacceleration = Acceleration;
                            if (_decelerationstarted)
                                _currentacceleration = -Acceleration;
                        }
                    }
                    else
                    {
                        // Sinoide Calculation
                        float timespeedup = TargetSpeed / Acceleration;
                        float timedelta = Time.time - _timestartacceleration;
                        float dir = 1;

                        if (_currentdestination < _currentstoppos)
                        {
                            dir = -1;
                        }

                        if (_decelerationstarted)
                            dir = -1;


                        double f = (-Math.Abs(TargetSpeed) * 4 * Math.PI) /
                                   (Math.Sin(2 * Math.PI) * timespeedup - 2 * Math.PI * timespeedup);
                        _currentacceleration = dir * f * (Math.Sin(Math.PI * timedelta / timespeedup)) *
                                               (Math.Sin(Math.PI * timedelta / timespeedup));
                    }
                }

            // Calculate Acceleration if Jogging
            if (!IsStopped)
                if ((JogBackward || JogForward) && UseAcceleration)
                {
                    _stopjogging = false;
                    if (JogForward)
                    {
                        if (CurrentSpeed < TargetSpeed)
                            _currentacceleration = Acceleration;
                        if (CurrentSpeed > TargetSpeed)
                            _currentacceleration = -Acceleration;
                    }
                    else
                    {
                        if (CurrentSpeed < TargetSpeed)
                            _currentacceleration = -Acceleration;
                        if (CurrentSpeed > TargetSpeed)
                            _currentacceleration = Acceleration;
                    }
                }

            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && !newtarget)
                {
                    if ((_isdrivingtotarget && CurrentSpeed > 0 && CurrentPosition >= _currentdestination &&
                         _lastcurrentposition < _currentdestination) ||
                        (_isdrivingtotarget && CurrentSpeed < 0 && CurrentPosition <= _currentdestination &&
                         _lastcurrentposition > _currentdestination))
                    {
                        Stop();
                        BlockDestination = true;
                        CurrentPosition = _currentdestination;
                        IsAtTarget = true;
                        _isdrivingtotarget = false;
                        _stopjogging = false;
                    }
                }

            // Calculate Speed
            if (!IsStopped)
                if (!ResetDrive && !_StopDrive && (!IsAtTarget || JogBackward || JogForward))
                {
                    if (!UseAcceleration)
                    {
                        if (!JogForward && !JogBackward && !BlockDestination)
                        {
                            if (CurrentPosition < _currentdestination)
                                CurrentSpeed = TargetSpeed;
                            if (CurrentPosition > _currentdestination)
                                CurrentSpeed = -TargetSpeed;
                        }
                        else
                        {
                            if (JogForward)
                                CurrentSpeed = TargetSpeed;
                            if (JogBackward)
                                CurrentSpeed = -TargetSpeed;
                        }
                    }
                    else
                    {
                        CurrentSpeed = CurrentSpeed + (float) _currentacceleration * Time.fixedDeltaTime;
                        // Limit Speed to maximum
                        if (CurrentSpeed > 0 && CurrentSpeed > TargetSpeed && _currentacceleration > 0)
                        {
                            _accelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = TargetSpeed;
                        }

                        if (CurrentSpeed < 0 && CurrentSpeed < -TargetSpeed && _currentacceleration < 0)
                        {
                            _decelerationstarted = false;
                            _currentacceleration = 0;
                            CurrentSpeed = -TargetSpeed;
                        }
                    }
                }


            // Drive at Target Position
            if (!IsStopped)
                if (!JogForward && !JogBackward && _stopjogging)
                {
                    if ((CurrentSpeed > 0 && _lastspeed < 0) || (CurrentSpeed < 0 && _lastspeed > 0))
                    {
                        Stop();
                        _currentacceleration = 0;
                        _stopjogging = false;
                        IsAtTarget = false;
                        _currentdestination = CurrentPosition;
                    }
                }

            // Set new Position
            SetPosition();


            if (UseLimits)
            {
                IsAtLowerLimit = false;
                IsAtUpperLimit = false;
                var currpos = CurrentPosition;
                if (_limitraycastnotnull)
                    currpos = LimitRayCast.RayCastDistance;
                if (JogForward && currpos >= UpperLimit)
                {
                    if (!JumpToLowerLimitOnUpperLimit)
                    {
                        CurrentSpeed = 0;
                        CurrentPosition = UpperLimit;
                        IsAtLowerLimit = true;
                    }
                    else
                    {
                        CurrentPosition = currpos - UpperLimit;
                        if (OnJumpToLowerLimit != null)
                            OnJumpToLowerLimit.Invoke(this);
                    }
                }

                if (JogBackward && currpos <= LowerLimit)
                {
                    CurrentSpeed = 0;
                    CurrentPosition = LowerLimit;
                    IsAtUpperLimit = true;
                }

                if (!JogForward && !JogBackward)
                {
                    if (!_limitraycastnotnull)
                    {
                        // Normal Limits
                        if (currpos > UpperLimit)
                            CurrentPosition = UpperLimit;
                        if (currpos < LowerLimit)
                            CurrentPosition = LowerLimit;
                    }
                    else
                    {
                        //  With Raycast
                        var diff = 0.0f;
                        if (currpos > UpperLimit)
                            diff = UpperLimit - currpos;
                        if (currpos < LowerLimit)
                            diff = LowerLimit - currpos;
                        CurrentPosition = CurrentPosition - diff;
                    }
                }
            }


            //  Current Values / Status
            if (CurrentSpeed == 0)
            {
                IsRunning = false;
            }
            else
            {
                IsRunning = true;
            }

            if (CurrentSpeed == TargetSpeed && TargetSpeed != 0)
                IsAtTargetSpeed = true;
            else
                IsAtTargetSpeed = false;


            bool isonposition = false;
            if (CurrentPosition == _currentdestination)
            {
                IsAtTarget = true;
                if (_lastisattarget != IsAtTarget && OnAtPosition != null)
                {
                    isonposition = true;
                }
            }
            else
            {
                IsAtTarget = false;
            }

            if (_transportsurfaces != null && (_lastspeed != CurrentSpeed))
            {
                for (int i = 0; i < _transportsurfaces.Length; i++)
                {
                    _transportsurfaces[i].SetSpeed(CurrentSpeed * SpeedScaleTransportSurface);
                }
            }


            _laststartdrivetotarget = TargetStartMove;
            _lastspeed = CurrentSpeed;
            _lastisattarget = IsAtTarget;
            _lastcurrentposition = CurrentPosition;
            _lastjog = JogBackward || JogForward;
            
            if (isonposition)
                OnAtPosition(this);
        }

        #endregion
    }
}