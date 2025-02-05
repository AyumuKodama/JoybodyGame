using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [CustomEditor(typeof(Swingarm))]
    public class RearForkEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Swingarm rearFork = (Swingarm)target;

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Look at wheel"))
            {
                Undo.RecordObject(target, "Look at wheel");
                rearFork.lookAtWheel();
            }
        }
    }
}