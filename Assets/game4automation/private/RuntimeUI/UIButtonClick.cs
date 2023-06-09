﻿// Game4Automation (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// (c) 2019 in2Sight GmbH - Usage of this source code only allowed based on License conditions see https://game4automation.com/lizenz  
using UnityEngine;
using UnityEngine.EventSystems;

namespace game4automation
{
    public class UIButtonClick : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler
    {
        public bool pressed = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
        }

    }
}