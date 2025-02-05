using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#322-trackspline2")]
    public class TrackSpline2 : MonoBehaviour
    {
        [HideInInspector] public bool showSpline = true;
        [HideInInspector] public bool showVecors;
        [HideInInspector] public ViewOptions viewOptions;
        private float trackWidth = 10;
        //public float borderWidth = 2;
        [Space()]
        public Settings settings;

        [HideInInspector] public Spline2 spline;
        [HideInInspector] public List<Spline2.Boor> selectedBoor;

        [SerializeField] [HideInInspector] private Vector3 toolPosition;
        [SerializeField] [HideInInspector] private Quaternion toolRotation = Quaternion.identity;

        [HideInInspector] public float peaksInOut = 0.5f;

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
            if (spline == null || spline.segments == null)
                spline = new Spline2(
                settings.segmentCount,
                settings.radius,
                settings.horizontalRandomDeviations,
                settings.verticalRandomDeviations);
            if (selectedBoor == null)
                selectedBoor = new List<Spline2.Boor>();

            //spline.update(true);
        }
        public void createFromTurns()
        {
            TrackSpline trackSpline = GetComponent<TrackSpline>();
            if (trackSpline == null)
                return;
            spline.createFromTurns(trackSpline.spline, trackSpline.trackWidth);
        }
        public void fitToPeaks()
        {
            TrackSpline trackSpline = GetComponent<TrackSpline>();
            if (trackSpline == null)
                return;
            spline.fitToPeaks(trackSpline.spline, trackSpline.splineIn, trackSpline.splineOut, trackSpline.trackWidth, peaksInOut);
        }

#if UNITY_EDITOR
        public void drawSpline()
        {
            GUIStyle style = new GUIStyle();
            foreach (Spline2.Boor b in spline.boor)
            {
                if (viewOptions.controlNet.show)
                {
                    int i = spline.boor.IndexOf(b);
                    int next = nextI(i);
                    Vector3 nextB = transform.TransformPoint(spline.boor[next].p);
                    Vector3 B = transform.TransformPoint(b.p);
                    Handles.color = viewOptions.controlNet.color;
                    Handles.DrawLine(B, nextB, 1);
                    /*
                    style.normal.textColor = viewOptions.controlNet.color;
                    float size = HandleUtility.GetHandleSize(B) * 0.3f;
                    B.z += size;
                    Handles.Label(B, i.ToString(), style);
                    */
                }
            }
            foreach (SplineSegment s in spline.segments)
            {
                if (viewOptions.bezierCurve.show)
                {
                    Vector3 p0 = transform.TransformPoint(s.P0);
                    Vector3 p1 = transform.TransformPoint(s.P1);
                    Vector3 p2 = transform.TransformPoint(s.P2);
                    Vector3 p3 = transform.TransformPoint(s.P3);

                    Handles.color = viewOptions.bezierCurve.color;
                    float size = HandleUtility.GetHandleSize(p0) * 0.03f;
                    Handles.DrawSolidDisc(p0, Vector3.up, size);

                    Color color = viewOptions.bezierCurve.color;
                    Handles.DrawBezier(p0, p3, p1, p2, color, null, 3);
                    /*
                    int i = spline2.segments.IndexOf(s);
                    style.normal.textColor = viewOptions.bezierCurve.color;
                    p0.z += size * 10;
                    Handles.Label(p0, i.ToString(), style);
                    */
                }
                if (viewOptions.controlNet.show)
                {
                    Vector3 p0 = transform.TransformPoint(s.P0);
                    Vector3 p1 = transform.TransformPoint(s.P1);
                    Vector3 p2 = transform.TransformPoint(s.P2);
                    Vector3 p3 = transform.TransformPoint(s.P3);
                    Handles.color = viewOptions.controlNet.color;
                    float size = HandleUtility.GetHandleSize(p0) * 0.03f;
                    Handles.DrawSolidDisc(p1, Vector3.up, size);
                    Handles.DrawLine(p0, p1, 1);
                    Handles.DrawSolidDisc(p2, Vector3.up, size);
                    Handles.DrawLine(p2, p3, 1);
                }
                if (showVecors)
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
                        if (viewOptions.curvatureRadius.show)
                        {
                            Vector3 v = transform.TransformVector(s.getCurvatureVector2d(t)) * 1.0f;
                            Handles.color = viewOptions.curvatureRadius.color;
                            Handles.DrawLine(p, p + v, 1);
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
                    Handles.Label(p, "R=" + turn.radius, style);

                }
            }
        }

        public void onHendleButtonClick(int iClick)
        {
            toolRotation = Quaternion.identity;
            Event e = Event.current;
            if (e.control)
            {
                if (selectedBoor.Contains(spline.boor[iClick]))
                {
                    selectedBoor.Remove(spline.boor[iClick]);
                }
                else
                {
                    selectedBoor.Add(spline.boor[iClick]);
                    toolPosition = spline.boor[iClick].p;
                }
            }
            else if (e.shift)
            {
                int index = prevI(iClick);
                int count = 0;
                bool found = false;
                while (index != iClick)
                {
                    count++;
                    if (selected(index))
                    {
                        found = true;
                        break;
                    }
                    index = prevI(index);
                }
                if (found)
                {
                    selectedBoor.Clear();
                    int i = index;
                    while (count >= 0)
                    {
                        selectedBoor.Add(spline.boor[i]);
                        i = nextI(i);
                        count--;
                    }
                    toolPosition = spline.boor[index].p;
                }
                else
                {
                    selectedBoor.Add(spline.boor[iClick]);
                    toolPosition = spline.boor[iClick].p;
                }
            }
            else
            {
                selectedBoor.Clear();
                selectedBoor.Add(spline.boor[iClick]);
                toolPosition = spline.boor[iClick].p;
            }
            if (Tools.pivotMode == PivotMode.Center && selectedBoor.Count > 0)
            {
                toolPosition = Vector3.zero;
                foreach (Spline2.Boor b in selectedBoor)
                    toolPosition += b.p;
                toolPosition /= selectedBoor.Count;
            }
        }
#endif
        public Vector3 getToolPos()
        {
            return transform.TransformPoint(toolPosition);
        }
        public Quaternion getToolRot()
        {
            return toolRotation;
        }
        public void setToolPos(Vector3 newPosition)
        {
            Vector3 diff = newPosition - transform.TransformPoint(toolPosition);
            toolPosition = transform.InverseTransformPoint(newPosition);
            foreach (Spline2.Boor b in selectedBoor)
            {
                int i = spline.boor.IndexOf(b);
                if (i != -1)
                {
                    Vector3 p = transform.TransformPoint(b.p);
                    setPos(p + diff, i);
                }
            }
        }
        public void setToolRot(Quaternion newRotation)
        {
            Quaternion dr = newRotation * Quaternion.Inverse(toolRotation);
            toolRotation = newRotation;
            foreach (Spline2.Boor b in selectedBoor)
                if (spline.boor.IndexOf(b) != -1)
                    rotate(dr, spline.boor.IndexOf(b));
        }

        public bool selected(int i)
        {
            return selectedBoor.Contains(spline.boor[i]);
        }
        public int getCount()
        {
            return spline.count;
        }
        public Vector3 getPos(int index)
        {
            return transform.TransformPoint(spline.boor[index].p);
        }

        private void setPos(Vector3 pos, int index)
        {
            spline.setPos(transform.InverseTransformPoint(pos), index);
            spline.updateLength();
            /*
            if (outerHill != null)
            {
                //Undo.RecordObject(outerHill, "boorChanged");
                outerHill.boorChanged(index);
            }
            */
        }
        private void rotate(Quaternion rot, int index)
        {
            Vector3 p = getPos(index);
            Vector3 toolPos = getToolPos();
            Vector3 dir = p - toolPos;
            Vector3 newPos = toolPos + rot * dir;
            setPos(newPos, index);
        }
        public void reset()
        {
            if (settings.segmentCount < 3)
                settings.segmentCount = 3;
            spline.reset(
                settings.segmentCount,
                settings.radius,
                settings.horizontalRandomDeviations,
                settings.verticalRandomDeviations);
            land();

            selectedBoor.Clear();
            toolRotation = Quaternion.identity;

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
                    minY = Mathf.Min(minY, p.y + left.y * trackWidth * 0.5f);
                    minY = Mathf.Min(minY, p.y + right.y * trackWidth * 0.5f);
                }
            for (int i = 0; i < spline.boor.Count; i++)
            {
                Vector3 p = spline.boor[i].p;
                p.y -= minY;
                spline.setPos(p, i);
            }
            spline.updateLength();
        }
        private int nextI(int i)
        {
            i++;
            if (i > spline.count - 1)
                i -= spline.count;
            return i;
        }
        private int prevI(int i)
        {
            i--;
            if (i < 0)
                i += spline.count;
            return i;
        }

        [Serializable]
        public class ViewOption
        {
            public bool show = true;
            public Color color = Color.white;
            public Color color2 = Color.white;
        }
        [Serializable]
        public class ViewOptions
        {
            public ViewOption bezierCurve;
            public ViewOption controlNet;
            [Header("Segment points")]
            public int numberOfVectors = 10;
            public ViewOption tangent;
            public ViewOption curvatureRadius;
            public ViewOption leftRight;
            public float velocity = 1;
            public ViewOption acceleration;
            public ViewOption trackSlope;
            public ViewOption turn;
        }
        [Serializable]
        public class Settings
        {
            public float radius = 50;
            public int segmentCount = 6;
            public float verticalRandomDeviations = 0;
            public float horizontalRandomDeviations = 0;
        }
    }

}