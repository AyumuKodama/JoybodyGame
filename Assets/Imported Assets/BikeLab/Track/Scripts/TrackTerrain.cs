using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#33-trackterrain")]
    public class TrackTerrain : MonoBehaviour
    {
        public TrackSpline spline;
        public Terrain terrain;
        public bool clearHeights;
        public bool landTrack;
        public float borderWidth = 2;

        private Vector3 size;
        private int res;
        private float terrainStep;

        private int alphaWidth;
        private int alphaHeight;
        private float alphaXstep;
        private float alphaYstep;
        private float fillingSstep;
        [HideInInspector] public int groundLayer = 0;
        [HideInInspector] public int trackLayer = 1;
        [HideInInspector] public int borderLayer = 2;

        private Spline1 sideIn1;
        private Spline1 sideIn2;
        private Spline1 sideOut1;
        private Spline1 sideOut2;

        private float[,] heights;
        private MapNode[,] map;


        void Start()
        {
            Init();
        }
        public void Init()
        {
            spline.Init();

            size = terrain.terrainData.size;
            res = terrain.terrainData.heightmapResolution;
            terrainStep = size.x / (res - 1);

            alphaWidth = terrain.terrainData.alphamapWidth;
            alphaHeight = terrain.terrainData.alphamapHeight;
            alphaXstep = size.x / alphaWidth;
            alphaYstep = size.z / alphaHeight;
        }

        public void buildTerrain()
        {
            float start = Time.realtimeSinceStartup;
            Debug.Log("Build started");

            Init();
            if (clearHeights)
                heights = new float[res, res];
            else
                heights = terrain.terrainData.GetHeights(0, 0, res, res);
            map = new MapNode[res, res];
            for (int i = 0; i < res; i++)
                for (int j = 0; j < res; j++)
                    map[i, j] = new MapNode();

            fillMap();
            updateTerrain();

            float end = Time.realtimeSinceStartup;
            Debug.Log("Build time " + (end - start));
        }
        private void fillMap()
        {
            fillTrack();

            landHills(true);
            landHills(false);
            if (landTrack)
                landTrack_();

            createSides(true);
            createSides(false);

            fillSides(true);
            fillSides(false);
            //fillInner();
            //fillOuter();
        }
        private void fillTrack()
        {
            for (int i = 0; i < spline.spline.count; i++)
            {
                SplineSegment s = spline.spline.segments[i];
                SplineSegment s1 = spline.splineIn.segments[i];
                SplineSegment s2 = spline.splineOut.segments[i];
                TrackSpline.Range range = spline.nodes[i].terrainRange;
                int segmentSteps = (int)(s.length / terrainStep * 3);
                for (int k = 0; k < segmentSteps; k++)
                {
                    float t = (float)k / segmentSteps;
                    if (t < range.from || t > range.to)
                        continue;
                    addTrack(s1, s2, t);
                }
            }
        }
        private void fillSides(bool inner)
        {
#if UNITY_EDITOR
            SplineSegment s0, s1, s2, s3;
            for (int i = 0; i < sideIn1.count; i++)
            {
                SplineSegment s = spline.spline.segments[i];
                int segmentSteps = (int)(s.length / terrainStep * 10);
                TrackSpline.Range range = spline.nodes[i].terrainRange;

                if (inner)
                {
                    s0 = spline.splineIn.segments[i];
                    s1 = sideIn1.segments[i];
                    s2 = sideIn2.segments[i];
                    s3 = spline.hillIn.segments[i];
                }
                else
                {
                    s0 = spline.splineOut.segments[i];
                    s1 = sideOut1.segments[i];
                    s2 = sideOut2.segments[i];
                    s3 = spline.hillOut.segments[i];
                }
                Vector3[] p0 = Handles.MakeBezierPoints(s0.P0, s0.P3, s0.P1, s0.P2, segmentSteps);
                Vector3[] p1 = Handles.MakeBezierPoints(s1.P0, s1.P3, s1.P1, s1.P2, segmentSteps);
                Vector3[] p2 = Handles.MakeBezierPoints(s2.P0, s2.P3, s2.P1, s2.P2, segmentSteps);
                Vector3[] p3 = Handles.MakeBezierPoints(s3.P0, s3.P3, s3.P1, s3.P2, segmentSteps);
                for (int j = 0; j < segmentSteps; j++)
                {
                    float t = (float)j / segmentSteps;
                    if (t < range.from || t > range.to)
                        continue;
                    p3[j].y = getTerrainHeight(p3[j]);
                    SplineSegment m = new SplineSegment(p0[j], p1[j], p2[j], p3[j]);
                    m.transformPoints(spline.transform);
                    m.InverseTransformPoints(terrain.transform);
                    m.updateLenght();
                    addMeridian(m);
                }
            }
#endif
        }
        private void landHills(bool inner)
        {
            List<SplineSegment> meridians;
            Spline1 hill;

            if (inner)
            {
                meridians = spline.meridiansIn;
                hill = spline.hillIn;
            }
            else
            {
                meridians = spline.meridiansOut;
                hill = spline.hillOut;
            }
            for (int i = 0; i < meridians.Count; i++)
            {
                SplineSegment m = meridians[i];
                SplineSegment s = hill.segments[i];
                float h0 = 0;
                float h1 = 0;
                float h2 = 0;
                float h3 = 0;
                if (!clearHeights)
                {
                    h0 = getTerrainHeight(s.P0);
                    h3 = getTerrainHeight(s.P3);
                    h1 = s.P1.y + (h0 - s.P0.y);
                    h2 = s.P2.y + (h3 - s.P3.y);
                }
                float hOld = m.P3.y - m.P1.y;
                if (false) //(hOld != 0)
                {
                    //m.setP1y(Mathf.LerpUnclamped(m.P0.y, h0, (m.P1.y - m.P0.y) / hOld));
                    //m.setP2y(Mathf.LerpUnclamped(m.P0.y, h0, (m.P2.y - m.P0.y) / hOld));
                }
                else
                {
                    m.setP2y(h0 + m.P2.y - m.P3.y);
                }
                m.setP3y(h0);
                s.setP0y(h0);
                s.setP1y(h1);
                s.setP2y(h2);
                s.setP3y(h3);
            }

        }
        private void landTrack_()
        {
            List<SplineSegment> s = spline.spline.segments;
            List<SplineSegment> sIn = spline.splineIn.segments;
            List<SplineSegment> sOut = spline.splineOut.segments;
            List<SplineSegment> mIn = spline.meridiansIn;
            List<SplineSegment> mOut = spline.meridiansOut;
            float[] h = new float[3];
            for (int i = 0; i < s.Count; i++)
            {
                float h0 = 0;
                float h3 = 0;
                if (!clearHeights)
                {
                    h[0] = getTerrainHeight(s[i].P0);
                    h[1] = getTerrainHeight(sIn[i].P0);
                    h[2] = getTerrainHeight(sOut[i].P0);
                    h0 = Mathf.Max(h);

                    h[0] = getTerrainHeight(s[i].P3);
                    h[1] = getTerrainHeight(sIn[i].P3);
                    h[2] = getTerrainHeight(sOut[i].P3);
                    h3 = Mathf.Max(h);
                }
                landSegment(s[i], h0, h3);
                landSegment(sIn[i], h0, h3, mIn[i]);
                landSegment(sOut[i], h0, h3, mOut[i]);
            }
        }
        private void landSegment(SplineSegment s, float h0, float h3, SplineSegment m = null)
        {
            float h1 = 0;
            float h2 = 0;
            if (!clearHeights)
            {
                h1 = s.P1.y + (h0 - s.P0.y);
                h2 = s.P2.y + (h3 - s.P3.y);
            }
            if (m != null)
            {
                m.setP0y(h0);
                m.setP1y(m.P1.y + (h0 - s.P0.y));
            }
            s.setP0y(h0);
            s.setP1y(h1);
            s.setP2y(h2);
            s.setP3y(h3);
        }
        private void createSides(bool inner)
        {
            List<SplineSegment> meridians;
            Spline1 side;
            float r1, r2;

            if (inner)
            {
                meridians = spline.meridiansIn;
                side = spline.splineIn;
                r1 = spline.settings.radius - spline.trackWidth - spline.hillWidth / 3;
                r2 = r1 - spline.hillWidth / 3;
            }
            else
            {
                meridians = spline.meridiansOut;
                side = spline.splineOut;
                r1 = spline.settings.radius + spline.trackWidth + spline.hillWidth / 3;
                r2 = r1 + spline.hillWidth / 3;
            }
            Spline1 side1 = new Spline1(spline.spline.count, r1, spline.settings.clockwise);
            Spline1 side2 = new Spline1(spline.spline.count, r2, spline.settings.clockwise);
            for (int i = 0; i < side1.count; i++)
            {
                int prev = side1.prevI(i);
                side1.segments[i].P0 = meridians[i].P1;
                side2.segments[i].P0 = meridians[i].P2;
                side1.segments[prev].P3 = meridians[i].P1;
                side2.segments[prev].P3 = meridians[i].P2;
            }
            for (int i = 0; i < side1.count; i++)
            {
                int prev = side1.prevI(i);

                SplineSegment s = spline.spline.segments[i];
                SplineSegment r = side1.segments[i];
                SplineSegment prevS = spline.spline.segments[prev];
                SplineSegment prevR = side1.segments[prev];
                Vector3 tangent1 = (s.P1 - s.P0) / (s.P3 - s.P0).magnitude;
                Vector3 tangent2 = (prevS.P2 - prevS.P3) / (prevS.P3 - prevS.P0).magnitude;
                float sL = (r.P3 - r.P0).magnitude;
                float prevL = (prevR.P3 - prevR.P0).magnitude;
                r.P1 = r.P0 + tangent1 * sL;
                prevR.P2 = prevR.P3 + tangent2 * prevL;

                s = side.segments[i];
                r = side2.segments[i];
                prevS = side.segments[prev];
                prevR = side2.segments[prev];
                tangent1 = (s.P1 - s.P0) / (s.P3 - s.P0).magnitude;
                tangent2 = (prevS.P2 - prevS.P3) / (prevS.P3 - prevS.P0).magnitude;
                sL = (r.P3 - r.P0).magnitude;
                prevL = (prevR.P3 - prevR.P0).magnitude;
                r.P1 = r.P0 + tangent1 * sL;
                prevR.P2 = prevR.P3 + tangent2 * prevL;
            }
            side1.updateLength();
            side2.updateLength();
            if (inner)
            {
                sideIn1 = side1;
                sideIn2 = side2;
            }
            else
            {
                sideOut1 = side1;
                sideOut2 = side2;
            }

        }
        public void drawSides()
        {
            drawSide(sideIn1);
            drawSide(sideIn2);
            drawSide(sideOut1);
            drawSide(sideOut2);
        }
        private void drawSide(Spline1 side)
        {
#if UNITY_EDITOR
            if (side == null)
                return;
            for (int i = 0; i < spline.spline.count; i++)
            {
                Vector3 p0 = spline.transform.TransformPoint(side.segments[i].P0);
                Vector3 p1 = spline.transform.TransformPoint(side.segments[i].P1);
                Vector3 p2 = spline.transform.TransformPoint(side.segments[i].P2);
                Vector3 p3 = spline.transform.TransformPoint(side.segments[i].P3);
                float size = HandleUtility.GetHandleSize(p0) * 0.02f;
                Handles.DrawSolidDisc(p0, Vector3.up, size);
                Handles.DrawSolidDisc(p1, Vector3.up, size);
                Handles.DrawSolidDisc(p2, Vector3.up, size);
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 1);
            }
#endif
        }
        private void addTrack(SplineSegment s1, SplineSegment s2, float t)
        {
            Vector3 p1 = s1.GetPoint(t);
            Vector3 p2 = s2.GetPoint(t);
            int sideSteps = (int)((p2 - p1).magnitude / terrainStep * 1.5f);
            p1 = spline.transform.TransformPoint(p1);
            p2 = spline.transform.TransformPoint(p2);
            p1 = terrain.transform.InverseTransformPoint(p1);
            p2 = terrain.transform.InverseTransformPoint(p2);

            int i = 0, j = 0;
            for (int w = 0; w <= sideSteps; w++)
            {
                float tt = (float)w / sideSteps;
                Vector3 p = Vector3.Lerp(p1, p2, tt);
                getIJ(p, ref i, ref j);
                map[i, j].addTrack(p.y / size.y);
            }
        }
        private void addMeridian(SplineSegment m)
        {
#if UNITY_EDITOR
            int steps = (int)(m.length / terrainStep * 3);
            if (steps < 1)
                return;
            Vector3[] p = Handles.MakeBezierPoints(m.P0, m.P3, m.P1, m.P2, steps);
            int i = 0, j = 0;
            for (int k = 0; k < p.Length; k++)
            {
                getIJ(p[k], ref i, ref j);
                map[i, j].addTrack(p[k].y / size.y);
            }
#endif
        }
        public float getTrecHeight(float t)
        {
            return Mathf.Exp(t * 2) / 7.389f;
        }
        private void updateTerrain()
        {
            int h = map.GetLength(0);
            int w = map.GetLength(1);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    float value = map[i, j].getValue();
                    if (clearHeights)
                        heights[i, j] = value;
                    else
                        heights[i, j] = Mathf.Max(heights[i, j], value);
                }
            terrain.terrainData.SetHeights(0, 0, heights);
        }
        private void getIJ(Vector3 pos, ref int i, ref int j)
        {
            i = Mathf.Clamp(Mathf.RoundToInt(pos.z / terrainStep), 0, res);
            j = Mathf.Clamp(Mathf.RoundToInt(pos.x / terrainStep), 0, res);
        }
        private void getIJ(Vector3 pos, float stepX, float stepZ, ref int i, ref int j)
        {
            pos = transform.TransformPoint(pos);
            pos = terrain.transform.InverseTransformPoint(pos);
            i = Mathf.RoundToInt(pos.z / stepZ);
            j = Mathf.RoundToInt(pos.x / stepX);
        }
        private void alphaIJ(Vector3 pos, ref int ai, ref int aj, ref int fi, ref int fj)
        {
            ai = Mathf.RoundToInt(pos.z / alphaYstep);
            aj = Mathf.RoundToInt(pos.x / alphaXstep);
            fi = Mathf.RoundToInt(pos.z / fillingSstep);
            fj = Mathf.RoundToInt(pos.x / fillingSstep);
        }
        private void drawSegment(SplineSegment s)
        {
#if UNITY_EDITOR
            Handles.DrawBezier(s.P0, s.P3, s.P1, s.P2, Color.white, null, 1);
#endif
        }

        public void drawTrack()
        {
            float t1 = Time.realtimeSinceStartup;
            Init();
            float[,,] map = terrain.terrainData.GetAlphamaps(0, 0, alphaWidth, alphaHeight);
            clearAlphaMap(map);
            /*
            Color[] colors = new Color[2560 * 2560];
            Color blankColor = new Color(0, 0, 0, 0.1f);
            for (int i = 0; i < colors.Length; i++)
                colors[i] = blankColor;
            Texture2D debugT = new Texture2D(2560, 2560);
            debugT.filterMode = FilterMode.Point;
            //debugT.SetPixels(colors);
            Color trackColor = new Color(0, 0, 1, 0.2f);
            Color borderColor = new Color(1, 0, 0, 0.2f);
            */
            int AA = 5;
            int fillingWH = Mathf.Max(alphaWidth, alphaHeight) * AA;
            bool[,] fillingMap = new bool[fillingWH, fillingWH];
            fillingSstep = Mathf.Max(size.x, size.z) / fillingWH;

            float AA2 = 1f / AA / AA;
            float alphaStep = Mathf.Min(alphaXstep, alphaYstep);
            float step = alphaStep / AA / 2;
            int borderSteps = Mathf.RoundToInt(borderWidth / step);
            //int aliasSteps = Mathf.RoundToInt(alphaStep / step);
            int ai = 0, aj = 0, fi = 0, fj = 0;
            for (int k = 0; k < spline.spline.segments.Count; k++)
            {
                SplineSegment s = spline.spline.segments[k];
                SplineSegment sIn = spline.splineIn.segments[k];
                SplineSegment sOut = spline.splineOut.segments[k];
                TrackSpline.Node node = spline.nodes[k];
                int sideSteps = Mathf.RoundToInt((node.inWidth + node.outWidth) / step);
                int segmentSteps = Mathf.RoundToInt(s.length / step);
                for (int i = 0; i < segmentSteps; i++)
                {
                    float t = (float)i / segmentSteps;
                    Vector3 pIn = spline.transform.TransformPoint(sIn.GetPoint(t));
                    Vector3 pOut = spline.transform.TransformPoint(sOut.GetPoint(t));
                    pIn = terrain.transform.InverseTransformPoint(pIn);
                    pOut = terrain.transform.InverseTransformPoint(pOut);
                    for (int j = 0; j <= sideSteps; j++)
                    {
                        float tt = (float)j / (sideSteps + 1);
                        Vector3 p = Vector3.Lerp(pIn, pOut, tt);
                        alphaIJ(p, ref ai, ref aj, ref fi, ref fj);
                        if (fillingMap[fi, fj])
                            continue;
                        fillingMap[fi, fj] = true;

                        if (j > borderSteps && j <= sideSteps - borderSteps)
                        {
                            map[ai, aj, trackLayer] += AA2;
                            //debugT.SetPixel(fj, fi, trackColor);
                        }
                        else
                        {
                            map[ai, aj, borderLayer] += AA2;
                            //debugT.SetPixel(fj, fi, borderColor);
                        }
                    }
                }
            }
            //debugT.Apply();
            //terrain.terrainData.terrainLayers[3].diffuseTexture = debugT;

            int w = map.GetLength(0);
            int h = map.GetLength(1);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    map[i, j, trackLayer] = Mathf.Pow(map[i, j, trackLayer], 0.7f);
                    map[i, j, borderLayer] = Mathf.Pow(map[i, j, borderLayer], 0.7f);
                    map[i, j, groundLayer] = 1 - map[i, j, trackLayer] - map[i, j, borderLayer];
                }
            terrain.terrainData.SetAlphamaps(0, 0, map);
            float t2 = Time.realtimeSinceStartup;
            Debug.Log("drawTrack time = " + (t2 - t1));
        }
        private void clearAlphaMap(float[,,] map)
        {
            int w = map.GetLength(0);
            int h = map.GetLength(1);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    map[i, j, groundLayer] = 1;
                    map[i, j, trackLayer] = 0;
                    map[i, j, borderLayer] = 0;
                }
        }
        public string[] getLayers()
        {
            TerrainLayer[] terrainLayers = terrain.terrainData.terrainLayers;
            string[] layers = new string[terrainLayers.Length];
            for (int i = 0; i < layers.Length; i++)
                layers[i] = terrainLayers[i].name;
            return layers;
        }

        private float getTerrainHeight(Vector3 pos)
        {
            pos = spline.transform.TransformPoint(pos);
            pos = terrain.transform.InverseTransformPoint(pos);
            float x = pos.x / terrain.terrainData.size.x;
            float z = pos.z / terrain.terrainData.size.z;
            float y = terrain.terrainData.GetInterpolatedHeight(x, z);
            return y;
        }
        public class MapNode
        {
            private float trackValue;
            private int count;
            public bool track;
            private float hillValue;
            public MapNode()
            {
                trackValue = 0;
                count = 0;
                track = false;
                hillValue = 0;
            }
            public void addTrack(float value)
            {
                track = true;
                trackValue += value;
                count++;
            }
            public float getValue()
            {
                if (track)
                {
                    if (count == 0)
                        return 0;
                    else
                        return trackValue / count;
                }
                else
                    return hillValue;
            }
        }
        public class ExpHeight
        {
            private float koef;
            private float v1, v2;
            public ExpHeight(float koef)
            {
                this.koef = koef;
                v1 = Mathf.Exp(-koef);
                v2 = Mathf.Exp(koef);
            }
            public float erp(float a, float b, float t)
            {
                if (a == b)
                    return a;
                if (v1 == v2)
                    return (a + b) / 2;
                float s, k1, k2, d;
                if (a < b)
                {
                    s = 1;
                    k1 = -koef;
                    k2 = koef;
                    d = a;
                }
                else
                {
                    s = -1;
                    k1 = koef;
                    k2 = -koef;
                    d = b;
                }
                float x = Mathf.Lerp(k1, k2, t);
                float v = (Mathf.Exp(x) * s - v1) * (b - a) / (v2 - v1) + d;
                return v;
            }
        }
    }
}