using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace VK.BikeLab
{

    [CustomEditor(typeof(SpeedLimits))]
    public class SpeedLimitsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpeedLimits speedLimits = (SpeedLimits)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Add Road Signs"))
                speedLimits.addRoadSigns();
            if (GUILayout.Button("Remove Road Signs"))
                speedLimits.removeRoadSigns();
        }
    }
}