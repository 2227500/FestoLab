// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  

using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace game4automation
{
    [SelectionBase]
    [RequireComponent(typeof(BoxCollider))]
    
    //! Sink to destroy objects in the scene
    public class Sink : Game4AutomationBehavior
    {
        // Public - UI Variables 
        [Header("Settings")] public bool DeleteMus = true; //!< Delete MUs
        [ShowIf("DeleteMus")] public bool Dissolve = true; //!< Dissolve MUs
        [ShowIf("DeleteMus")] public string DeleteOnlyTag; //!< Delete only MUs with defined Tag
        [ShowIf("DeleteMus")] public float DestroyFadeTime=0.5f; //!< Time to fade out MU
        [Header("Sink IO's")] public PLCOutputBool Delete; //!< PLC output for deleting MUs
        private bool _lastdeletemus = false;
    
        [Header("Status")] 
        [ReadOnly] public float SumDestroyed; //!< Sum of destroyed objects
        [ReadOnly] public float DestroyedPerHour; //!< Sum of destroyed objects per Hour
        [ReadOnly] public List<GameObject> CollidingObjects; //!< Currently colliding objects

        public SinkEventOnDestroy OnMUDelete;
        
        private bool _isDeleteNotNull;

        // Use this when Script is inserted or Reset is pressed
        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }    
    
        // Use this for initialization
        private void Start()
        {
            _isDeleteNotNull = Delete != null;
        }

        public void DeleteMUs()
        {
            
            var tmpcolliding = CollidingObjects;
            foreach (var obj in tmpcolliding.ToArray())
            {
                var mu = GetTopOfMu(obj);
                if (mu != null)
                {
                    if (DeleteOnlyTag == "" || (mu.gameObject.tag == DeleteOnlyTag))
                    {

                        OnMUDelete.Invoke(mu);
                        if (!Dissolve)
                             Destroy(mu.gameObject);
                        else
                            mu.Dissolve(DestroyFadeTime);
                        SumDestroyed++;
                    }
                }

                CollidingObjects.Remove(obj);
            }
        }
    
        // ON Collission Enter
        private void OnTriggerEnter(Collider other)
        {
            GameObject obj = other.gameObject;
            CollidingObjects.Add(obj);
            if (DeleteMus==true)
            {
                // Act as Sink
                DeleteMUs();
            }
        }
    
        // ON Collission Exit
        private void OnTriggerExit(Collider other)
        {
            GameObject obj = other.gameObject;
            CollidingObjects.Remove(obj);
        }

        private void Update()
        {
            DestroyedPerHour = SumDestroyed / (Time.time / 3600);
            if (_isDeleteNotNull)
            {
                DeleteMus = Delete.Value;
            }
        
            if (DeleteMus && !_lastdeletemus)
            {
                DeleteMUs();
            }
            _lastdeletemus = DeleteMus;

        }
    }
}