using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#34-tracmesh")]
    [RequireComponent(typeof(TrackSpline), typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class TrackMesh : MonoBehaviour
    {
        public Terrain terrain;
        public float resolution = 0.5f;
        public float yOffset;
        public float trackWidth = 10;
        public float height = 2;
        public float markerWidth = 1;
        public bool landRoadside;
        public bool landUnderside;

        public enum Submesh { Top, Left, Right, Bottom, LeftMarker, RightMarker };

        private TrackSpline spline;
        private Spline1 splineL;
        private Spline1 splineR;
        private MeshFilter meshFilter;
        private List<Vector3> vertecies;
        private List<int> trianglesTop;
        private List<int> trianglesSide;
        private List<int> trianglesMarker;
        private List<int> trianglesUnder;
        private List<int> trianglesStart;
        private int startBegin, startEnd;
        private List<Vector2> uvL;
        private List<Vector3> normalsL;
        private Vector2[] uv;
        private Vector3[] normals;
        private Strip stripTop;
        private Strip stripSideL;
        private Strip stripSideR;
        private Strip stripMarkerL;
        private Strip stripMarkerR;
        private Strip stripUnderL;
        private Strip stripUnderR;
        private Strip stripUnder;

        public void Init()
        {
            spline = GetComponent<TrackSpline>();
            spline.Init();
            //spline.clockwise = true; //Uncomment if the internal clockwise variable is not set correctly.
            if (spline.clockwise)
            {
                splineL = spline.splineOut;
                splineR = spline.splineIn;
            }
            else
            {
                splineL = spline.splineIn;
                splineR = spline.splineOut;
            }

            int count = (int)(spline.spline.length * resolution);
            int vCount = count * 16;
            int triCount = count * 6 * 8;
            vertecies = new List<Vector3>(vCount);
            trianglesTop = new List<int>(triCount);
            trianglesSide = new List<int>(triCount);
            trianglesMarker = new List<int>(triCount);
            trianglesUnder = new List<int>(triCount);
            trianglesStart = new List<int>(triCount);
            uvL = new List<Vector2>(vCount);
            normalsL = new List<Vector3>(vCount);
            stripTop = new Strip(vertecies, count);
            stripSideL = new Strip(vertecies, count);
            stripSideR = new Strip(vertecies, count);
            stripMarkerL = new Strip(vertecies, count);
            stripMarkerR = new Strip(vertecies, count);
            stripUnderL = new Strip(vertecies, count);
            stripUnderR = new Strip(vertecies, count);
            stripUnder = new Strip(vertecies, count);
        }

        public void buildMeshL()
        {
            float t1 = Time.realtimeSinceStartup;
            Init();
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = "TrackMesh";
            meshFilter.sharedMesh.subMeshCount = 3;

            addVertices(Submesh.Top);
            addVertices(Submesh.Left);
            addVertices(Submesh.Right);
            addVertices(Submesh.Bottom);
            addVertices(Submesh.LeftMarker);
            addVertices(Submesh.RightMarker);
            meshFilter.sharedMesh.SetVertices(vertecies);
            meshFilter.sharedMesh.uv = uvL.ToArray();
            meshFilter.sharedMesh.SetTriangles(trianglesTop, 0);
            meshFilter.sharedMesh.SetTriangles(trianglesSide, 1);
            meshFilter.sharedMesh.SetTriangles(trianglesMarker, 2);

            addNormals(trianglesTop);
            addNormals(trianglesSide);
            //meshFilter.sharedMesh.normals = normals.ToArray();
            meshFilter.sharedMesh.RecalculateNormals();

            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            float t2 = Time.realtimeSinceStartup;
            Debug.Log("buildMesh t = " + (t2 - t1));
            //AssetDatabase.CreateAsset(meshFilter.mesh, "Assets/Prefabs/track.mesh");
            //AssetDatabase.SaveAssets();
        }
        public void buildMesh()
        {
            float t1 = Time.realtimeSinceStartup;
            Init();
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                return;
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.name = "TrackMesh";
            meshFilter.sharedMesh.subMeshCount = 5;

            build();
            /*
            int begin = 161;
            int end = 514;
            int count = end - begin;
            Vector2Int[] stripStartA = new Vector2Int[count + 1];
            stripTop.strip.CopyTo(begin - 0, stripStartA, 0, count + 1);
            stripTop.strip.RemoveRange(begin, count - 1);
            stripStart.strip.AddRange(stripStartA);
            */
            normals = new Vector3[vertecies.Count];
            uv = new Vector2[vertecies.Count];

            stripTop.addTriangles(trianglesTop);
            stripSideL.addTriangles(trianglesSide);
            stripSideR.addTriangles(trianglesSide);
            stripMarkerL.addTriangles(trianglesMarker);
            stripMarkerR.addTriangles(trianglesMarker);
            stripUnderL.addTriangles(trianglesUnder);
            stripUnderR.addTriangles(trianglesUnder);
            stripUnder.addTriangles(trianglesUnder);
            //stripStart.addTriangles(trianglesStart);

            stripTop.calculate(normals, uv);
            stripSideL.calculate(normals, uv);
            stripSideR.calculate(normals, uv);
            stripMarkerL.calculate(normals, uv);
            stripMarkerR.calculate(normals, uv);
            stripUnderL.calculate(normals, uv);
            stripUnderR.calculate(normals, uv);
            stripUnder.calculate(normals, uv);
            //stripStart.calculate(normals, uv);

            trianglesStart.AddRange(removeStartFromTop());

            meshFilter.sharedMesh.SetVertices(vertecies);
            meshFilter.sharedMesh.uv = uv;
            meshFilter.sharedMesh.SetTriangles(trianglesTop, 0);
            meshFilter.sharedMesh.SetTriangles(trianglesSide, 1);
            meshFilter.sharedMesh.SetTriangles(trianglesMarker, 2);
            meshFilter.sharedMesh.SetTriangles(trianglesUnder, 3);
            meshFilter.sharedMesh.SetTriangles(trianglesStart, 4);
            meshFilter.sharedMesh.normals = normals;
            //meshFilter.sharedMesh.RecalculateNormals();

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                int l = mr.sharedMaterials.Length;
                float scaleY = Mathf.RoundToInt(spline.spline.length / trackWidth);
                float scaleYm = Mathf.RoundToInt(spline.spline.length / markerWidth / 2);
                if (l > 0)
                    mr.sharedMaterials[0].mainTextureScale = new Vector2(1, scaleY);
                if (l > 1)
                    mr.sharedMaterials[1].mainTextureScale = new Vector2(1, scaleY);
                if (l > 2)
                    mr.sharedMaterials[2].mainTextureScale = new Vector2(1, scaleYm);
                if (l > 3)
                    mr.sharedMaterials[3].mainTextureScale = new Vector2(1, scaleY);
                if (l > 4)
                    mr.sharedMaterials[4].mainTextureScale = new Vector2(1, scaleY);
            }

            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
                meshCollider.sharedMesh = meshFilter.sharedMesh;

            float t2 = Time.realtimeSinceStartup;
            Debug.Log("buildMesh t = " + (t2 - t1));
        }
        private void build()
        {
            startBegin = -1;
            startEnd = -1;
            Vector3 size = terrain.terrainData.size;
            float tStep = Mathf.Max(size.x, size.z) / terrain.terrainData.heightmapResolution + 1 / resolution;
            for (int i = 0; i < spline.spline.segments.Count; i++)
            {
                SplineSegment s = spline.spline.segments[i];
                SplineSegment s1 = splineL.segments[i];
                SplineSegment s2 = splineR.segments[i];
                int segmentSteps = Mathf.RoundToInt(s.length * resolution);
                int count = segmentSteps;
                if (i == spline.spline.segments.Count - 1)
                    segmentSteps++;

                TrackSpline.Range lMarker = spline.nodes[i].inMarker;
                TrackSpline.Range rMarker = spline.nodes[i].outMarker;
                if (spline.clockwise)
                {
                    lMarker = spline.nodes[i].outMarker;
                    rMarker = spline.nodes[i].inMarker;
                }

                TrackSpline.Range range = spline.nodes[i].terrainRange;
                TrackSpline.Range rangeP = spline.nodes[spline.spline.prevI(i)].terrainRange;
                TrackSpline.Range rangeN = spline.nodes[spline.spline.nextI(i)].terrainRange;
                float nStep = tStep / s.length * 2;
                TrackSpline.Range sideRange = new TrackSpline.Range(range);
                TrackSpline.Range underRange = new TrackSpline.Range(range);
                if (rangeP.to < 1 || range.from > 0)
                {
                    sideRange.from += nStep;
                    underRange.from -= nStep;
                }
                if (rangeN.from > 0 || range.to < 1)
                {
                    sideRange.to -= nStep;
                    underRange.to += nStep;
                }
                for (int j = 0; j < segmentSteps; j++)
                {
                    float fj = j;
                    float t = Mathf.Clamp01(fj / count);

                    if (startBegin == -1 && i == spline.startFrom.seg && t >= spline.startFrom.t)
                        startBegin = stripTop.strip.Count;
                    if (startEnd == -1 && i == spline.startTo.seg && t >= spline.startTo.t)
                        startEnd = stripTop.strip.Count;

                    // roadside
                    Vector3 q1 = s1.GetPoint(t);
                    Vector3 q2 = s2.GetPoint(t);
                    q1.y += yOffset;
                    q2.y += yOffset;

                    // track
                    Vector3 p = s.GetPoint(t);
                    p.y += yOffset;
                    Vector3 dir = (q1 - q2).normalized * trackWidth / 2;
                    Vector3 p1 = p + dir; //Vector3.Lerp(p, q1, trackWidth);
                    Vector3 p2 = p - dir; //Vector3.Lerp(p, q2, trackWidth);

                    if (landRoadside)
                    {
                        if (sideRange.include(t))
                        {
                            q1.y = Mathf.Min(q1.y, getTerrainHeight(q1));
                            q2.y = Mathf.Min(q2.y, getTerrainHeight(q2));
                        }
                        else
                        {
                            q1.y -= yOffset;
                            q2.y -= yOffset;
                        }
                    }

                    // markers
                    Vector3 m1, m2;
                    if ((lMarker.from < lMarker.to) && t >= lMarker.from && t <= lMarker.to)
                        m1 = p1 + (q1 - p).normalized;
                    else
                        m1 = p1;
                    if ((rMarker.from < rMarker.to) && t >= rMarker.from && t <= rMarker.to)
                        m2 = p2 + (q2 - p).normalized;
                    else
                        m2 = p2;

                    // underside
                    Vector3 u1 = q1;
                    Vector3 u2 = q2;
                    u1.y -= height;
                    u2.y -= height;
                    if (landUnderside && underRange.include(t))
                    {
                        u1.y = Mathf.Min(u1.y, getTerrainHeight(u1));
                        u2.y = Mathf.Min(u2.y, getTerrainHeight(u2));
                    }

                    stripTop.add(p1, p2);
                    stripSideL.add(q1, m1);
                    stripSideR.add(m2, q2);
                    stripMarkerL.add(m1, p1);
                    stripMarkerR.add(p2, m2);
                    stripUnderL.add(u1, q1);
                    stripUnderR.add(q2, u2);
                    stripUnder.add(u2, u1);
                }
            }
        }
        private void buildP()
        {
            Vector3 size = terrain.terrainData.size;
            float tStep = Mathf.Max(size.x, size.z) / terrain.terrainData.heightmapResolution + 1 / resolution;
            for (int i = 0; i < spline.spline.segments.Count; i++)
            {
                SplineSegment s = spline.spline.segments[i];
                SplineSegment s1 = splineL.segments[i];
                SplineSegment s2 = splineR.segments[i];
                int segmentSteps = Mathf.RoundToInt(s.length * resolution);
                int count = segmentSteps;
                if (i == spline.spline.segments.Count - 1) // || i == startSegment - 1)
                    segmentSteps++;

                TrackSpline.Range lMarker = spline.nodes[i].inMarker;
                TrackSpline.Range rMarker = spline.nodes[i].outMarker;

                TrackSpline.Range range = spline.nodes[i].terrainRange;
                TrackSpline.Range rangeP = spline.nodes[spline.spline.prevI(i)].terrainRange;
                TrackSpline.Range rangeN = spline.nodes[spline.spline.nextI(i)].terrainRange;
                float nStep = tStep / s.length * 2;
                TrackSpline.Range sideRange = new TrackSpline.Range(range);
                TrackSpline.Range underRange = new TrackSpline.Range(range);
                if (rangeP.to < 1 || range.from > 0)
                {
                    sideRange.from += nStep;
                    underRange.from -= nStep;
                }
                if (rangeN.from > 0 || range.to < 1)
                {
                    sideRange.to -= nStep;
                    underRange.to += nStep;
                }
                for (int j = 0; j < segmentSteps; j++)
                {
                    float fj = j;
                    //if (j > 0)
                    //  fj += Random.Range(0f, 0.9f);
                    float t = Mathf.Clamp01(fj / count);

                    // roadside
                    Vector3 q1 = s1.GetPoint(t);
                    Vector3 q2 = s2.GetPoint(t);
                    q1.y += yOffset;
                    q2.y += yOffset;

                    // track
                    Vector3 p = s.GetPoint(t);
                    p.y += yOffset;
                    Vector3 dir = (q1 - q2).normalized * trackWidth / 2;
                    Vector3 p1 = p + dir; //Vector3.Lerp(p, q1, trackWidth);
                    Vector3 p2 = p - dir; //Vector3.Lerp(p, q2, trackWidth);

                    if (landRoadside)
                    {
                        if (sideRange.include(t))
                        {
                            q1.y = Mathf.Min(q1.y, getTerrainHeight(q1));
                            q2.y = Mathf.Min(q2.y, getTerrainHeight(q2));
                        }
                        else
                        {
                            q1.y -= yOffset;
                            q2.y -= yOffset;
                        }
                    }

                    // markers
                    Vector3 m1, m2;
                    if ((lMarker.from < lMarker.to) && t >= lMarker.from && t <= lMarker.to)
                        m1 = p1 + (q1 - p).normalized;
                    else
                        m1 = p1;
                    if ((rMarker.from < rMarker.to) && t >= rMarker.from && t <= rMarker.to)
                        m2 = p2 + (q2 - p).normalized;
                    else
                        m2 = p2;

                    // underside
                    Vector3 u1 = q1;
                    Vector3 u2 = q2;
                    u1.y -= height;
                    u2.y -= height;
                    if (landUnderside && underRange.include(t))
                    {
                        u1.y = Mathf.Min(u1.y, getTerrainHeight(u1));
                        u2.y = Mathf.Min(u2.y, getTerrainHeight(u2));
                    }

                    stripTop.add(p1, p2);
                    stripSideL.add(q1, m1);
                    stripSideR.add(m2, q2);
                    stripMarkerL.add(m1, p1);
                    stripMarkerR.add(p2, m2);
                    stripUnderL.add(u1, q1);
                    stripUnderR.add(q2, u2);
                    stripUnder.add(u2, u1);
                }
            }
        }
        private void addVertices(Submesh submesh)
        {
            List<int> triangles;
            bool inverse;
            float uv1X, uv2X;
            float dY1, dY2;
            Spline1 spline1, spline2;
            switch (submesh)
            {
                case Submesh.Top:
                    {
                        triangles = trianglesTop;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = 0;
                        dY2 = 0;
                        spline1 = splineL;
                        spline2 = splineR;
                        break;
                    }
                case Submesh.Bottom:
                    {
                        triangles = trianglesSide;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = -2;
                        dY2 = -2;
                        spline2 = splineL;
                        spline1 = splineR;
                        break;
                    }
                case Submesh.Left:
                    {
                        triangles = trianglesSide;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = -2;
                        dY2 = 0;
                        spline1 = splineL;
                        spline2 = splineL;
                        break;
                    }
                case Submesh.Right:
                    {
                        triangles = trianglesSide;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = 0;
                        dY2 = -2;
                        spline1 = splineR;
                        spline2 = splineR;
                        break;
                    }
                case Submesh.LeftMarker:
                    {
                        triangles = trianglesMarker;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = 0;
                        dY2 = 0;
                        spline1 = splineL;
                        spline2 = splineR;
                        break;
                    }
                case Submesh.RightMarker:
                    {
                        triangles = trianglesMarker;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = 0;
                        dY2 = 0;
                        spline1 = splineL;
                        spline2 = splineR;
                        break;
                    }
                default:
                    {
                        triangles = trianglesTop;
                        inverse = false;
                        uv1X = 0;
                        uv2X = 1;
                        dY1 = 0;
                        dY2 = 0;
                        spline1 = splineL;
                        spline2 = splineR;
                        break;
                    }
            }
            int current = vertecies.Count;
            int first = current;
            float uvY = 0;
            float uvStep = 1f / resolution / spline.settings.trackWidth;
            if (submesh == Submesh.LeftMarker || submesh == Submesh.RightMarker)
                uvStep = 0.25f / resolution;
            for (int i = 0; i < spline.spline.segments.Count; i++)
            {
                SplineSegment s = spline.spline.segments[i];
                SplineSegment s1 = spline1.segments[i];
                SplineSegment s2 = spline2.segments[i];
                int segmentSteps = Mathf.RoundToInt(s.length * resolution);
                TrackSpline.Range lMarker = spline.nodes[i].inMarker;
                TrackSpline.Range rMarker = spline.nodes[i].outMarker;
                if (submesh == Submesh.LeftMarker && (lMarker.from >= lMarker.to))
                    continue;
                if (submesh == Submesh.RightMarker && (rMarker.from >= rMarker.to))
                    continue;
                for (int j = 0; j <= segmentSteps; j++)
                {
                    float t = (float)j / segmentSteps;
                    if (submesh == Submesh.LeftMarker && (t < lMarker.from || t > lMarker.to))
                        continue;
                    if (submesh == Submesh.RightMarker && (t < rMarker.from || t > rMarker.to))
                        continue;
                    Vector3 tangent = s.getDerivate1(t).normalized;
                    Vector3 p = s.GetPoint(t);
                    Vector3 q1 = s1.GetPoint(t);
                    Vector3 q2 = s2.GetPoint(t);
                    float d1 = Vector3.Dot(Vector3.Project(q1 - p, tangent), tangent);
                    float d2 = Vector3.Dot(Vector3.Project(q2 - p, tangent), tangent);
                    float t1 = Mathf.Clamp01(t - d1 / s1.length * 0.6f);
                    float t2 = Mathf.Clamp01(t - d2 / s2.length * 0.6f);
                    q1 = s1.GetPoint(t1);
                    q2 = s2.GetPoint(t2);
                    Vector3 p1 = Vector3.Lerp(p, q1, trackWidth);
                    Vector3 p2 = Vector3.Lerp(p, q2, trackWidth);

                    p1.y += yOffset;
                    p2.y += yOffset;
                    p1.y = Mathf.Max(getTerrainHeight(p1) + 0.01f, p1.y);
                    p2.y = Mathf.Max(getTerrainHeight(p2) + 0.01f, p2.y);
                    p1.y += dY1;
                    p2.y += dY2;
                    if (submesh == Submesh.LeftMarker)
                    {
                        Vector3 p1new = p1 + (p1 - p2).normalized;
                        p2 = p1;
                        p1 = p1new;
                    }
                    if (submesh == Submesh.RightMarker)
                    {
                        Vector3 p2new = p2 + (p2 - p1).normalized;
                        p1 = p2;
                        p2 = p2new;
                    }

                    vertecies.Add(p1);
                    vertecies.Add(p2);

                    if (current > first)
                        addQuad(triangles, current - 2, current, inverse);
                    current = vertecies.Count;

                    uvL.Add(new Vector2(uv1X, uvY));
                    uvL.Add(new Vector2(uv2X, uvY));
                    uvY += uvStep;
                    if (uvY > 1.0001f)
                    {
                        uvY = 0;
                        j--;
                    }
                }
            }
            //if (triangles.Count > 0)
            //  addQuad(triangles, current - 2, first, inverse);

        }
        private void addQuad(List<int> triangles, int prev, int current, bool inverse)
        {
            if (inverse)
            {
                triangles.Add(prev);
                triangles.Add(prev + 1);
                triangles.Add(current);

                triangles.Add(current + 1);
                triangles.Add(current);
                triangles.Add(prev + 1);
            }
            else
            {
                triangles.Add(prev);
                triangles.Add(current);
                triangles.Add(prev + 1);

                triangles.Add(current + 1);
                triangles.Add(prev + 1);
                triangles.Add(current);
            }
        }
        private void addNormals(List<int> triangles)
        {
            List<Vector3> v = vertecies;
            List<int> t = triangles;
            int count = triangles.Count / 6;
            int c, p; // current, prev
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                    p = count - 1;
                else
                    p = i - 1;
                p *= 6;
                c = i * 6;

                Vector3 p0 = v[t[c]];
                Vector3 n1 = Vector3.Cross(v[t[c + 1]] - p0, v[t[c + 2]] - p0);
                Vector3 n2 = Vector3.Cross(v[t[c + 2]] - p0, v[t[p + 2]] - p0);
                Vector3 n3 = Vector3.Cross(v[t[p + 2]] - p0, v[t[p]] - p0);
                normalsL.Add((n1 + n2 + n3).normalized);

                p0 = v[t[c + 2]];
                n1 = Vector3.Cross(v[t[p + 2]] - p0, v[t[c]] - p0);
                n2 = Vector3.Cross(v[t[c]] - p0, v[t[c + 1]] - p0);
                n3 = Vector3.Cross(v[t[c + 1]] - p0, v[t[c + 3]] - p0);
                normalsL.Add((n1 + n2 + n3).normalized);
            }
        }
        private float getTerrainHeight(Vector3 pos)
        {
            pos = transform.TransformPoint(pos);
            pos = terrain.transform.InverseTransformPoint(pos);
            float x = pos.x / terrain.terrainData.size.x;
            float z = pos.z / terrain.terrainData.size.z;
            float y = terrain.terrainData.GetInterpolatedHeight(x, z);
            return y;
        }
        private int[] removeStartFromTop()
        {
            int begin, end, count;
            int[] triStart;
            if (startBegin <= startEnd)
            {
                begin = startBegin * 6;
                end = startEnd * 6;
                count = end - begin;
                triStart = new int[count];
                trianglesTop.CopyTo(begin, triStart, 0, count);
                trianglesTop.RemoveRange(begin, count);
            }
            else
            {
                int count1 = trianglesTop.Count - startBegin * 6;
                int count2 = startEnd * 6;
                count = count1 + count2;
                triStart = new int[count];
                begin = startBegin * 6;
                trianglesTop.CopyTo(begin, triStart, 0, count1);
                trianglesTop.CopyTo(0, triStart, count1, count2);
            }
            return triStart;
        }

        public class Strip
        {
            public List<Vector2Int> strip;
            private List<Vector3> vertecies;
            public Strip(List<Vector3> vertecies, int count = 0)
            {
                this.vertecies = vertecies;
                strip = new List<Vector2Int>(count);
            }
            public void add(Vector3 p1, Vector3 p2)
            {
                vertecies.Add(p1);
                vertecies.Add(p2);
                strip.Add(new Vector2Int(vertecies.Count - 2, vertecies.Count - 1));
            }
            public void addTriangles(List<int> triangles)
            {
                //    --->
                //  |\--<-| Y
                //  | \   |
                //  |  \  |
                //  |   \ |
                //  |->--\| X
                //  e1    e2
                for (int i = 1; i < strip.Count; i++)
                {
                    Vector2Int e1 = strip[i - 1];
                    Vector2Int e2 = strip[i];

                    triangles.Add(e1.x);
                    triangles.Add(e2.x);
                    triangles.Add(e1.y);

                    triangles.Add(e2.y);
                    triangles.Add(e1.y);
                    triangles.Add(e2.x);
                }
            }
            public void calculateNormals(Vector3[] normals)
            {
                Vector2Int e1, e2, e3;
                Vector3 x1, x2, x3, y1, y2, y3;
                Vector3 n1, n2, n3, n4;
                //    --->
                //  |\----|\----| Y
                //  | \ n2| \ n4|
                //  |  \  |  \  |
                //  |n1 \ |n3 \ |
                //  |----\|----\| X
                //  e1    e2    e3
                for (int i = 1; i < strip.Count - 1; i++)
                {
                    e1 = strip[i - 1];
                    e2 = strip[i];
                    e3 = strip[i + 1];

                    x1 = vertecies[e1.x];
                    x2 = vertecies[e2.x];
                    x3 = vertecies[e3.x];
                    y1 = vertecies[e1.y];
                    y2 = vertecies[e2.y];
                    y3 = vertecies[e3.y];

                    n1 = Vector3.Cross(y1 - x1, x2 - x1).normalized;
                    n2 = Vector3.Cross(y1 - y2, x2 - y2).normalized;
                    n3 = Vector3.Cross(y2 - x2, x3 - x2).normalized;
                    n4 = Vector3.Cross(x3 - y3, y2 - y3).normalized;
                    if (x1 == y1)
                    {
                        n1 = Vector3.Cross(x2 - y2, y1 - y2).normalized;
                        n2 = n1;
                    }
                    if (x3 == y3)
                    {
                        n4 = n3;
                    }

                    normals[e2.x] = -(n1 + n2 + n3).normalized;
                    normals[e2.y] = -(n2 + n3 + n4).normalized;
                }
                if (strip.Count > 1)
                {
                    e1 = strip[0];
                    e2 = strip[1];
                    x1 = vertecies[e1.x];
                    x2 = vertecies[e2.x];
                    y1 = vertecies[e1.y];
                    y2 = vertecies[e2.y];
                    n1 = Vector3.Cross(y1 - x1, x2 - x1).normalized;
                    n2 = Vector3.Cross(x2 - y2, y1 - y2).normalized;
                    if (x1 == y1)
                    {
                        n1 = Vector3.Cross(x2 - y2, y1 - y2).normalized;
                        n2 = n1;
                    }
                    normals[e1.x] = -n1;
                    normals[e1.y] = -(n1 + n2).normalized;

                    e2 = strip[strip.Count - 2];
                    e3 = strip[strip.Count - 1];
                    x2 = vertecies[e2.x];
                    x3 = vertecies[e3.x];
                    y2 = vertecies[e2.y];
                    y3 = vertecies[e3.y];
                    n3 = Vector3.Cross(y2 - x2, x3 - x2).normalized;
                    n4 = Vector3.Cross(x3 - y3, y2 - y3).normalized;
                    if (x3 == y3)
                    {
                        n4 = n3;
                    }
                    normals[e3.x] = -(n3 + n4).normalized;
                    normals[e3.y] = -n4;
                }
            }
            public void calculateUV(Vector2[] UV)
            {
                if (strip.Count < 2)
                    return;
                for (int i = 0; i < strip.Count; i++)
                {
                    float uv = (float)i / (strip.Count - 1);
                    UV[strip[i].x].x = 0;
                    UV[strip[i].x].y = uv;
                    UV[strip[i].y].x = 1;
                    UV[strip[i].y].y = uv;
                }
            }
            public void calculate(Vector3[] normals, Vector2[] UV)
            {
                calculateNormals(normals);
                calculateUV(UV);
            }
        }
    }
}