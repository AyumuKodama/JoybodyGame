using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace VK.BikeLab
{

    [CustomEditor(typeof(TrackTerrain))]
    public class TrackTarrainEditor : Editor
    {
        private TrackTerrain track;
        private void Awake()
        {
            track = (TrackTerrain)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            string[] options = track.getLayers();
            track.groundLayer = EditorGUILayout.Popup("Ground layer", track.groundLayer, options);
            track.trackLayer = EditorGUILayout.Popup("Track layer", track.trackLayer, options);
            track.borderLayer = EditorGUILayout.Popup("Border layer", track.borderLayer, options);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build terrain"))
            {
                Undo.RecordObject(track, "Build terrain");
                track.buildTerrain();
            }
            if (GUILayout.Button("Draw Track"))
            {
                Undo.RecordObject(track, "Draw Track");
                track.drawTrack();
            }
            GUILayout.EndHorizontal();
        }
    }
}