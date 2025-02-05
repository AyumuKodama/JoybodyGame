using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#135-chain")]
    public class Chain : MonoBehaviour
    {
        public enum ChainPitch { Pitch4, Pitch5, Pitch6 }

        public WheelCollider rearCollider;
        public Transform rearWheel;
        public Transform frontSprocket;
        [Header("Visual model objects")]
        public Transform chain;
        public Transform frontSprocketModel;

        // in chain space
        [HideInInspector] public float frontRadius;
        [HideInInspector] public float rearRadius;
        [HideInInspector] public Vector3 rearSprocketPos;
        [HideInInspector] public Vector3[] chainPoints;

        // in this space
        [HideInInspector] public float localFR = 0.04f;
        [HideInInspector] public float localRR = 0.12f;
        [HideInInspector] public Vector3 localRSPos;
        [HideInInspector] public Vector3[] localCP;

        [HideInInspector] public float chainPitch = 0.0127f;
        [HideInInspector] public ChainPitch pitch;
        [HideInInspector] public bool doublePitch;
        [HideInInspector] public Transform[] chainParts;
        [HideInInspector] public Vector3 chainOriginalPos;
        [HideInInspector] public Quaternion chainOriginalRot;
        [HideInInspector] public float chainAngleOffset = 0;
        [HideInInspector] public string meshPath = "Assets/BikeLab/Meshes/Chain.asset";

        private void Start()
        {
            if (frontSprocket != null && frontSprocketModel != null)
                frontSprocketModel.parent = frontSprocket;
            setChainPoints();
        }
        void Update()
        {
        }
        private void FixedUpdate()
        {
        }
        public void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (chain == null)
                return;

            localRSPos = transform.InverseTransformPoint(rearWheel.position);
            localRSPos.x = 0;

            Vector3 rPos = transform.TransformPoint(localRSPos);
            Vector3 fPos = transform.position;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rPos, localRR);
            Gizmos.DrawWireSphere(fPos, localFR);

            if (localCP == null || localCP.Length != 4)
                setChainPoints();

            Vector3 p0 = transform.TransformPoint(localCP[0]);
            Vector3 p1 = transform.TransformPoint(localCP[1]);
            Vector3 p2 = transform.TransformPoint(localCP[2]);
            Vector3 p3 = transform.TransformPoint(localCP[3]);
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(rPos, p0);
            Gizmos.DrawLine(rPos, p3);
            Gizmos.DrawLine(fPos, p1);
            Gizmos.DrawLine(fPos, p2);

            Vector3 top = (p1 - p0);
            float m = top.magnitude;
            top.Normalize();
            float offset = 0;
            Handles.color = Color.red;
            while (offset < m && chainPitch > 0)
            {
                Vector3 point = p0 + top * offset;
                Camera camera = SceneView.currentDrawingSceneView.camera;
                //Vector3 up = (camera.transform.position - point).normalized;
                Vector3 up = camera.transform.forward;
                Handles.DrawSolidDisc(point, up, 0.0015f);
                offset += chainPitch;
            }
#endif
        }
        public void lookAtWheel()
        {
            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            Vector3 localPos = transform.parent.InverseTransformPoint(pos);
            localPos.x = transform.localPosition.x;
            pos = transform.parent.TransformPoint(localPos);
            transform.LookAt(pos);
        }
        private bool chainReady()
        {
            bool points = chainPoints != null && chainPoints.Length == 4;
            bool r = frontRadius > 0 && rearRadius > 0;
            return points && r;
        }
        public void detectRadius()
        {
            Mesh mesh = chain.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vert = mesh.vertices;
            rearRadius = getSprocketRadius(false, vert);
            frontRadius = getSprocketRadius(true, vert);
            localRR = rearRadius * chain.lossyScale.z;
            localFR = frontRadius * chain.lossyScale.z;
            setChainPoints();
        }
        public void updateChainPoints()
        {
            if (chain == null)
                return;
            rearRadius = localRR / chain.lossyScale.z;
            frontRadius = localFR / chain.lossyScale.z;
            setChainPoints();
        }
        public void detectPitch()
        {
            float t1 = Time.realtimeSinceStartup;
            Mesh mesh = chain.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vert = mesh.vertices;
            Vector3 p0 = chain.transform.InverseTransformPoint(chainPoints[0]);
            Vector3 p1 = chain.transform.InverseTransformPoint(chainPoints[1]);
            Vector3 s = (p1 - p0).normalized;
            for (int i = 0; i < vert.Length; i++)
            {
                float d = Vector3.Cross(vert[i] - p0, s).magnitude;
                if (d < 0.02f)
                {
                }
                else
                {
                }
            }
            float t2 = Time.realtimeSinceStartup;
            Debug.Log("detectPitch " + (t2 - t1));
        }
        public void rotateChain(float angle)
        {
            angle += chainAngleOffset;
            float angleR = angle;
            float angleF = angle * localRR / localFR;

            angleR %= 360;
            angleF %= 360;
            float rotPitchR = chainPitch / localRR * Mathf.Rad2Deg;
            float rotPitchF = chainPitch / localFR * Mathf.Rad2Deg;
            if (doublePitch)
            {
                rotPitchR *= 2;
                rotPitchF *= 2;
            }
            angleR %= rotPitchR;
            angleF %= rotPitchF;
            float offset = -angleR * Mathf.Deg2Rad * localRR;

            foreach (Transform t in chainParts)
                t.SetLocalPositionAndRotation(chainOriginalPos, chainOriginalRot);

            Vector3 point = transform.TransformPoint(localRSPos);
            chainParts[0].RotateAround(point, transform.right, angleR);

            point = transform.position;
            chainParts[2].RotateAround(point, transform.right, angleF);

            Vector3 translation = (localCP[1] - localCP[0]).normalized * offset;
            chainParts[1].Translate(translation, transform);

            translation = (localCP[3] - localCP[2]).normalized * offset;
            chainParts[3].Translate(translation, transform);

            if (frontSprocket != null)
            {
                frontSprocket.localRotation = Quaternion.identity;
                point = frontSprocket.position;
                frontSprocket.RotateAround(point, transform.right, angleF);
                frontSprocket.localPosition = Vector3.zero;
            }
        }
        public void subdivideMesh()
        {
            float t1 = Time.realtimeSinceStartup;
            setChainPoints();
            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            Vector3 back1 = (chainPoints[0] - chainPoints[1]).normalized;
            Vector3 back2 = (chainPoints[3] - chainPoints[2]).normalized;
            Vector3 fPos = chain.InverseTransformPoint(transform.position);
            Vector3 rPos = rearSprocketPos;
            Vector3 right = Vector3.Cross(rPos - fPos, chainPoints[0] - rPos);
            Vector3 up = Vector3.Cross(right, rPos - fPos);
            Mesh mesh = chain.GetComponent<MeshFilter>().sharedMesh;
            MeshL[] meshL = new MeshL[4];
            for (int i = 0; i < 4; i++)
                meshL[i] = new MeshL(mesh.triangles.Length, mesh);
            Vector3[] vert = mesh.vertices;
            int[][] tri = new int[mesh.subMeshCount][];
            for (int i = 0; i < tri.Length; i++)
                tri[i] = mesh.GetTriangles(i);
            for (int s = 0; s < tri.Length; s++)
            {
                int n = tri[s].Length / 3;
                for (int i = 0; i < n; i++)
                {
                    int k = i * 3;
                    Vector3 v = (vert[tri[s][k]] + vert[tri[s][k + 1]] + vert[tri[s][k + 2]]) / 3;
                    float d0 = Vector3.Dot((v - chainPoints[0]).normalized, back1);
                    float d3 = Vector3.Dot(v - chainPoints[3], back2);
                    if (d0 > 0 || d3 > 0)
                    {
                        meshL[0].addTri(k, s);
                    }
                    else
                    {
                        float d1 = Vector3.Dot(v - chainPoints[1], back1);
                        float d2 = Vector3.Dot(v - chainPoints[2], back2);
                        if (d1 < 0 && d2 < 0)
                            meshL[2].addTri(k, s);
                        else
                        {
                            float h = Vector3.Dot(v - rPos, up);
                            if (h > 0)
                                meshL[1].addTri(k, s);
                            else
                                meshL[3].addTri(k, s);
                        }
                    }
                }
            }
            //if (chainParts != null)
              //  foreach (Transform t in chainParts)
                //    if (t != null)
                  //      DestroyImmediate(t.gameObject);
            System.Type[] components = { typeof(MeshFilter), typeof(MeshRenderer) };
            if (chainParts == null || chainParts.Length != 4)
                chainParts = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                Transform tr = chainParts[i];
                if (tr == null)
                {
                    GameObject go = new GameObject("Chain" + i, components);
                    tr = go.transform;
                    chainParts[i] = tr;
                    tr.parent = transform;
                }
                tr.GetComponent<MeshRenderer>().sharedMaterials = chain.GetComponent<MeshRenderer>().sharedMaterials;
                mesh = meshL[i].getMesh();
                mesh.name = "Chain" + i;
                tr.GetComponent<MeshFilter>().mesh = mesh;
                tr.SetPositionAndRotation(chain.position, chain.rotation);
            }
            chainOriginalPos = chainParts[0].localPosition;
            chainOriginalRot = chainParts[0].localRotation;
            chain.gameObject.SetActive(false);
            float t2 = Time.realtimeSinceStartup;
            Debug.Log("splitChain " + (t2 - t1));
        }
        private float getSprocketRadius(bool forward, Vector3[] vertices)
        {
            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            Vector3 center = pos;
            Vector3 toChain = (pos - transform.position);
            toChain.Normalize();
            if (forward)
            {
                center = transform.position;
                toChain *= -1;
            }
            center = chain.InverseTransformPoint(center);
            toChain = chain.InverseTransformVector(toChain).normalized;
            Vector3 axis = -chain.InverseTransformVector(transform.right).normalized;
            float min = 999999999;
            float max = 0;
            Vector3 mid = Vector3.zero;
            int n = 0;
            foreach (Vector3 v in vertices)
            {
                Vector3 r = v - center;
                r -= Vector3.Project(r, axis);
                if (Vector3.Dot(r.normalized, toChain) > 0.1f)
                {
                    min = Mathf.Min(min, r.magnitude);
                    max = Mathf.Max(max, r.magnitude);

                    mid += v;
                    n++;
                }
            }
            if (!forward && n > 0)
            {
                mid /= n;
                rearSprocketPos = chain.InverseTransformPoint(pos);
                rearSprocketPos.x = mid.x;
                
                Vector3 lp = transform.localPosition;
                lp.x = transform.parent.InverseTransformPoint(chain.TransformPoint(mid)).x;
                transform.localPosition = lp;
            }
            float radius = (min + max) / 2;
            return radius;
        }
        private void setChainPoints()
        {
            Vector3 rPos = chain.TransformPoint(rearSprocketPos);
            Vector3 fPos = transform.position;
            float rearR = rearRadius * chain.lossyScale.y;
            float frontR = frontRadius * chain.lossyScale.y;
            float r = rearR - frontR;
            Vector3 D = rPos - fPos;
            float d = D.magnitude;
            if (d == 0)
                return;
            D.Normalize();
            float sin = r / d;
            if (sin > 1)
                return;
            float cos = Mathf.Sqrt(1 - sin * sin);
            Vector3 N = Vector3.Cross(D, transform.right).normalized;
            Vector3 R0 = (-D * sin + N * cos) * rearR;
            Vector3 R1 = (-D * sin + N * cos) * frontR;
            Vector3 R2 = (-D * sin - N * cos) * frontR;
            Vector3 R3 = (-D * sin - N * cos) * rearR;
            localCP = new Vector3[4];
            localCP[0] = rPos + R0;
            localCP[1] = fPos + R1;
            localCP[2] = fPos + R2;
            localCP[3] = rPos + R3;
            chainPoints = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                chainPoints[i] = chain.InverseTransformPoint(localCP[i]);
                localCP[i] = transform.InverseTransformPoint(localCP[i]);
            }
        }
        public void saveMesh()
        {
            if (chainParts == null || chainParts.Length != 4 || chainParts[0] == null)
                return;
            Mesh mesh = chainParts[0].GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
                return;
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(mesh, meshPath);
            string path = AssetDatabase.GetAssetPath(mesh);
            for (int i = 1; i < 4; i++)
                AssetDatabase.AddObjectToAsset(chainParts[i].GetComponent<MeshFilter>().sharedMesh, mesh);
            AssetDatabase.SaveAssets();
#endif
        }
        [System.Serializable]
        public class MeshL
        {
            public List<Vector3> vertices;
            public List<int>[] triangles;
            public List<Vector3> normals;
            public List<Vector2> uv;

            private int[][] tri;
            private List<Vector3> vert;
            private List<Vector3> norm;
            private List<Vector2> uvL;
            public MeshL(int capacity, Mesh origin)
            {
                vertices = new List<Vector3>(capacity);
                normals = new List<Vector3>(capacity);
                uv = new List<Vector2>(capacity);

                triangles = new List<int>[origin.subMeshCount];
                for (int i = 0; i < origin.subMeshCount; i++)
                    triangles[i] = new List<int>(capacity);

                vert = new List<Vector3>(capacity);
                norm = new List<Vector3>(capacity);
                uvL = new List<Vector2>(capacity);
                origin.GetVertices(vert);
                //tri = (int[])origin.triangles.Clone();
                tri = new int[origin.subMeshCount][];
                for (int i = 0; i < origin.subMeshCount; i++)
                    tri[i] = origin.GetTriangles(i);
                origin.GetNormals(norm);
                origin.GetUVs(0, uvL);
            }
            public void addTri(int i, int subMesh)
            {
                if (i > tri[subMesh].Length - 3)
                { 
                }
                int i1 = tri[subMesh][i];
                int i2 = tri[subMesh][i + 1];
                int i3 = tri[subMesh][i + 2];

                vertices.Add(vert[i1]);
                vertices.Add(vert[i2]);
                vertices.Add(vert[i3]);

                normals.Add(norm[i1]);
                normals.Add(norm[i2]);
                normals.Add(norm[i3]);

                uv.Add(uvL[i1]);
                uv.Add(uvL[i2]);
                uv.Add(uvL[i3]);

                triangles[subMesh].Add(vertices.Count - 3);
                triangles[subMesh].Add(vertices.Count - 2);
                triangles[subMesh].Add(vertices.Count - 1);
            }
            public Mesh getMesh()
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.uv = uv.ToArray();

                mesh.subMeshCount = triangles.Length;
                for (int i = 0; i < triangles.Length; i++)
                    mesh.SetTriangles(triangles[i], i);

                mesh.RecalculateBounds();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
        [System.Serializable]
        public class Triangles
        {
            public List<int> triangles;
            public Triangles(int capacity)
            {
                triangles = new List<int>(capacity);
            }
        }
    }
}