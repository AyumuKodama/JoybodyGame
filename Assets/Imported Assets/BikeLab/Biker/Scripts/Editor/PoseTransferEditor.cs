using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VK.BikeLab
{
    [CustomEditor(typeof(PoseTransfer))]
    [CanEditMultipleObjects]
    public class PoseTransferEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PoseTransfer poseTransfer = (PoseTransfer)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Trasfer"))
                poseTransfer.transferPose();
        }
    }
}