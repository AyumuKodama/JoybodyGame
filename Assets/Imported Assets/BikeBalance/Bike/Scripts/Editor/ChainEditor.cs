using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [CustomEditor(typeof(Chain))]
    public class ChainEditor : Editor
    {
        private void Awake()
        {
        }
        public override void OnInspectorGUI()
        {
            Chain chain = (Chain)target;

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Look at wheel"))
            {
                Undo.RecordObject(target, "Look at wheel");
                chain.lookAtWheel();
            }

            GUILayout.Space(20);

            bool isReadable = false;
            chain.chain = (Transform)EditorGUILayout.ObjectField("Chain", chain.chain, typeof(Transform), true);
            if (chain.chain != null)
            {
                isReadable = chain.chain.GetComponent<MeshFilter>().sharedMesh.isReadable;
                if (!isReadable)
                {
                    string text = "Chain mesh is not readable. Please set checkbox Read/Write in prefab";
                    EditorGUILayout.HelpBox(text, MessageType.Warning);
                }
            }
            if (chain.chain != null
                && chain.rearRadius == 0 && chain.frontRadius == 0 && isReadable)
                chain.detectRadius();
            if (chain.chainPoints == null || chain.chainPoints[0] == Vector3.zero)
                chain.updateChainPoints();
            //if (chain.chainParts != null && chain.chainParts.Length == 4 && chain.chainParts[0].GetComponent<MeshFilter>().mesh == null)
            //    chain.restoreMesh();
            GUILayout.Space(20);
            //GUILayout.Label("Sprockets");
            EditorGUI.BeginChangeCheck();
            GUIContent content = new GUIContent("R1", "Front Sprocket Radius");
            float localFR = EditorGUILayout.FloatField(content, chain.localFR);
            content = new GUIContent("R2", "Rear Sprocket Radius");
            float localRR = EditorGUILayout.FloatField(content, chain.localRR);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Edit Radius");
                chain.localFR = localFR;
                chain.localRR = localRR;
                chain.updateChainPoints();
                SceneView.RepaintAll();
            }
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Detect Radius"))
            {
                Undo.RecordObject(target, "Detect Radius");
                chain.detectRadius();
                SceneView.RepaintAll();
            }

            GUILayout.Space(20);
            EditorGUI.BeginChangeCheck();
            float chainPitch = EditorGUILayout.FloatField("Chain pitch", chain.chainPitch);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "chainPitch");
                chain.chainPitch = chainPitch;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Chain.ChainPitch pitch = (Chain.ChainPitch)EditorGUILayout.EnumPopup("", chain.pitch);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "pitch");
                chain.pitch = pitch;
                float inch = 0.0254f;
                switch (pitch)
                {
                    case Chain.ChainPitch.Pitch4: { chain.chainPitch = inch * 4 / 8; break; }
                    case Chain.ChainPitch.Pitch5: { chain.chainPitch = inch * 5 / 8; break; }
                    case Chain.ChainPitch.Pitch6: { chain.chainPitch = inch * 6 / 8; break; }
                }
                SceneView.RepaintAll();
            }

            GUILayout.Space(20);
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Subdivide Mesh"))
            {
                Undo.RecordObject(target, "Split Chain");
                chain.subdivideMesh();
                SceneView.RepaintAll();
            }

            GUILayout.Label("Adjust offset");
            EditorGUI.BeginChangeCheck();
            bool doublePitch = GUILayout.Toggle(chain.doublePitch, "Double Pitch");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Double Pitch");
                chain.doublePitch = doublePitch;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            float angle = GUILayout.HorizontalSlider(chain.chainAngleOffset, -30, 30);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Adjust offset");
                chain.chainAngleOffset = angle;
                chain.rotateChain(0);
                SceneView.RepaintAll();
            }
            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            angle = EditorGUILayout.FloatField("Offset", chain.chainAngleOffset);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Adjust offset");
                chain.chainAngleOffset = angle;
                chain.rotateChain(0);
                SceneView.RepaintAll();
            }
            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            string path = EditorGUILayout.TextField("Mesh path", chain.meshPath);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Mesh path");
                chain.meshPath = path;
            }
            if (GUILayout.Button("Save Mesh"))
            {
                Undo.RecordObject(target, "Save Mesh");
                chain.saveMesh();
            }
        }
        private void OnSceneGUI()
        {
            Chain rearFork = (Chain)target;
            Vector3[] points = rearFork.chainPoints;
            if (points != null && points.Length == 4)
            {
                Handles.Label(rearFork.chain.TransformPoint(rearFork.chainPoints[0]), "p0");
                Handles.Label(rearFork.chain.TransformPoint(rearFork.chainPoints[1]), "p1");
                Handles.Label(rearFork.chain.TransformPoint(rearFork.chainPoints[2]), "p2");
                Handles.Label(rearFork.chain.TransformPoint(rearFork.chainPoints[3]), "p3");
            }
        }
    }
}