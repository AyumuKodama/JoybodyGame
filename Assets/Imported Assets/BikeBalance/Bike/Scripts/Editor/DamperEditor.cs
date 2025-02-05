using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [CustomEditor(typeof(Damper))]
    public class DamperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Damper damper = (Damper)target;

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Look at each other"))
            {
                Undo.RecordObject(target, "Look at each other");
                damper.lookAt();
            }
        }
    }
}