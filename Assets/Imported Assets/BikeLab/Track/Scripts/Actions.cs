using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#38-actions")]
    public class Actions : MonoBehaviour
    {
        public GameObject panelMenu;
        
        private List<Camera123> cameras;
        private Rigidbody[] rigidbodies;
        private WheelColliderInterpolator[] interpolators;
        private TrackController[] trackControllers;
        private MotoballController[] motoballControllers;
        private int selected;
        private bool soundOn = true;
        private bool slow = false;

        private bool menuDisabled;
        private float startTime;
        private void Awake()
        {
            Camera123[] cams = FindObjectsOfType<Camera123>(true);
            cameras = new List<Camera123>();
            for (int i = 0; i < cams.Length; i++)
            {
                if (cams[i].transform.parent.gameObject.activeInHierarchy)
                    cameras.Add(cams[i]);
                cams[i].init();
            }

            rigidbodies = FindObjectsOfType<Rigidbody>();
            interpolators = FindObjectsOfType<WheelColliderInterpolator>();
            trackControllers = FindObjectsOfType<TrackController>();
            motoballControllers = FindObjectsOfType<MotoballController>();
        }
        void Start()
        {
            startTime = Time.realtimeSinceStartup;
            menuDisabled = false;

            selected = -1;
            for (int i = 0; i < cameras.Count; i ++)
            {
                if (cameras[i].gameObject.activeSelf)
                    selected = i;
                if (selected >= 0 && i > selected)
                    cameras[i].gameObject.SetActive(false);
            }
        }
        void Update()
        {
            if (!menuDisabled && Time.realtimeSinceStartup > startTime + 1)
            {
                panelMenu.SetActive(false);
                menuDisabled = true;
            }

            bool pKey, tKey;
            bool alpha0, alpha1, alpha2, alpha3, alpha4, alpha5, alpha6, alpha7, alpha8, alpha9;
#if ENABLE_INPUT_SYSTEM
            pKey = Keyboard.current.pKey.wasPressedThisFrame;
            tKey = Keyboard.current.tKey.wasPressedThisFrame;
            alpha0 = Keyboard.current.digit0Key.wasPressedThisFrame;
            alpha1 = Keyboard.current.digit1Key.wasPressedThisFrame;
            alpha2 = Keyboard.current.digit2Key.wasPressedThisFrame;
            alpha3 = Keyboard.current.digit3Key.wasPressedThisFrame;
            alpha4 = Keyboard.current.digit4Key.wasPressedThisFrame;
            alpha5 = Keyboard.current.digit5Key.wasPressedThisFrame;
            alpha6 = Keyboard.current.digit6Key.wasPressedThisFrame;
            alpha7 = Keyboard.current.digit7Key.wasPressedThisFrame;
            alpha8 = Keyboard.current.digit8Key.wasPressedThisFrame;
            alpha9 = Keyboard.current.digit9Key.wasPressedThisFrame;
#else
            pKey = Input.GetKeyDown(KeyCode.P);
            tKey = Input.GetKeyDown(KeyCode.T);
            alpha0 = Input.GetKeyDown(KeyCode.Alpha0);
            alpha1 = Input.GetKeyDown(KeyCode.Alpha1);
            alpha2 = Input.GetKeyDown(KeyCode.Alpha2);
            alpha3 = Input.GetKeyDown(KeyCode.Alpha3);
            alpha4 = Input.GetKeyDown(KeyCode.Alpha4);
            alpha5 = Input.GetKeyDown(KeyCode.Alpha5);
            alpha6 = Input.GetKeyDown(KeyCode.Alpha6);
            alpha7 = Input.GetKeyDown(KeyCode.Alpha7);
            alpha8 = Input.GetKeyDown(KeyCode.Alpha8);
            alpha9 = Input.GetKeyDown(KeyCode.Alpha9);
#endif
            if (pKey)
                selectNext();
            if (tKey)
                updateTimeScale();
            if (alpha0)
                select(0);
            if (alpha1)
                select(1);
            if (alpha2)
                select(2);
            if (alpha3)
                select(3);
            if (alpha4)
                select(4);
            if (alpha5)
                select(5);
            if (alpha6)
                select(6);
            if (alpha7)
                select(7);
            if (alpha8)
                select(8);
            if (alpha9)
                select(9);
        }
        public void showHidePanelMenu()
        {
            panelMenu.SetActive(!panelMenu.activeSelf);
        }
        public void selectNext()
        {
            if (cameras == null || cameras.Count == 0 || selected == -1)
                return;
            selected = 0;
            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i].gameObject.activeSelf)
                    selected = i;
                cameras[i].gameObject.SetActive(false);
            }

            selected++;
            if (selected > cameras.Count - 1)
                selected = 0;
            cameras[selected].gameObject.SetActive(true);
        }
        public void select(int index)
        {
            if (cameras == null || cameras.Count == 0 || selected == -1 || index >= cameras.Count)
                return;
            cameras[selected].gameObject.SetActive(false);
            selected = index;
            cameras[selected].gameObject.SetActive(true);
        }
        public void switchCamera()
        {
            if (cameras == null || cameras.Count == 0 || selected == -1)
                return;
            cameras[selected].switchCamera();
        }
        public void zoomPlus()
        {
            if (cameras == null || cameras.Count == 0 || selected == -1)
                return;
            cameras[selected].zoomPlus();
        }
        public void zoomMinus()
        {
            if (cameras == null || cameras.Count == 0 || selected == -1)
                return;
            cameras[selected].zoomMinus();
        }
        public void sound()
        {
            soundOn = !soundOn;
            foreach (Sound s in FindObjectsOfType<Sound>())
                s.sound = soundOn;
        }
        public void updateTimeScale()
        {
            slow = !slow;

            foreach (WheelColliderInterpolator wi in interpolators)
            {
                wi.enabled = slow;
                if (slow)
                    wi.reset();
            }
            
            foreach (Rigidbody rb in rigidbodies)
                if (slow)
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                else
                    rb.interpolation = RigidbodyInterpolation.None;

            if (slow)
                Time.timeScale = 0.1f;
            else
                Time.timeScale = 1;
        }

        public void start()
        {
            foreach (TrackController tc in trackControllers)
                if (tc.enabled)
                    tc.start();
            foreach (MotoballController mc in motoballControllers)
                if (mc.enabled)
                    mc.start();
        }
        public void reset()
        {
            foreach (TrackController tc in trackControllers)
                if (tc.enabled)
                    tc.reset();
            foreach (MotoballController mc in motoballControllers)
                if (mc.enabled)
                    mc.reset();
        }
    }
}