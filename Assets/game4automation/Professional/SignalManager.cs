using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace game4automation
{
public class SignalManager : Game4AutomationBehavior
{
    [ReorderableList]public List<GameObject> AutoConnectLevels;
    public List<Signal> UnconnectedSignals;
    
    [Button("Check for unconnected Signals")]
    void CheckUnconnected()
    {
        Game4AutomationController = UnityEngine.Object.FindObjectOfType<Game4AutomationController>();
        if (Game4AutomationController!=null)
            Game4AutomationController.UpdateSignals();
        UnconnectedSignals = new List<Signal>();
        var signals = GetComponentsInChildren<Signal>();
        foreach (var signal in signals)
        {
            if (signal.IsConnectedToBehavior() == false)
                UnconnectedSignals.Add(signal);
        }
    }
    
    [Button("Delete unconnected Signals")]
    void DeleteUnconnected()
    {
        #if UNITY_EDITOR
        CheckUnconnected();
       foreach (var signal in UnconnectedSignals)
       {
           Undo.DestroyObjectImmediate(signal.gameObject);
       }
       #endif
        UnconnectedSignals.Clear();
    }

    [Button("Delete automatically created signals")]
    void DeleteAutoSignals()
    {
#if UNITY_EDITOR
        var signals = GetComponentsInChildren<Signal>();
        foreach (var signal in signals)
        {
            if (signal.Autoconnected== true)
                Undo.DestroyObjectImmediate(signal.gameObject);
        }
        #endif
    }
    
    [Button("Start Signal creation & connection")]
    void AutoConnect()
    {
        
        foreach (var go in AutoConnectLevels)
        {
            var behaviors = go.GetComponentsInChildren<BehaviorInterface>();
            foreach (var behavior in behaviors)
            {
                var connectlogics = GetComponents<AutoConnectBase>();
                foreach (var logic in connectlogics)
                {
                    logic.AutoConnect(behavior);
                }
            }
        }   
        CheckUnconnected();
    }
    
}
}
