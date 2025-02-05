using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VK.BikeLab
{
    public class Camera123 : MonoBehaviour
    {
        [System.Serializable]
        public enum CameraView { FirstPerson, SecondPerson, ThirdPerson }
        public Transform observationObject;
        public Transform camera1;
        public Transform camera2;
        public Transform camera3;
        public CameraView cameraView;

        private bool initialized = false;
        private Vector3 localPos2 = new Vector3(0, 1.5f, -4);
        private float eulerX2 = 6;
        private float offsetY2 = 1.5f;
        private Vector3 wordOffset3 = new Vector3(10, 1, 0);
        private float zoom = 1;
        private float zoom1 = 1;
        private float zoom2 = 1;
        private float zoom3 = 1;
        private float zoomStep = 1.02f;

        private void OnApplicationFocus(bool focus)
        {
            init();
        }
        
        public void init()
        {
            if (initialized)
                return;
            localPos2 = camera2.localPosition;
            eulerX2 = camera2.localEulerAngles.x;
            offsetY2 = (camera2.position - camera2.parent.position).y;

            wordOffset3 = camera3.position - camera3.parent.position;
            zoom1 = 1;
            zoom2 = 1;
            zoom3 = 1;

            initialized = true;
        }
        void Update()
        {
            bool cKey, plusKey, minusKey;
#if ENABLE_INPUT_SYSTEM
            cKey = Keyboard.current.cKey.wasPressedThisFrame;
            plusKey = Keyboard.current.numpadPlusKey.isPressed;
            minusKey = Keyboard.current.numpadMinusKey.isPressed;
            zoomStep = 1.02f;
#else
            cKey = Input.GetKeyDown(KeyCode.C);
            plusKey = Input.GetKeyDown(KeyCode.KeypadPlus);
            minusKey = Input.GetKeyDown(KeyCode.KeypadMinus);
            zoomStep = 1.5f;
#endif
            if (cKey)
                switchCamera();
            
            zoom = 1;
            if (plusKey)
                zoom = 1f / zoomStep;
            if (minusKey)
                zoom = zoomStep;
            setView();
        }
        public void setView()
        {

            camera1.gameObject.SetActive(cameraView == CameraView.FirstPerson);
            camera2.gameObject.SetActive(cameraView == CameraView.SecondPerson);
            camera3.gameObject.SetActive(cameraView == CameraView.ThirdPerson);

            switch (cameraView)
            {
                case CameraView.FirstPerson:
                    {
                        zoom1 *= zoom;
                        //camera1.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
                        Vector3 e = camera1.eulerAngles;
                        e.z = 0;
                        camera1.eulerAngles = e;
                        break;
                    }
                case CameraView.SecondPerson:
                    {
                        zoom2 *= zoom;
                        camera2.localPosition = localPos2 * zoom2;

                        Vector3 pos = camera2.position;
                        pos.y = camera2.parent.position.y + offsetY2 * zoom2;
                        camera2.position = pos;

                        camera2.LookAt(camera2.parent.position + Vector3.up, Vector3.up);
                        camera2.eulerAngles = new Vector3(eulerX2, camera2.eulerAngles.y, 0);
                        break;
                    }
                case CameraView.ThirdPerson:
                    {
                        zoom3 *= zoom;
                        camera3.position = camera3.parent.position + wordOffset3 * zoom3;
                        camera3.LookAt(camera3.parent.position + Vector3.up * zoom3, Vector3.up);
                        //Vector3 e = camera3.eulerAngles;
                        //e.x = 0;
                        //camera3.eulerAngles = e;
                        break;
                    }
            }
        }
        public void switchCamera()
        {
            switch (cameraView)
            {
                case CameraView.FirstPerson: cameraView = CameraView.SecondPerson; break;
                case CameraView.SecondPerson: cameraView = CameraView.ThirdPerson; break;
                case CameraView.ThirdPerson: cameraView = CameraView.FirstPerson; break;
            }

        }
        public void zoomPlus()
        {
            zoom = 0.5f;
            setView();
        }
        public void zoomMinus()
        {
            zoom = 2;
            setView();
        }
    }
}