using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VK.BikeLab.Segway
{
    [RequireComponent(typeof(Segway))]
    [RequireComponent(typeof(BikeInput))]
    public class ManualControl : MonoBehaviour
    {
        private BikeInput bikeInput;
        private Segway segway;
        void Start()
        {
            bikeInput = GetComponent<BikeInput>();
            segway = GetComponent<Segway>();
        }
        private void Update()
        {
            bool spaceKey = false, sKey = false;
#if ENABLE_INPUT_SYSTEM
            spaceKey = Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        sKey = Input.GetKey(KeyCode.S);
#endif
            if (spaceKey)
                bikeInput.yAxis = 0;
        }
        private void FixedUpdate()
        {
            float incline = bikeInput.xAxis * 30;
            segway.setSideIncline(incline);

            float v = bikeInput.yAxis * 8;
            segway.setVelocity(v);
        }
    }
}