using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace VK.BikeLab
{

    public class MotoballDispatcher : MonoBehaviour
    {
        [Range(0, 1)]
        public float avoidCollisions = 0;
        public bool holdActive;

        private List<MotoballController> controllers;
        private MotoballController user;
        private int userIndex;
        private bool slow = false;
        private bool soundOn = true;

        void Start()
        {
            Screen.SetResolution(1280, 720, false, 60);

            controllers = new List<MotoballController>();
            MotoballController[] arr = FindObjectsOfType<MotoballController>();
            foreach (MotoballController c in arr)
                if (c.isActiveAndEnabled)
                    controllers.Add(c);

            for (int i = 0; i < controllers.Count; i++)
            {
                MotoballController c = controllers[i];
                Camera123 camera = c.GetComponentInChildren<Camera123>();
                if (camera != null && camera.isActiveAndEnabled)
                {
                    user = c;
                    userIndex = i;
                }
            }
        }

        void Update()
        {
            bool sKey, rKey, pKey, oKey, tKey;
#if ENABLE_INPUT_SYSTEM
            sKey = Keyboard.current.sKey.wasPressedThisFrame;
            rKey = Keyboard.current.rKey.wasPressedThisFrame;
            pKey = Keyboard.current.pKey.wasPressedThisFrame;
            oKey = Keyboard.current.oKey.wasPressedThisFrame;
            tKey = Keyboard.current.tKey.wasPressedThisFrame;
#else
        sKey = Input.GetKey(KeyCode.S);
        rKey = Input.GetKey(KeyCode.R);
        pKey = Input.GetKey(KeyCode.P);
        oKey = Input.GetKey(KeyCode.O);
        tKey = Input.GetKey(KeyCode.T);
#endif
            if (sKey)
            {
                start();
            }
            if (rKey)
            {
                reset();
            }
            if (pKey)
            {
                swichBike();
            }

            if (holdActive)
                setHold();
        }
        public void start()
        {
            foreach (MotoballController c in controllers)
            {
                c.start();
            }
        }
        public void reset()
        {
            foreach (MotoballController c in controllers)
                c.reset();
        }
        public void swichBike()
        {
            int newIndex = userIndex + 1;
            if (newIndex >= controllers.Count)
                newIndex = 0;
            setBike(newIndex);
        }
        public void setCamera()
        {
            user.camera123.switchCamera();
        }
        public void zoomPlus()
        {
            user.camera123.zoomPlus();
        }
        public void zoomMinus()
        {
            user.camera123.zoomMinus();
        }
        public void sound()
        {
            soundOn = !soundOn;
            foreach (Sound s in FindObjectsOfType<Sound>())
                s.sound = soundOn;
        }
        private void setHold()
        {
            for (int i = 0; i < controllers.Count; i++)
                if (controllers[i].camRequest)
                {
                    setBike(i);
                    controllers[i].camRequest = false;
                }
        }
        private void setBike(int index)
        {
            if (index == userIndex)
                return;

            foreach (MotoballController mc in controllers)
                mc.camera123.gameObject.SetActive(false);

            userIndex = index;
            user = controllers[userIndex];
            user.camera123.gameObject.SetActive(true);
        }
        private void setInterpolation()
        {
            foreach (MotoballController c in controllers)
            {
                Bike b = c.gameObject.GetComponent<Bike>();
                WheelColliderInterpolator fI = b.frontCollider.GetComponent<WheelColliderInterpolator>();
                WheelColliderInterpolator rI = b.rearCollider.GetComponent<WheelColliderInterpolator>();
                if (fI != null)
                {
                    fI.enabled = slow;
                    if (slow)
                        fI.reset();
                }
                if (rI != null)
                {
                    rI.enabled = slow;
                    if (slow)
                        rI.reset();
                }

                RigidbodyInterpolation interpolation = RigidbodyInterpolation.None;
                if (slow)
                    interpolation = RigidbodyInterpolation.Interpolate;
                
                Ball ball = FindObjectOfType<Ball>();
                ball.GetComponent<Rigidbody>().interpolation = interpolation;

                foreach (Rigidbody rb in b.GetComponentsInChildren<Rigidbody>())
                    rb.interpolation = interpolation;
            }
        }
    }
}