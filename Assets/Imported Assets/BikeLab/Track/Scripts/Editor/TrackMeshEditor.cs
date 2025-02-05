using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace VK.BikeLab
{

    [CustomEditor(typeof(TrackMesh))]
    public class TrackMashEditor : Editor
    {
        private TrackMesh track;
        private void Awake()
        {
            track = (TrackMesh)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Build Mesh"))
                track.buildMesh();
        }
        private void OnSceneGUI()
        {
            //track.drawLabels();
            //track.drawTri(track.triangle);
            //track.drawSides();
        }
    }
}