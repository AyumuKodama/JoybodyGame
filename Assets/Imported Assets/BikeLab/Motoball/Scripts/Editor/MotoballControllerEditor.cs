using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VK.BikeLab
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MotoballController))]
    public class MotoballControllerEditor : Editor
    {
        private MotoballController motoball;
        private void Awake()
        {
            motoball = (MotoballController)target;
        }
        private void OnSceneGUI()
        {
            if (Application.isPlaying)
            {
                motoball.draw();
            }
        }
    }
}