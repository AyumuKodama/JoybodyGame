using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [CustomEditor(typeof(Camera123))]
    public class Camera123Editor : Editor
    {
        private Camera123.CameraView view;
        private void Awake()
        {
            Camera123 camera123 = (Camera123)target;
            view = camera123.cameraView;
        }
        public override void OnInspectorGUI()
        {
            Camera123 camera123 = (Camera123)target;
            DrawDefaultInspector();

            if (view != camera123.cameraView)
            {
                view = camera123.cameraView;
                camera123.init();
                camera123.setView();
            }

        }
    }
}