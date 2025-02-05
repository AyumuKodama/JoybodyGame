using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#321-trackspline-script")]
    public class TrackSpline : MonoBehaviour
    {
        [HideInInspector] public bool showHiddenValues;
        [HideInInspector] public bool showTrack;
        [HideInInspector] public bool showHills;
        [HideInInspector] public bool showVecors;
        [HideInInspector] public bool showNode;
        [HideInInspector] public bool moreOptions;
        [HideInInspector] public ViewOptions viewOptions;
        [HideInInspector] public ViewScope viewScope;
        //public float borderWidth = 2;
        [Space()]
        public Settings settings;
        public List<Jump> jumps;
        [HideInInspector] public Spline1 spline;
        [HideInInspector] public Spline1 splineIn;
        [HideInInspector] public Spline1 splineOut;
        [HideInInspector] public Spline1 hillIn;
        //[HideInInspector] public Spline1 hillIn1;
        //[HideInInspector] public Spline1 hillIn2;
        [HideInInspector] public Spline1 hillOut;
        //[HideInInspector] public Spline1 hillOut1;
        //[HideInInspector] public Spline1 hillOut2;
        [HideInInspector] public List<Node> nodes;
        [HideInInspector] public List<SplineSegment> meridiansIn;
        [HideInInspector] public List<SplineSegment> meridiansOut;
        [HideInInspector] public int selectedIndex;
        [HideInInspector] public Quaternion handleRotation = Quaternion.identity;
        [HideInInspector] public bool innerCollapsed;
        [HideInInspector] public Vector3 innerCenter;
        //[HideInInspector] public SelectedPoints selectedPoint;

        [HideInInspector] public LPoint startFrom;
        [HideInInspector] public LPoint startTo;
        [HideInInspector] public LPoint startTrack;
        [Range(0, 1)]
        [HideInInspector] public float startTrackWidth;
        private Vector3 startTrackPos1;
        private Vector3 startTrackPos2;
        [HideInInspector] public bool clockwise;
        [HideInInspector] public bool planeTrack;
        [HideInInspector] public float trackWidth;
        [HideInInspector] public float hillWidth;
        void Start()
        {
            Init();
        }
        private void Awake()
        {
            Init();
        }
        void Update()
        {
        }
        public void Init()
        {
            if (spline == null)
                reset();
            if (spline.segments == null)
                reset();
            spline.init();
            if (splineIn == null)
                reset();
            if (splineOut == null)
                reset();
            if (nodes == null)
                reset();
            if (nodes.Count > spline.segments.Count)
                while (nodes.Count > spline.segments.Count)
                    nodes.RemoveAt(nodes.Count - 1);
            foreach (Node n in nodes)
                if (n.maxVelocity == 0)
                    n.maxVelocity = 100;
            //clockwise = settings.clockwise;
            if (trackWidth == 0)
                trackWidth = settings.trackWidth;
            if (hillWidth == 0)
                hillWidth = settings.hillWidth;
            //planeTrack = settings.planeTrack;
        }
        public void reset()
        {
            if (settings.segmentCount < 3)
                settings.segmentCount = 3;
            //par = settings.Clone();
            selectedIndex = 0;
            clockwise = settings.clockwise;
            trackWidth = settings.trackWidth;
            hillWidth = settings.hillWidth;
            planeTrack = settings.planeTrack;
            innerCollapsed = false;
            int count = settings.segmentCount;

            nodes = new List<Node>(count);
            for (int i = 0; i < count; i++)
                nodes.Add(new Node(trackWidth, hillWidth));

            float r = settings.radius;
            float r1 = settings.radius - trackWidth / 2;
            float r2 = settings.radius + trackWidth / 2;
            bool cw = settings.clockwise;
            spline = new Spline1(count, r, cw);
            splineIn = new Spline1(count, r1, cw);
            splineOut = new Spline1(count, r2, cw);
            if (!planeTrack)
            {
                float r3 = r1 - hillWidth;
                float r4 = r2 + hillWidth;
                hillIn = new Spline1(count, r3, cw);
                hillOut = new Spline1(count, r4, cw);

                meridiansIn = new List<SplineSegment>(count);
                meridiansOut = new List<SplineSegment>(count);
                for (int i = 0; i < count; i++)
                {
                    Vector3 p0 = splineIn.segments[i].P0;
                    Vector3 p3 = hillIn.segments[i].P0;
                    meridiansIn.Add(new SplineSegment(p0, p3));

                    p0 = splineOut.segments[i].P0;
                    p3 = hillOut.segments[i].P0;
                    meridiansOut.Add(new SplineSegment(p0, p3));
                }
            }
            float hDev = settings.horizontalRandomDeviations;
            float vDev = settings.verticalRandomDeviations;
            for (int i = 0; i < count; i++)
            {
                float hd = Random.Range(-hDev, hDev);
                float vd = Random.Range(0, vDev);

                Vector3 p = spline.segments[i].P0;
                p *= (hd / r + 1);
                p.y += vd;
                selectedIndex = i;
                setPos0(transform.TransformPoint(p));
            }
            for (int i = 0; i < count; i++)
            {
                setTangents(i);
                updateSide(i);
            }

            land();
            updateSpline();
        }
        public void land()
        {
            float minY = 10 * settings.radius;
            foreach (SplineSegment s in spline.segments)
                for (int i = 0; i < 100; i++)
                {
                    float t = (float)i / 100;
                    s.getTrackSlope(t, viewOptions.velocity, out Vector3 left, out Vector3 right);
                    Vector3 p = s.GetPoint(t);
                    minY = Mathf.Min(minY, p.y + left.y * trackWidth * 0.0f);
                    minY = Mathf.Min(minY, p.y + right.y * trackWidth * 0.0f);
                }
            for (int i = 0; i < spline.segments.Count; i++)
            {
                spline.segments[i].addY(-minY);
                splineIn.segments[i].addY(-minY);
                splineOut.segments[i].addY(-minY);
                if (!planeTrack)
                {
                    meridiansIn[i].setP0y(meridiansIn[i].P0.y - minY);
                    meridiansIn[i].setP1y(meridiansIn[i].P1.y - minY);

                    meridiansOut[i].setP0y(meridiansOut[i].P0.y - minY);
                    meridiansOut[i].setP1y(meridiansOut[i].P1.y - minY);
                }
            }
        }
        public Jump getJump(float s)
        {
            foreach (Jump jump in jumps)
                if (s >= jump.startS && s <= jump.endS)
                    return jump;
            return null;
        }
        public Vector3 getMarker(float s)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node n = nodes[i];
                float s1 = -spline.length * 2;
                float s2 = -spline.length * 2;
                float t1 = n.inMarker.from;
                float t2 = n.outMarker.from;
                SplineSegment ss1 = splineIn.segments[i];
                SplineSegment ss2 = splineOut.segments[i];
                float s0 = spline.segments[i].offsetInSpline;
                if (n.inMarker.from != n.inMarker.to)
                    s1 = ss1.getS(t1) + s0;
                if (n.outMarker.from != n.outMarker.to)
                    s2 = ss2.getS(t2) + s0;
                Vector3 p = Vector3.zero;
                if (s1 > s && s2 > s)
                {
                    if (s1 < s2)
                        p = ss1.GetPoint(n.inMarker.from);
                    else
                        p = ss2.GetPoint(n.outMarker.from);
                }
                else
                {
                    if (s1 > s)
                        p = ss1.GetPoint(n.inMarker.from);
                    if (s2 > s)
                        p = ss2.GetPoint(n.outMarker.from);
                }
                if (p != Vector3.zero)
                    return p;
            }
            return Vector3.zero;
        }
        public void getNormalSides(float t, int index, out float t1, out float t2)
        {
            SplineSegment s = spline.segments[index];
            SplineSegment s1 = splineIn.segments[index];
            SplineSegment s2 = splineOut.segments[index];
            Vector3 q1 = s1.GetPoint(t);
            Vector3 q2 = s2.GetPoint(t);
            Vector3 p = s.GetPoint(t);
            Vector3 tangent = s.getDerivate1(t).normalized;
            float d1 = Vector3.Dot(Vector3.Project(q1 - p, tangent), tangent);
            float d2 = Vector3.Dot(Vector3.Project(q2 - p, tangent), tangent);
            t1 = Mathf.Clamp01(t - d1 / s1.length * 0.6f);
            t2 = Mathf.Clamp01(t - d2 / s2.length * 0.6f);
        }
        public void resetTangents()
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].tangent = Tangents.mid;
            for (int i = 0; i < nodes.Count; i++)
            {
                setTangents(i);
                updateSide(i);
            }
        }
        public void resetWidth()
        {
            trackWidth = settings.trackWidth;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].inWidth = trackWidth / 2;
                nodes[i].outWidth = trackWidth / 2;
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                updateSide(i);
            }
        }
        public void resetHills()
        {
            if (planeTrack)
                return;
            updateSpline();
            int count = spline.segments.Count;
            hillWidth = settings.hillWidth;
            for (int i = 0; i < count; i++)
            {
                nodes[i].hillInWidth = hillWidth;
                nodes[i].hillOutWidth = hillWidth;
                setTangents(i);
                updateSide(i);
                normalizeMeridian(true, i);
                normalizeMeridian(false, i);
            }

            land();
            updateSpline();
        }
        public void addNode()
        {
            spline.addSegment(selectedIndex);
            splineIn.addSegment(selectedIndex);
            splineOut.addSegment(selectedIndex);
            int next = spline.nextI(selectedIndex);
            nodes.Insert(next, new Node(trackWidth, hillWidth));
            if (!planeTrack)
            {
                hillIn.addSegment(selectedIndex);
                hillOut.addSegment(selectedIndex);

                Vector3 p0 = splineIn.segments[next].P0;
                Vector3 p3 = hillIn.segments[next].P0;
                Vector3 tangent = (p3 - p0) / 3;
                tangent.y = 0;
                Vector3 p1 = p0 + tangent;
                Vector3 p2 = p3 - tangent;
                meridiansIn.Insert(next, new SplineSegment(p0, p1, p2, p3));

                p0 = splineOut.segments[next].P0;
                p3 = hillOut.segments[next].P0;
                tangent = (p3 - p0) / 3;
                tangent.y = 0;
                p1 = p0 + tangent;
                p2 = p3 - tangent;
                meridiansOut.Insert(next, new SplineSegment(p0, p1, p2, p3));
            }
            selectedIndex = spline.nextI(selectedIndex);
            setPos0(transform.TransformPoint(spline.segments[selectedIndex].P0));
        }
        public void removeNode()
        {
            if (spline.segments.Count < 4)
                return;
            spline.removeSegment(selectedIndex);
            splineIn.removeSegment(selectedIndex);
            splineOut.removeSegment(selectedIndex);
            nodes.RemoveAt(selectedIndex);
            if (!planeTrack)
            {
                hillIn.removeSegment(selectedIndex);
                hillOut.removeSegment(selectedIndex);
                meridiansIn.RemoveAt(selectedIndex);
                meridiansOut.RemoveAt(selectedIndex);
            }
            selectedIndex = spline.prevI(selectedIndex);
            setPos0(transform.TransformPoint(spline.segments[selectedIndex].P0));
        }
        public void updateSpline(bool soft = false)
        {
            float t1 = Time.realtimeSinceStartup;

            float tw;
            if (GetComponent<TrackMesh>() == null)
                tw = trackWidth;
            else
                tw = trackWidth / 2;

            spline.updateLength(soft);
            spline.updateTurns(settings.maxTurnRadius, settings.minTurnAngle, tw, settings.mergeTurns);
            splineIn.updateLength(soft);
            splineOut.updateLength(soft);

            if (jumps == null)
                jumps = new List<Jump>();
            foreach (Jump jump in jumps)
                jump.update(spline);
            /*
            int d = nodes.Count - spline.segments.Count;
            if (d > 0)
            {
                for (int i = 0; i < d; i++)
                    nodes.RemoveAt(nodes.Count - 1);
                Debug.Log("Removed last " + d + " nodes");
            }
            */
            float t2 = Time.realtimeSinceStartup;
            //Debug.Log("updateSpline() " + (t2 - t1) + "s");
        }
        public void collapseInner()
        {
            innerCenter = Vector3.zero;
            for (int i = 0; i < meridiansIn.Count; i++)
                innerCenter += meridiansIn[i].P3;
            innerCenter /= meridiansIn.Count;

            float r = hillIn.radius * 0.001f;
            for (int i = 0; i < meridiansIn.Count; i++)
            {
                int prev = hillIn.prevI(i);
                SplineSegment m = meridiansIn[i];
                SplineSegment s = hillIn.segments[i];
                SplineSegment sp = hillIn.segments[prev];
                float d = (innerCenter - m.P3).magnitude;
                float t = (d - r) / d;
                Vector3 p3 = Vector3.Lerp(m.P3, innerCenter, t);
                Vector3 offset = p3 - m.P3;
                m.P3 += offset;
                m.P2 += offset;
                s.P0 += offset;
                sp.P3 += offset;
                s.P1 = Vector3.Lerp(s.P1, innerCenter, t);
                sp.P2 = Vector3.Lerp(sp.P2, innerCenter, t);
            }

            innerCollapsed = true;
        }
        public void uncollapseInner()
        {
            innerCollapsed = false;
        }
#if UNITY_EDITOR
        public void drawSpline()
        {
            GUIStyle style = new GUIStyle();
            if (viewOptions.treckCurves.show)
            {
                Handles.color = viewOptions.treckCurves.color;
                for (int i = 0; i < spline.segments.Count; i++)
                {
                    drawSegment(spline.segments[i], viewOptions.treckCurves.width);
                    drawSegment(splineIn.segments[i], viewOptions.treckCurves.width);
                    drawSegment(splineOut.segments[i], viewOptions.treckCurves.width);
                }
            }
            if (showHills && viewOptions.hillCurves.show && !planeTrack)
            {
                float w = viewOptions.hillCurves.width;
                Handles.color = viewOptions.hillCurves.color;
                for (int i = 0; i < spline.segments.Count; i++)
                {
                    drawSegment(hillIn.segments[i], w);
                    drawSegment(hillOut.segments[i], w);
                    drawSegment(meridiansIn[i], w);
                    drawSegment(meridiansOut[i], w);

                    //drawSegment(hillIn1.segments[i], w);
                    //drawSegment(hillIn2.segments[i], w);
                    //drawSegment(hillOut1.segments[i], w);
                    //drawSegment(hillOut2.segments[i], w);
                }
            }
            if (showVecors)
            {
                int start, count;
                switch (viewScope)
                {
                    case ViewScope.OneSegment:
                        start = selectedIndex;
                        count = 1;
                        break;
                    case ViewScope.TwoSegments:
                        start = spline.prevI(selectedIndex);
                        count = 2;
                        break;
                    case ViewScope.AllSegments:
                        start = 0;
                        count = spline.count;
                        break;
                    default:
                        start = selectedIndex;
                        count = 1;
                        break;
                }
                int index = start;
                for (int j = start; j < start + count; j++)
                {
                    SplineSegment s = spline.segments[index];
                    for (int i = 0; i < viewOptions.numberOfVectors; i++)
                    {
                        float t = 1f / viewOptions.numberOfVectors * i;
                        Vector3 p = transform.TransformPoint(s.GetPoint(t));
                        if (viewOptions.tangent.show)
                        {
                            Vector3 v = transform.TransformVector(s.getDerivate1(t));
                            Handles.color = viewOptions.tangent.color;
                            Handles.DrawLine(p, p + v, 1);
                        }
                        if (viewOptions.curvatureRadius.show)
                        {
                            Vector3 r = transform.TransformVector(s.getCurvatureVector2d(t));
                            Handles.color = viewOptions.curvatureRadius.color;
                            Handles.DrawLine(p, p + r, 1);
                            style.normal.textColor = viewOptions.curvatureRadius.color;
                            string l = "R=" + r.magnitude.ToString("0.0");
                            Handles.Label(p, l, style);
                        }
                        if (viewOptions.leftRight.show)
                        {
                            s.getLeftRight(t, out Vector3 left, out Vector3 right);
                            left = transform.TransformVector(left) * trackWidth * 0.5f;
                            right = transform.TransformVector(right) * trackWidth * 0.5f;
                            Handles.color = viewOptions.leftRight.color;
                            Handles.color = viewOptions.leftRight.color;
                            Handles.DrawLine(p, p + left, 1);
                            Handles.color = viewOptions.leftRight.color2;
                            Handles.DrawLine(p, p + right, 1);
                        }
                        if (viewOptions.acceleration.show)
                        {
                            float v = viewOptions.velocity;
                            Vector3 a = transform.TransformVector(s.getDerivate2(t)) * 1.0f;
                            Handles.color = viewOptions.acceleration.color;
                            Handles.DrawLine(p, p + a, 1);
                        }
                        if (viewOptions.trackSlope.show)
                        {
                            float v = viewOptions.velocity;
                            s.getTrackSlope(t, v, out Vector3 left, out Vector3 right);
                            left = transform.TransformVector(left) * trackWidth * 0.5f;
                            right = transform.TransformVector(right) * trackWidth * 0.5f;
                            Handles.color = viewOptions.trackSlope.color;
                            Handles.DrawLine(p, p + left, 1);
                            Handles.color = viewOptions.trackSlope.color2;
                            Handles.DrawLine(p, p + right, 1);
                        }
                    }

                    index = spline.nextI(index);
                }
            }
            foreach (SplineSegment s in spline.segments)
            {
                if (viewOptions.label.show)
                {
                    int index = spline.segments.IndexOf(s);
                    style.normal.textColor = viewOptions.label.color;
                    Vector3 p = transform.TransformPoint(s.P0);
                    string l = "    " + index.ToString();
                    l += " l=" + s.length.ToString("0.0");
                    Handles.Label(p, l, style);
                }
                if (viewOptions.nodes.show)
                {
                    drawTangents(s, viewOptions.nodes.width);
                }
                Handles.color = Color.white;
            }
            if (viewOptions.turn.show)
            {
                Handles.color = viewOptions.turn.color;
                style.normal.textColor = viewOptions.turn.color;
                foreach (Spline2.Turn turn in spline.turns)
                {
                    Vector3 p = transform.TransformPoint(turn.position);
                    float size = HandleUtility.GetHandleSize(p) * 0.03f;
                    Handles.DrawSolidDisc(p, Vector3.up, size);

                    Vector3 p1 = transform.TransformPoint(turn.startPoint);
                    Vector3 p2 = transform.TransformPoint(turn.endPoint);
                    Vector3 dir1 = -spline.getDerivate1(turn.startS);
                    Vector3 dir2 = spline.getDerivate1(turn.endS); ;
                    Quaternion r1 = Quaternion.LookRotation(dir1);
                    Quaternion r2 = Quaternion.LookRotation(dir2);
                    Handles.ConeHandleCap(0, p1, r1, size * 5, EventType.Repaint);
                    Handles.ConeHandleCap(0, p2, r2, size * 5, EventType.Repaint);
                    float d1 = turn.s - turn.startS;
                    float d2 = turn.endS - turn.s;
                    string i = spline.turns.IndexOf(turn).ToString();
                    string s = i + " R = " + (turn.radius * turn.leftRight).ToString("0.0") +
                        "\nSmR = " + (turn.smoothRadius * turn.leftRight).ToString("0.0") +
                        "\n[" + d1.ToString("0.0") + ", " + d2.ToString("0.0") +
                        "]\nA=" + turn.angle.ToString("0.0") +
                        "\nS=" + turn.s.ToString("0.000");
                    Handles.Label(p, s, style);

                }
            }
            if (viewOptions.measure.show)
            {
                Handles.color = viewOptions.measure.color;
                style.normal.textColor = viewOptions.measure.color;
                /*
                foreach (SplineUtils.SPoint sp in spline.sPoints)
                {
                    Vector3 p = transform.TransformPoint(sp.pos);
                    string st = "sp " + sp.s.ToString();
                    Handles.Label(p, st, style);
                }
                */
                /*
                float len = 0;
                SplineSegment ss = spline.segments[1];
                for (int i = 0; i < 30; i++)
                {
                    float t = (float)i / 30;
                    len = ss.getLength(t);
                    Vector3 p = transform.TransformPoint(ss.GetPoint(t));
                    Handles.Label(p, len.ToString(), style);
                }
                */
                /*
                for (int i = 0; i < ss.lengthT.Length; i++)
                {
                    float t = (float)(i + 1) / ss.lengthT.Length;
                    len = ss.lengthT[i];
                    Vector3 p = transform.TransformPoint(ss.GetPoint(t));
                    Handles.Label(p, "\n" + len.ToString() + "s", style);
                }
                */

                float step = viewOptions.measure.width;
                float l = spline.getLength();
                float s = 0;
                while (s < l)
                {
                    Vector3 p = transform.TransformPoint(spline.getPoint(s));
                    Handles.Label(p, s.ToString() + "m", style);
                    s += step;
                }
            }
            if (viewOptions.nodes.show)
            {
                if (!planeTrack && showHills && viewOptions.hillTangents.show)
                {
                    Handles.color = viewOptions.hillTangents.color;
                    float width = viewOptions.hillTangents.width;
                    drawTangents(meridiansIn[selectedIndex], width);
                    drawTangents(meridiansOut[selectedIndex], width);
                    //drawTangents(hillIn.segments[selectedIndex], width);
                    //drawTangents(hillOut.segments[selectedIndex], width);
                }
            }
            if (viewOptions.startTrack.show)
            {
                Handles.color = viewOptions.startTrack.color;
                float width = viewOptions.startTrack.width;
                //SplineUtils.SPoint startIn = splineIn.getClosest(startTo.getPos());
                //SplineUtils.SPoint startOut = splineOut.getClosest(startTo.getPos());
                spline.segments[startTrack.seg].getLeftRight(startTrack.t, out Vector3 left, out Vector3 right);
                Vector3 p0 = transform.TransformPoint(startTo.pos + left * trackWidth / 4);
                Vector3 p3 = transform.TransformPoint(startTrackPos1);
                float l = (p3 - p0).magnitude / 3;
                Vector3 p1 = p0 + transform.TransformVector(startTo.tangent).normalized * l;
                Vector3 p2 = p3 - transform.TransformVector(startTrack.tangent).normalized * l;
                Handles.DrawBezier(p0, p3, p1, p2, Handles.color, null, width);

                p0 = transform.TransformPoint(startTo.pos + right * trackWidth / 4);
                p1 = p0 + transform.TransformVector(startTo.tangent).normalized * l;
                p3 = transform.TransformPoint(startTrackPos2);
                p2 = p3 - transform.TransformVector(startTrack.tangent).normalized * l;
                Handles.DrawBezier(p0, p3, p1, p2, Handles.color, null, width);
            }
        }
        private void drawSegment(SplineSegment s, float width)
        {
            Vector3 p0 = transform.TransformPoint(s.P0);
            Vector3 p1 = transform.TransformPoint(s.P1);
            Vector3 p2 = transform.TransformPoint(s.P2);
            Vector3 p3 = transform.TransformPoint(s.P3);

            float size = HandleUtility.GetHandleSize(p0) * 0.02f;
            Handles.DrawSolidDisc(p0, Vector3.up, size);
            Handles.DrawBezier(p0, p3, p1, p2, Handles.color, null, width);
        }
        private void drawTangents(SplineSegment s, float width)
        {
            Vector3 p0 = transform.TransformPoint(s.P0);
            Vector3 p1 = transform.TransformPoint(s.P1);
            Vector3 p2 = transform.TransformPoint(s.P2);
            Vector3 p3 = transform.TransformPoint(s.P3);

            float size = HandleUtility.GetHandleSize(p0) * 0.04f;
            Handles.DrawSolidDisc(p1, Vector3.up, size);
            Handles.DrawSolidDisc(p2, Vector3.up, size);
            Handles.DrawLine(p0, p1, width);
            Handles.DrawLine(p2, p3, width);
        }
#endif
        public int getCount()
        {
            return spline.segments.Count;
        }
        public Vector3 getPos0(int index)
        {
            return transform.TransformPoint(spline.segments[index].P0);
        }
        public Vector3 getPos1(int index)
        {
            SplineSegment s = spline.segments[index];
            return transform.TransformPoint(s.P1);
        }
        public Vector3 getPos2(int index)
        {
            int prev = spline.prevI(index);
            SplineSegment s = spline.segments[prev];
            return transform.TransformPoint(s.P2);
        }
        public void setPos0(Vector3 pos)
        {
            Vector3 localPos = transform.InverseTransformPoint(pos);
            int prev = spline.prevI(selectedIndex);
            SplineSegment s = spline.segments[selectedIndex];
            SplineSegment sp = spline.segments[prev];

            Vector3 offset = localPos - s.P0;
            s.P0 = localPos;
            sp.P3 = localPos;

            if (nodes[selectedIndex].tangent == Tangents.free ||
                nodes[selectedIndex].tangent == Tangents.sharp)
            {
                s.P1 += offset;
                sp.P2 += offset;
            }

            setTangents();

            setBorderPos();
        }
        public void setPos1(Vector3 pos)
        {
            Vector3 localPos = transform.InverseTransformPoint(pos);
            int prev = spline.prevI(selectedIndex);
            SplineSegment s = spline.segments[selectedIndex];
            SplineSegment sp = spline.segments[prev];

            s.P1 = localPos;
            if (nodes[selectedIndex].tangent != Tangents.sharp)
            {
                Vector3 tangent = (s.P1 - s.P0).normalized;
                sp.P2 = s.P0 - tangent * (sp.P2 - sp.P3).magnitude;
            }
            setBorderPos();
        }
        public void setPos2(Vector3 pos)
        {
            Vector3 localPos = transform.InverseTransformPoint(pos);
            int prev = spline.prevI(selectedIndex);
            SplineSegment s = spline.segments[selectedIndex];
            SplineSegment sp = spline.segments[prev];

            sp.P2 = localPos;
            if (nodes[selectedIndex].tangent != Tangents.sharp)
            {
                Vector3 tangent = (sp.P2 - s.P0).normalized;
                s.P1 = s.P0 - tangent * (s.P1 - s.P0).magnitude;
            }
            setBorderPos();
        }
        public void setRotation(Quaternion rot)
        {
            Vector3 e = rot.eulerAngles;
            e.z = 0;
            rot.eulerAngles = e;
            Quaternion diff = rot * Quaternion.Inverse(handleRotation);
            handleRotation = rot;

            SplineSegment s = spline.segments[selectedIndex];
            SplineSegment p = spline.segments[spline.prevI(selectedIndex)];

            Vector3 p1 = s.P0 + diff * (s.P1 - s.P0);
            Vector3 p2 = s.P0 + diff * (p.P2 - s.P0);
            setPos1(transform.TransformPoint(p1));
            setPos2(transform.TransformPoint(p2));
        }
        public Vector3 getInOutPos(out Vector3 posIn, out Vector3 posOut, out Vector3 dirIn, out Vector3 dirOut)
        {
            SplineSegment s = spline.segments[selectedIndex];
            SplineSegment sIn = splineIn.segments[selectedIndex];
            SplineSegment sOut = splineOut.segments[selectedIndex];
            dirIn = transform.TransformVector(sIn.P0 - s.P0).normalized;
            dirOut = transform.TransformVector(sOut.P0 - s.P0).normalized;
            posIn = transform.TransformPoint(sIn.P0);
            posOut = transform.TransformPoint(sOut.P0);
            return transform.TransformPoint(s.P0);
        }
        public Vector3 getHillInPos()
        {
            Vector3 p0 = hillIn.segments[selectedIndex].P0;
            if (nodes[selectedIndex].landInHill)
                p0.y = 0;
            return transform.TransformPoint(p0);
        }
        public Vector3 getHillOutPos()
        {
            Vector3 p0 = hillOut.segments[selectedIndex].P0;
            if (nodes[selectedIndex].landInHill)
                p0.y = 0;
            return transform.TransformPoint(p0);
        }
        public Vector3 getInPos()
        {
            Vector3 p0 = splineIn.segments[selectedIndex].P0;
            if (nodes[selectedIndex].landInHill)
                p0.y = 0;
            return transform.TransformPoint(p0);
        }
        public Vector3 getOutPos()
        {
            Vector3 p0 = splineOut.segments[selectedIndex].P0;
            if (nodes[selectedIndex].landOutHill)
                p0.y = 0;
            return transform.TransformPoint(p0);
        }
        public Node getNode()
        {
            return new Node(nodes[selectedIndex]);
        }
        public Tangents getTangents(int index)
        {
            return nodes[index].tangent;
        }
        public Vector3 getMarkerPos(bool inOut, bool fromTo, out Vector3 tangent)
        {
            SplineSegment s;
            Range marker;
            Node node = nodes[selectedIndex];
            if (inOut)
            {
                s = splineIn.segments[selectedIndex];
                marker = node.inMarker;
            }
            else
            {
                s = splineOut.segments[selectedIndex];
                marker = node.outMarker;
            }
            float t;
            if (fromTo)
            {
                t = marker.from;
                t = Mathf.Clamp(t, 0, marker.to);
                tangent = transform.TransformVector(s.getDerivate1(t).normalized);
            }
            else
            {
                t = marker.to;
                t = Mathf.Clamp(t, marker.from, 1);
                tangent = transform.TransformVector(-s.getDerivate1(t).normalized);
            }
            Vector3 pos = s.GetPoint(t);
            return transform.TransformPoint(pos);
        }
        public void setMarkerPos(bool inOut, bool fromTo, Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            SplineSegment s;
            Node node = nodes[selectedIndex];
            Range marker;
            if (inOut)
            {
                s = splineIn.segments[selectedIndex];
                marker = node.inMarker;
            }
            else
            {
                s = splineOut.segments[selectedIndex];
                marker = node.outMarker;
            }
            float t;
            if (fromTo)
                t = marker.from;
            else
                t = marker.to;

            Vector3 tangent = s.getDerivate1(t).normalized;
            Vector3 prevPos = s.GetPoint(t);
            float dt = Vector3.Dot(pos - prevPos, tangent) / s.length;
            float t1 = Mathf.Clamp01(t + dt);
            //float t1 = Vector3.Dot(pos - s.p0, tangent) / s.lenght;
            if (fromTo)
            {
                t1 = Mathf.Clamp(t1, 0, marker.to);
                marker.from = t1;
            }
            else
            {
                t1 = Mathf.Clamp(t1, marker.from, 1);
                marker.to = t1;
            }
        }
        public Vector3 getTerrainPos(bool fromTo, out Vector3 tangent)
        {
            SplineSegment s = spline.segments[selectedIndex];
            Node node = nodes[selectedIndex];
            Range range = node.terrainRange;
            float t;
            if (fromTo)
            {
                t = range.from;
                t = Mathf.Clamp(t, 0, range.to);
                tangent = transform.TransformVector(s.getDerivate1(t).normalized);
            }
            else
            {
                t = range.to;
                t = Mathf.Clamp(t, range.from, 1);
                tangent = transform.TransformVector(-s.getDerivate1(t).normalized);
            }
            Vector3 pos = s.GetPoint(t);
            return transform.TransformPoint(pos);
        }
        public void setTerrainPos(bool fromTo, Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            SplineSegment s = spline.segments[selectedIndex];
            Node node = nodes[selectedIndex];
            Range range = node.terrainRange;
            float t;
            if (fromTo)
                t = range.from;
            else
                t = range.to;

            Vector3 tangent = s.getDerivate1(t).normalized;
            Vector3 prevPos = s.GetPoint(t);
            float dt = Vector3.Dot(pos - prevPos, tangent) / s.length;
            float t1 = Mathf.Clamp01(t + dt);
            if (fromTo)
            {
                t1 = Mathf.Clamp(t1, 0, range.to);
                range.from = t1;
            }
            else
            {
                t1 = Mathf.Clamp(t1, range.from, 1);
                range.to = t1;
            }
        }
        public Vector3 getJumpPos(int jumpIndex, int part, out Vector3 tangent)
        {
            Jump jump = jumps[jumpIndex];

            if (jump.startPos == null || jump.startTangent == Vector3.zero)
                jump.updateStart(spline);
            if (jump.peakPos == null || jump.peakTangent == Vector3.zero)
                jump.updatePeak(spline);
            if (jump.endPos == null || jump.endTangent == Vector3.zero)
                jump.updateEnd(spline);

            Vector3 pos;
            if (part == 0)
            {
                tangent = transform.TransformVector(jump.startTangent.normalized);
                pos = jump.startPos;
            }
            else if (part == 1)
            {
                tangent = transform.TransformVector(jump.peakTangent.normalized);
                pos = jump.peakPos;
            }
            else
            {
                tangent = transform.TransformVector(-jump.endTangent.normalized);
                pos = jump.endPos;
            }

            return transform.TransformPoint(pos);
        }
        public void setJumpPos(int jumpIndex, int item, Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            Jump jump = jumps[jumpIndex];
            float ds;
            if (item == 0)
            {
                ds = Vector3.Dot(pos - jump.startPos, jump.startTangent.normalized);
                jump.startS += ds;
                jump.updateStart(spline);
            }
            else if (item == 1)
            {
                ds = Vector3.Dot(pos - jump.peakPos, jump.peakTangent.normalized);
                jump.peakS += ds;
                jump.updatePeak(spline);
            }
            else
            {
                ds = Vector3.Dot(pos - jump.endPos, jump.endTangent.normalized);
                jump.endS += ds;
                jump.updateEnd(spline);
            }
        }
        public Vector3 getStartPos(bool fromTo, out Vector3 tangent)
        {
            Vector3 pos;
            if (fromTo)
            {
                if (startFrom.pos == Vector3.zero)
                    startFrom.setPos(spline.getPoint(0), spline);
                pos = startFrom.pos;
                tangent = transform.TransformVector(startFrom.tangent.normalized);
            }
            else
            {
                if (startTo.pos == Vector3.zero)
                    startTo.setPos(spline.getPoint(0), spline);
                pos = startTo.pos;
                tangent = -transform.TransformVector(startTo.tangent.normalized);
            }
            return transform.TransformPoint(pos);
        }
        public void setStartPos(bool fromTo, Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            LPoint start;
            if (fromTo)
                start = startFrom;
            else
                start = startTo;
            start.setPos(pos, spline);
        }
        public Vector3 getStartTrackPos(out Vector3 tangent, out Vector3 pos1, out Vector3 pos2)
        {
            /*
            pos1 = splineIn.getClosestL(startTrack.pos, out float cl1, startTrackCl1);
            pos2 = splineOut.getClosestL(startTrack.pos, out float cl2, startTrackCl2);
            startTrackCl1 = cl1;
            startTrackCl2 = cl2;
            pos1 = Vector3.Lerp(startTrack.pos, pos1, startTrackWidth / 2);
            pos2 = Vector3.Lerp(startTrack.pos, pos2, startTrackWidth / 2);
            */
            if (startTo.pos == Vector3.zero)
                startTo.setPos(spline.getPoint(0), spline);
            if (startTrack.pos == Vector3.zero)
                startTrack.setPos(startTo.pos, spline);

            spline.segments[startTrack.seg].getLeftRight(startTrack.t, out Vector3 left, out Vector3 right);
            pos1 = startTrack.pos + left * trackWidth / 4 * startTrackWidth;
            pos2 = startTrack.pos + right * trackWidth / 4 * startTrackWidth;
            startTrackPos1 = pos1;
            startTrackPos2 = pos2;
            pos1 = transform.TransformPoint(pos1);
            pos2 = transform.TransformPoint(pos2);

            tangent = transform.TransformVector(startTrack.tangent.normalized);
            return transform.TransformPoint(startTrack.pos);
        }
        public void setStartTrackPos(Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            startTrack.setPos(pos, spline);
        }
        public void setStartTrackWidth(Vector3 pos)
        {
            pos = transform.InverseTransformPoint(pos);
            Vector3 diff = pos - startTrack.pos;
            startTrackWidth = Mathf.Clamp(diff.magnitude / trackWidth * 2, 0, 0.5f) * 2;

            spline.segments[startTrack.seg].getLeftRight(startTrack.t, out Vector3 left, out Vector3 right);

            if (Vector3.Dot(diff, left) < 0)
                startTrackWidth = 0;
        }

        public void updateTangents(Tangents tangents)
        {
            if (tangents != nodes[selectedIndex].tangent)
            {
                nodes[selectedIndex].tangent = tangents;
                setTangents();
                setBorderPos();
            }
        }
        public void updateNode(Node node)
        {
            if (node.tangent != nodes[selectedIndex].tangent)
            {
                nodes[selectedIndex].tangent = node.tangent;
                setTangents();
                setBorderPos();
            }
            if (node.inWidth != nodes[selectedIndex].inWidth)
            {
                nodes[selectedIndex].inWidth = node.inWidth;
                setBorderPos();
            }
            if (node.outWidth != nodes[selectedIndex].outWidth)
            {
                nodes[selectedIndex].outWidth = node.outWidth;
                setBorderPos();
            }
            if (node.hillInWidth != nodes[selectedIndex].hillInWidth)
            {
                nodes[selectedIndex].hillInWidth = node.hillInWidth;
                setBorderPos();
            }
            if (node.hillOutWidth != nodes[selectedIndex].hillOutWidth)
            {
                nodes[selectedIndex].hillOutWidth = node.hillOutWidth;
                setBorderPos();
            }
            if (node.maxVelocity != nodes[selectedIndex].maxVelocity)
            {
                nodes[selectedIndex].maxVelocity = node.maxVelocity;
                setBorderPos();
            }
            if (node.landInHill != nodes[selectedIndex].landInHill)
            {
                nodes[selectedIndex].landInHill = node.landInHill;
                setTangents();
                setBorderPos();
            }
            if (node.landOutHill != nodes[selectedIndex].landOutHill)
            {
                nodes[selectedIndex].landOutHill = node.landOutHill;
                setTangents();
                setBorderPos();
            }
        }
        public void onHendleButtonClick(int iClick)
        {
            selectedIndex = iClick;

            SplineSegment s = spline.segments[iClick];
            SplineSegment sr = splineOut.segments[iClick];
            if (clockwise)
                sr = splineIn.segments[iClick];
            Vector3 forward = transform.TransformVector(s.getDerivate1(0)).normalized;
            Vector3 right = transform.TransformVector(sr.P0 - s.P0).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            handleRotation = Quaternion.LookRotation(forward, up);
        }
        private void setTangents()
        {
            int prev = spline.prevI(selectedIndex);
            int next = spline.nextI(selectedIndex);
            setTangents(selectedIndex);
            setTangents(prev);
            setTangents(next);
        }
        private void setTangents(int index)
        {
            if (nodes[index].tangent == Tangents.sharp)
                return;

            int prevI = spline.prevI(index);
            SplineSegment s = spline.segments[index];
            SplineSegment prev = spline.segments[prevI];

            Vector3 tangent;
            float k = 1.3f / 3, kPrev = 1.3f / 3;
            switch (nodes[index].tangent)
            {
                case Tangents.mid:
                    {
                        Vector3 v1 = (s.P3 - s.P0).normalized;
                        Vector3 v2 = (prev.P0 - s.P0).normalized;
                        Vector3 bisector = (v1 + v2);
                        if (bisector.magnitude > Vector3.kEpsilon)
                        {
                            Vector3 normal = Vector3.Cross(v1, v2).normalized;
                            tangent = Vector3.Cross(bisector, normal).normalized;
                        }
                        else
                            tangent = (s.P3 - prev.P0).normalized;
                        break;
                    }
                case Tangents.forward:
                    {
                        tangent = (s.P3 - s.P0).normalized;
                        kPrev = 2f / 3;
                        break;
                    }
                case Tangents.backward:
                    {
                        tangent = (prev.P3 - prev.P0).normalized;
                        k = 2f / 3;
                        break;
                    }
                case Tangents.free:
                    {
                        Vector3 t1 = s.P1 - s.P0;
                        Vector3 t2 = prev.P3 - prev.P2;
                        Vector3 t = (t1 + t2).normalized;
                        s.P1 = s.P0 + t * t1.magnitude;
                        prev.P2 = prev.P3 - t * t2.magnitude;
                        return;
                    }
                default:
                    {
                        tangent = (s.P3 - prev.P0).normalized;
                        break;
                    }
            }

            float l = (s.P3 - s.P0).magnitude;
            float lPrev = (prev.P3 - prev.P0).magnitude;
            float lTotal = l + lPrev;
            float lOposit = (s.P3 - prev.P0).magnitude;
            l *= lOposit / lTotal;
            lPrev *= lOposit / lTotal;
            s.P1 = s.P0 + tangent * l * k;
            prev.P2 = prev.P3 - tangent * lPrev * kPrev;
        }

        private void setBorderPos()
        {
            int prev = spline.prevI(selectedIndex);
            int next = spline.nextI(selectedIndex);
            updateSide(selectedIndex);
            updateSide(prev);
            updateSide(next);
        }
        private void updateSide(int index)
        {
            int prev = spline.prevI(selectedIndex);
            SplineSegment s = spline.segments[index];
            SplineSegment sp = spline.segments[prev];
            s.getLeftRight(0, out Vector3 leftC, out Vector3 rightC);
            sp.getLeftRight(1, out Vector3 leftP, out Vector3 rightP);
            Vector3 left, right;
            if (nodes[index].tangent == Tangents.sharp)
            {
                left = (leftC + leftP).normalized;
                right = (rightC + rightP).normalized;
            }
            else
            {
                left = leftC;
                right = rightC;
            }
            Vector3 In, Out;
            if (spline.clockwise)
            {
                Out = left;
                In = right;
            }
            else
            {
                In = left;
                Out = right;
            }

            Node n = nodes[index];
            updateSide(index, splineIn, In * n.inWidth);
            updateSide(index, splineOut, Out * n.outWidth);
            if (!planeTrack)
            {
                if (!innerCollapsed)
                    updateSide(index, hillIn, In * (n.hillInWidth + n.inWidth));
                updateSide(index, hillOut, Out * (n.hillOutWidth + n.outWidth));
                updateMeridian(true, index);
                updateMeridian(false, index);

                //Vector3 p0 = spline.segments[index].p0;
                //updateSide(index, hillIn1, meridiansIn[index].p1 - p0);
                //updateSide(index, hillIn2, meridiansIn[index].p2 - p0);
                //updateSide(index, hillOut1, meridiansOut[index].p1 - p0);
                //updateSide(index, hillOut2, meridiansOut[index].p2 - p0);
            }
        }
        private void updateSide(int index, Spline1 spln, Vector3 offset)
        {
            int prev = spline.prevI(index);
            SplineSegment s = spline.segments[index];
            SplineSegment prevS = spline.segments[prev];
            SplineSegment r = spln.segments[index];
            SplineSegment prevR = spln.segments[prev];

            r.P0 = s.P0 + offset;
            prevR.P3 = s.P0 + offset;

            Vector3 tangent1 = (s.P1 - s.P0) / (s.P3 - s.P0).magnitude;
            Vector3 tangent2 = (prevS.P2 - prevS.P3) / (prevS.P3 - prevS.P0).magnitude;

            float sL = (r.P3 - r.P0).magnitude;
            float prevL = (prevR.P3 - prevR.P0).magnitude;

            r.P1 = r.P0 + tangent1 * sL;
            prevR.P2 = prevR.P3 + tangent2 * prevL;
            Node n = nodes[index];
            if ((spln == hillIn && n.landInHill) || (spln == hillOut && n.landOutHill))
            {
                r.setP0y(0);
                r.setP1y(0);
                prevR.setP2y(0);
                prevR.setP3y(0);
            }
        }
        private void updateMeridian(bool In, int index)
        {
            Vector3 start, end;
            SplineSegment m;
            if (In)
            {
                start = splineIn.segments[index].P0;
                end = hillIn.segments[index].P0;
                m = meridiansIn[index];
            }
            else
            {
                start = splineOut.segments[index].P0;
                end = hillOut.segments[index].P0;
                m = meridiansOut[index];
            }
            Vector3 proj = end - start;
            Vector3 proj1 = m.P1 - m.P0;
            Vector3 proj2 = m.P2 - m.P3;
            proj.y = 0;
            proj1.y = 0;
            proj2.y = 0;
            float l1 = proj1.magnitude;
            float l2 = proj2.magnitude;
            float h1 = (m.P1.y - m.P0.y);
            float h2 = (m.P2.y - m.P3.y);
            proj.Normalize();
            m.P0 = start;
            m.P3 = end;
            m.P1 = m.P0 + proj * l1 + Vector3.up * h1;
            m.P2 = m.P3 - proj * l2 + Vector3.up * h2;
        }
        private void updateMeridian1(bool In, int index)
        {
            Vector3 start, end;
            SplineSegment m;
            if (In)
            {
                start = splineIn.segments[index].P0;
                end = hillIn.segments[index].P0;
                m = meridiansIn[index];
            }
            else
            {
                start = splineOut.segments[index].P0;
                end = hillOut.segments[index].P0;
                m = meridiansOut[index];
            }
            Vector3 p1 = m.P1 - m.P0;
            Vector3 p2 = m.P2 - m.P3;
            m.P0 = start;
            m.P3 = end;
            m.P1 = m.P0 + p1;
            m.P2 = m.P3 + p2;
        }
        private void normalizeMeridian(bool In, int index)
        {
            SplineSegment m;
            if (In)
                m = meridiansIn[index];
            else
                m = meridiansOut[index];
            Vector3 dir = m.P3 - m.P0;
            m.P1 = m.P0 + dir / 3;
            m.P2 = m.P0 + dir * 2 / 3;
            m.setP1y(m.P0.y);
            m.setP2y(m.P3.y);
        }

        public Vector3 getMeridianNormal(out Vector3 in1, out Vector3 in2, out Vector3 out1, out Vector3 out2)
        {
            SplineSegment mIn = meridiansIn[selectedIndex];
            SplineSegment mOut = meridiansOut[selectedIndex];
            Vector3 n = Vector3.Cross(mIn.P3 - mOut.P3, Vector3.up);
            in1 = transform.TransformPoint(mIn.P1);
            in2 = transform.TransformPoint(mIn.P2);
            out1 = transform.TransformPoint(mOut.P1);
            out2 = transform.TransformPoint(mOut.P2);
            return transform.TransformVector(n).normalized;
        }
        public void setIn1(Vector3 pos)
        {
            meridiansIn[selectedIndex].P1 = transform.InverseTransformPoint(pos);
        }
        public void setIn2(Vector3 pos)
        {
            meridiansIn[selectedIndex].P2 = transform.InverseTransformPoint(pos);
        }
        public void setOut1(Vector3 pos)
        {
            meridiansOut[selectedIndex].P1 = transform.InverseTransformPoint(pos);
        }
        public void setOut2(Vector3 pos)
        {
            meridiansOut[selectedIndex].P2 = transform.InverseTransformPoint(pos);
        }

        public Vector3 getCenter()
        {
            return transform.TransformPoint(innerCenter);
        }
        public void setCenter(Vector3 center)
        {
            center = transform.InverseTransformPoint(center);
            Vector3 offset = center - innerCenter;
            innerCenter += offset;
            foreach (SplineSegment s in hillIn.segments)
            {
                s.P0 += offset;
                s.P1 += offset;
                s.P2 += offset;
                s.P3 += offset;
            }
            foreach (SplineSegment s in meridiansIn)
            {
                s.P3 += offset;
                s.P2 += offset;
            }
        }

        [System.Serializable]
        public class ViewOption
        {
            public bool show = false;
            public Color color = Color.white;
            public Color color2 = Color.white;
            public float width = 1;
        }
        [System.Serializable]
        public class ViewOptions
        {
            public ViewOption treckCurves;
            public ViewOption nodes;
            public ViewOption width;
            public ViewOption terrain;
            public ViewOption markers;

            public ViewOption hillCurves;
            public ViewOption hillWidth;
            public ViewOption hillTangents;

            [Header("Segment points")]
            public int numberOfVectors = 10;
            public ViewOption tangent;
            public ViewOption curvatureRadius;
            public ViewOption leftRight;
            public float velocity = 1;
            public ViewOption acceleration;
            public ViewOption trackSlope;
            public ViewOption turn;
            public ViewOption jump;
            public ViewOption start;
            public ViewOption startTrack;
            public ViewOption label;
            public ViewOption measure;
            public ViewOption hint;
        }
        [System.Serializable]
        public class Settings
        {
            public bool clockwise;
            public bool planeTrack;
            public float radius = 300;
            public float maxTurnRadius = 30;
            public float minTurnAngle = 45;
            public bool mergeTurns = true;
            public float trackWidth = 10;
            public float hillWidth = 10;
            public int segmentCount = 6;
            public float verticalRandomDeviations;
            public float horizontalRandomDeviations;
            public Settings Clone()
            {
                return (Settings)this.MemberwiseClone();
            }
        }
        [System.Serializable]
        public class Range
        {
            public Range(float from, float to)
            {
                this.from = from;
                this.to = to;
            }
            public Range(Range range)
            {
                from = range.from;
                to = range.to;
            }
            public bool include(float value)
            {
                return value >= from && value <= to;
            }
            public float from;
            public float to;
        }
        [System.Serializable]
        public enum Tangents { mid, forward, backward, free, sharp }
        public enum ViewScope { OneSegment, TwoSegments, AllSegments }
        [System.Serializable]
        public class Node
        {
            public Tangents tangent;
            public float inWidth;
            public float outWidth;
            public float hillInWidth;
            public float hillOutWidth;
            public float maxVelocity = 100;
            public bool landInHill;
            public bool landOutHill;
            public Range inMarker;
            public Range outMarker;
            public Range terrainRange;
            public Node(float trackWidth, float hillWidth)
            {
                tangent = Tangents.mid;
                inWidth = trackWidth / 2;
                outWidth = trackWidth / 2;
                hillInWidth = hillWidth;
                hillOutWidth = hillWidth;
                maxVelocity = 100;
                landInHill = true;
                landOutHill = true;
                inMarker = new Range(0, 0);
                outMarker = new Range(0, 0);
                terrainRange = new Range(0, 1);
            }
            public Node(Node source)
            {
                tangent = source.tangent;
                inWidth = source.inWidth;
                outWidth = source.outWidth;
                hillInWidth = source.hillInWidth;
                hillOutWidth = source.hillOutWidth;
                maxVelocity = source.maxVelocity;
                landInHill = source.landInHill;
                landOutHill = source.landOutHill;
                inMarker = source.inMarker;
                outMarker = source.outMarker;
                terrainRange = source.terrainRange;
            }
        }
        [System.Serializable]
        public class Jump
        {
            public float startS;
            public float peakS;
            public float endS;
            [HideInInspector] public Vector3 startPos;
            [HideInInspector] public Vector3 peakPos;
            [HideInInspector] public Vector3 endPos;
            [HideInInspector] public Vector3 startTangent;
            [HideInInspector] public Vector3 peakTangent;
            [HideInInspector] public Vector3 endTangent;
            public Jump()
            {
                startTangent = Vector3.forward;
                peakTangent = Vector3.forward;
                endTangent = Vector3.forward;
            }

            public void updateStart(Spline1 spline)
            {
                startS = SplineBase.clampS(startS, spline.length);
                startPos = spline.getPoint(startS);
                startTangent = spline.getDerivate1(startS);
            }
            public void updatePeak(Spline1 spline)
            {
                peakS = SplineBase.clampS(peakS, spline.length);
                peakPos = spline.getPoint(peakS);
                peakTangent = spline.getDerivate1(peakS);
            }
            public void updateEnd(Spline1 spline)
            {
                endS = SplineBase.clampS(endS, spline.length);
                endPos = spline.getPoint(endS);
                endTangent = spline.getDerivate1(endS);
            }
            public void update(Spline1 spline)
            {
                updateStart(spline);
                updatePeak(spline);
                updateEnd(spline);
            }
        }
        [System.Serializable]
        public class LPoint
        {
            public LPoint()
            {
                l = 0;
                seg = 0;
                t = 0;
                pos = Vector3.zero;
                tangent = Vector3.one;
            }
            public void setPos(Vector3 pos, Spline1 spline)
            {
                this.pos = spline.getClosestL(pos, out float l);
                this.tangent = spline.getDerivate1(l);
                this.l = l;
                this.t = spline.getSegT(l, out int seg);
                this.seg = seg;

            }
            public float l;
            public int seg;
            public float t;
            public Vector3 pos;
            public Vector3 tangent;
        }
    }
}