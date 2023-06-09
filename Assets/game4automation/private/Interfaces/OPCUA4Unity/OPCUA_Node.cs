﻿
using System;
using System.Collections.Generic;
using UnityEngine;

namespace game4automation
{
    //! OPCUA Node Unity Script
    //! Represents an OPCUA node, holds the Nodes Attributes and Reads and Writes nodes values
    public class OPCUA_Node : MonoBehaviour
    {
        public OPCUA_Interface Interface; //!< The OPCUA Interface the node is part of / connected to
        public bool WriteValue; //!< True if the value should be written to the OPC server
        public bool ReadValue; //!< True if the value should be read from the OPC server
        public bool SubscribeValue; //!< True if value changes should be subscribed
        public bool PollInput; //!< Provides a cyclic poll if OPCUA server supports no subscription 
        public string NodeId; //!< Nodeid from OPC UA Node (e.g. "ns=4;s=PLC.PLCOut_PNEUMATICZ_Einfahren")

#if GAME4AUTOMATION
        [ReadOnly] public string Value; //!< Value as string of OPC Node
        [ReadOnly] public string Type; //!< Type as string of OPC Node
        [ReadOnly] public bool ValueIsArray; //!< true if Value is Array
        [ReadOnly] public string Status; //!< The status of the OPC Node
        [ReadOnly] public string UserAccessLevel; //!< User Access level 
        [ReadOnly] public string IdentifierType; //!< 
        [ReadOnly] public string Identifier; //!< 
#endif
        
#if !GAME4AUTOMATION
        public string Name; //!< Name of the node
        public bool Connected; //!< true if connected
        public string Value; //!< Value as string of OPC Node
        public string Type; //!< Type as string of OPC Node
        public bool ValueIsArray; //!< true if Value is Array
        public string ValueArrayElementType;
        public int ValueArraySize;
        public List<string> ValuesArray = new List<string>();
        public string Status;
        public bool StatusGood;
        public bool StatusBad;
        public string ServerTimestamp;
        public string SourceTimestamp;
        public string AccessLevel;
        public string UserAccessLevel;
        public string Description;
        public int NamespaceIndex;
        public string IdentifierType;
        public string Identifier;
#endif
        public object SignalValue;
        

        private string oldvalue;
        

#if GAME4AUTOMATION
        private Signal signal;
#endif

        private OPCUANodeSubscription subscription;

#if GAME4AUTOMATION
        private Signal CreateGame4AutomationSignal(System.Type type)
        {
            Signal comp = (Signal) GetComponent(type);
            if (comp == null)
            {
                Signal exist = GetComponent<Signal>();
                if (exist != null)
                    DestroyImmediate(exist);

                comp = (Signal) gameObject.AddComponent(type);
            }

            return comp;
        }
#endif

        public object ReadNodeValue()
        {
            return Interface.ReadNodeValue(this);
        }
        
        public bool WriteNodeValue(object value)
        {
            return Interface.WriteNodeValue(this, value);
        }
        
        
        
        
        //! Reads the node properties from OPC Server and Updates displayed porperties
        public bool ReadNode()
        {
            var value  = Interface.ReadNodeValue(this);
            if (value != null)
            {
                Type valueType = value.GetType();
                if (valueType.IsArray)
                {
                    ValueIsArray = true;
                }
                else
                {
                    ValueIsArray = false;
                }
#if GAME4AUTOMATION
                UpdatePLCSignal();
#endif
                return true;
            }

            return false;
        }
        
        //! Reads the node properties from OPC Server and Updates displayed porperties
        public void WriteNode()
        {
            var value  = Interface.ReadNodeValue(this);
            if (value != null)
            {
#if GAME4AUTOMATION
                UpdatePLCSignal();
#endif
            }
        }
        

#if GAME4AUTOMATION
        //! Only used for Game4Automation Framework, Updates the PLCInput or PLCOutput Signal scripts corresponding to the Node direction and Node datatype
        public Signal UpdatePLCSignal()
        {
            Signal sig = null;

            if ((ReadValue || WriteValue) && !ValueIsArray)
            {
                if (ReadValue && !WriteValue)
                {
                    switch (Type)
                    {
                        case "Boolean":
                            sig = CreateGame4AutomationSignal(typeof(PLCOutputBool));
                            break;
                        case "Double":
                        case "Float":
                            sig = CreateGame4AutomationSignal(typeof(PLCOutputFloat));
                            break;
                        case "UByte":
                        case "Byte":
                        case "SByte":
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "UInt16":
                        case "UInt32":
                        case "UInt64":
                            sig = CreateGame4AutomationSignal(typeof(PLCOutputInt));
                            break;
                    }
                }

                if (WriteValue && !ReadValue)
                {
                    switch (Type)
                    {
                        case "Boolean":
                            sig = CreateGame4AutomationSignal(typeof(PLCInputBool));
                            break;
                        case "Double":
                        case "Float":
                            sig = CreateGame4AutomationSignal(typeof(PLCInputFloat));
                            break;
                        case "UByte":
                        case "SByte":
                        case "Byte":
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "UInt16":
                        case "UInt32":
                        case "UInt64":
                            sig = CreateGame4AutomationSignal(typeof(PLCInputInt));
                            break;
                    }
                }
            }


            if (!System.Object.ReferenceEquals(sig, null) && !System.Object.ReferenceEquals(SignalValue, null))
            {
                if (ReadValue)
                    sig.SetValue(SignalValue);
            }

            if (!System.Object.ReferenceEquals(sig, null))
            {
                if (sig.IsInput() && (WriteValue != true || SubscribeValue == true))
                    Debug.LogWarning(
                        "Game4Automation PLCInputs should normally have WriteValue=true and SubsribeValue=false, please make sure, that  setting of " +
                        this.name + " is correct");
            }

            if (!System.Object.ReferenceEquals(sig, null))
            {
                if (!sig.IsInput() && (ReadValue != true || SubscribeValue == false))
                    Debug.LogWarning(
                        "Game4Automation PLCInputs should normally have ReadValue=true and SubsribeValue=true, please make sure, that setting of " +
                        this.name + " is correct");
            }

            if ((ReadValue == true && WriteValue == true))
                Debug.LogWarning(
                    "Game4Automation Signals should normally be only Read OR Write and not both, please make sure, that setting of " +
                    this.name + " is correct!");
            return sig;


        }
#endif

        //! Node Changed event for subscription
        private void NodeChanged(OPCUANodeSubscription sub, object obj)
        {
            SignalValue = obj;
            Value = obj.ToString();

#if GAME4AUTOMATION
            if (!ReferenceEquals(signal, null))
            {
                signal.SetValue(obj);
             
            }
#endif
        }

#if GAME4AUTOMATION
        //! Write signal changed event subscription
        private void WriteSignalChanged(Signal signal)
        {
           /* if (_opcNode != null)
            {
                _opcNode.Value = signal.GetValue();
                if (Interface.ConnectToServer)
                    signal.SetStatusConnected(true);
                else
                {
                    signal.SetStatusConnected(false);
                }
            }*/
        }

   
#endif

        //! Unity Awake Event - Subscibes to Game4Automation PLCInput Signal changes (only for Game4Automation Framework)
        public void Awake()
        {
#if GAME4AUTOMATION
            signal = GetComponent<Signal>();
            if (signal != null)
            {
                if (WriteValue)
                    signal.SignalChanged += WriteSignalChanged;
            }
#endif
            if (Interface!=null)
                Interface.EventOnConnected.AddListener(OnConnected);
        }

        //! Starts Subscription if SubscribeValue is set to true
        public void Subscribe()
        {
            if (SubscribeValue)
            {
                if (Interface != null)
                {
                    subscription = Interface.Subscribe(NodeId, NodeChanged);
                }

                ReadNode();
            }
        }

        public void OnConnected()
        {
            if (Interface != null)
            {
                if (Interface.IsConnected)
                {
#if GAME4AUTOMATION
                    UpdatePLCSignal();

                    if (signal != null)
                        signal.SetStatusConnected(true);
#endif
                    if (ReadValue && PollInput)
                    {
                  /*      _opcNode = Interface.AddWatchedNode(NodeId);
                        _opcNode.PollNode = true; */
                    }

                    if (WriteValue)
                    {
             /*           _opcNode = Interface.AddWatchedNode(NodeId); */
#if GAME4AUTOMATION
                        if (signal != null)
                        {
                            WriteSignalChanged(signal);
                            oldvalue = signal.GetValue().ToString();
                        }
#endif
                   
                    }
                    Subscribe();
                }
            }
        }
        
        public void OnDisconnected()
        {
            subscription = null;
#if GAME4AUTOMATION
            if (signal != null)
                signal.SetStatusConnected(false);
#endif
        }
        
        //! Unity Start Event - Subscibes to the Changes of Nodes on OPCUA Server, Node Value will always be updated if SubscribeValue=true
        public void Start()
        {
         
        }

        private void FixedUpdate()
        {
       
                if (PollInput)
                {
                    ReadNode();
#if GAME4AUTOMATION_PROFESSIONAL
                    signal.SetValue(SignalValue);
                    signal.SetStatusConnected(true);
#endif
                }


#if GAME4AUTOMATION_PROFESSIONAL
                if (WriteValue && (signal != null))
                {
                    var val = signal.GetValue();
                    var vals = val.ToString();
                    if (oldvalue != val.ToString())
                    {
                        WriteNodeValue(val);
                        oldvalue = vals;
                    }

                   /* if (StatusGood)
                        signal.SetStatusConnected(_opcNode.Connected);
                    else
                        signal.SetStatusConnected(false);*/
                }
#endif
            }
    }
}