using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
namespace VK.BikeLab
{
    [Serializable]
    public class SplineSegment
    {
        [SerializeField] private Vector3 p0, p1, p2, p3;
        public float length;
        public float offsetInSpline;
        public float minRadius;

        [SerializeField] private float[] t2l, l2t;

        public Vector3 P0 { get { return p0; } set { p0 = value; } }
        public Vector3 P1 { get { return p1; } set { p1 = value; } }
        public Vector3 P2 { get { return p2; } set { p2 = value; } }
        public Vector3 P3 { get { return p3; } set { p3 = value; } }

        public SplineSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            updateLenght();
        }
        public SplineSegment(Vector3 p0, Vector3 p3)
        {
            this.p0 = p0;
            this.p1 = p0 + (p3 - p0) / 3f;
            this.p2 = p0 + (p3 - p0) * 2f / 3f;
            this.p3 = p3;
            updateLenght();
        }
        public SplineSegment()
        {
            P0 = Vector3.zero;
            P1 = Vector3.zero;
            P2 = Vector3.zero;
            P3 = Vector3.zero;
        }
        public void addY(float value)
        {
            p0.y += value;
            p1.y += value;
            p2.y += value;
            p3.y += value;
            updateLenght();
        }
        public void setP0y(float value)
        {
            p0.y = value;
            updateLenght();
        }
        public void setP1y(float value)
        {
            p1.y = value;
            updateLenght();
        }
        public void setP2y(float value)
        {
            p2.y = value;
            updateLenght();
        }
        public void setP3y(float value)
        {
            p3.y = value;
            updateLenght();
        }
        public float getS(float t)
        {
            float l = interpolateArr(t, 1f, t2l);
            return l;
        }
        public float getT(float s)
        {
            float t = interpolateArr(s, length, l2t);
            return t;
        }
        private float interpolateArr(float value, float range, float[] arr)
        {
            float t;
            //if (value != range)
            //value %= range;
            value = Mathf.Clamp(value, 0, range);

            float V = value / range * arr.Length;
            int i = Mathf.FloorToInt(V);
            if (i < arr.Length)
            {
                float t1 = 0;
                if (i > 0)
                    t1 = arr[i - 1];
                float t2 = arr[i];
                float theta = V - i;
                t = Mathf.Lerp(t1, t2, theta);
            }
            else
            {
                t = arr[arr.Length - 1];
            }

            return t;
        }

        public Vector3 GetPoint(float t)
        {
            float t1 = Mathf.Clamp(t, 0, 1);
            float t2 = t1 * t1;
            float t3 = t2 * t1;

            float m1 = 1 - t1;
            float m2 = m1 * m1;
            float m3 = m2 * m1;

            Vector3 point = m3 * P0 + 3 * m2 * t1 * P1 + 3 * m1 * t2 * P2 + t3 * P3;
            return point;
        }
        public Vector3 GetPointL(float l)
        {
            float t = getT(l);
            return GetPoint(t);
        }
        public Vector3 getDerivate1(float t)
        {
            float t1 = Mathf.Clamp(t, 0, 1);
            float t2 = t1 * t1;

            float m1 = 1 - t1;
            float m2 = m1 * m1;

            Vector3 v = 3 * m2 * (P1 - P0) + 6 * m1 * t1 * (P2 - P1) + 3 * t2 * (P3 - P2);
            return v;
        }
        public Vector3 getDerivate2(float t)
        {
            //float t1 = Mathf.Clamp(t, 0, 1);
            //float m1 = 1 - t1;
            //Vector3 c = 6 * m1 * (p0 - 2 * p1 + p2) + 6 * t1 * (p1 - 2 * p2 + p3);

            Vector3 a = 6 * (-P0 + 3 * P1 - 3 * P2 + P3) * t + 2 * (3 * P0 - 6 * P1 + 3 * P2);
            return a;
        }
        public float getLength(float t)
        {
            if (t >= 1)
                return length;
            int i = Mathf.FloorToInt(t * t2l.Length);
            float dt = t - (float)i / t2l.Length;
            float theta = dt * t2l.Length;
            float prevL = 0;
            if (i > 0)
                prevL = t2l[i - 1];
            float s = Mathf.Lerp(prevL, t2l[i], theta);
            return s;
        }
        public void getLeftRight(float t, out Vector3 left, out Vector3 right)
        {
            Vector3 v = getDerivate1(t);
            left = new Vector3(-v.z, 0, v.x);
            right = new Vector3(v.z, 0, -v.x);
            left.Normalize();
            right.Normalize();
        }
        public Vector3 getCurvatureVector2d(float t)
        {
            Vector3 d1 = getDerivate1(t);
            Vector3 d2 = getDerivate2(t);
            d1.y = 0;
            d2.y = 0;
            Vector3 cross = Vector3.Cross(d1, d2);
            float m = d1.magnitude;
            float R = m * m * m / cross.magnitude;
            Vector3 r = Vector3.Cross(cross, d1).normalized * R;
            return r;
        }
        public Vector3 getCurvatureVector3d(float t)
        {
            Vector3 d1 = getDerivate1(t);
            Vector3 d2 = getDerivate2(t);
            Vector3 cross = Vector3.Cross(d1, d2);
            float m = d1.magnitude;
            float R = m * m * m / cross.magnitude;
            Vector3 r = Vector3.Cross(cross, d1).normalized * R;
            return r;
        }
        public float getCurvatureRadius2d(float t)
        {
            Vector3 d1 = getDerivate1(t);
            Vector3 d2 = getDerivate2(t);
            d1.y = 0;
            d2.y = 0;
            float cross = Vector3.Cross(d1, d2).magnitude;
            float m = d1.magnitude;
            float r = m * m * m / (cross + 0.0001f);
            return r;
        }
        public float getCurvatureRadius3d(float t)
        {
            Vector3 d1 = getDerivate1(t);
            Vector3 d2 = getDerivate2(t);
            float cross = Vector3.Cross(d1, d2).magnitude;
            float m = d1.magnitude;
            float r = m * m * m / cross;
            return r;
        }
        public float getSignedRadius2d(float t)
        {
            Vector3 d1 = getDerivate1(t);
            Vector3 d2 = getDerivate2(t);
            d1.y = 0;
            d2.y = 0;
            Vector3 cross = Vector3.Cross(d1, d2);
            float sign = Mathf.Sign(cross.y);
            float m = d1.magnitude;
            float R = m * m * m / cross.magnitude * sign;
            return R;
        }
        public Vector3 getNormalAcceleration(float t, float velocity)
        {
            Vector3 r = getCurvatureVector2d(t);
            Vector3 a = r * velocity * velocity / r.sqrMagnitude;
            return a;
        }
        public void getTrackSlope(float t, float velocity, out Vector3 left, out Vector3 right)
        {
            Vector3 v = getDerivate1(t);
            Vector3 r = getCurvatureVector2d(t);
            if (r != Vector3.zero)
            {
                Vector3 a = r * velocity * velocity / r.sqrMagnitude;
                Vector3 ag = -a + Physics.gravity;
                right = Vector3.Cross(v, ag);
            }
            else
            {
                right = Vector3.Cross(v, Physics.gravity);
            }
            left = -right;
            left.Normalize();
            right.Normalize();
        }
        public Vector3 getClosest(Vector3 point, out float t)
        {
            float t1 = 0;
            float t2 = 1;
            for (int i = 0; i < 10; i++)
            {
                Vector3 p1 = GetPoint(t1);
                Vector3 p2 = GetPoint(t2);
                float d1 = (point - p1).magnitude;
                float d2 = (point - p2).magnitude;
                if (d1 < d2)
                    t2 = (t1 + t2) / 2;
                else
                    t1 = (t1 + t2) / 2;
            }
            t = (t1 + t2) / 2;
            return GetPoint(t);
        }
        public void update(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            updateLenght();
        }
        public void updateLenght()
        {
            float minL = Mathf.Infinity;
            float maxAngle = 0;
            length = 0;
            int n = 10;
            t2l = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                Vector3 pos = GetPoint(t);
                float nextT = (float)(i + 1) / n;
                Vector3 nextP = GetPoint(nextT);
                Vector3 t1 = getDerivate1(t);
                Vector3 t2 = getDerivate1(nextT);
                float angle = Vector3.Angle(t1, t2) * Mathf.Deg2Rad;
                maxAngle = Mathf.Max(maxAngle, angle);
                float cos = Vector3.Dot(t1.normalized, t2.normalized);
                float sinSqr = Mathf.Max((1f - cos) / 2, 0); // half angle formula
                float sinHalfA = Mathf.Sqrt(sinSqr);
                float dl = (nextP - pos).magnitude;
                if (sinHalfA > Mathf.Epsilon * 10 && angle > Mathf.Epsilon * 10)
                {
                    dl *= angle / 2 / sinHalfA;
                }
                if (dl < minL)
                    minL = dl;

                length += dl;
                t2l[i] = length;
            }
            float sin = Mathf.Sin(maxAngle / 2);
            if (sin == 0)
                minRadius = 100000;
            else
                minRadius = length / 2 / n / sin;
            if (minL == 0)
            {
                Debug.LogError("minL == 0");
                return;
            }
            n = Mathf.RoundToInt(length / minL) + 2;
            l2t = new float[n];
            for (int i = 0; i < n; i++)
            {
                float l = (float)(i + 1) / n * length;
                for (int j = 0; j < t2l.Length; j++)
                {
                    if (l < t2l[j])
                    {
                        float l1 = 0;
                        if (j > 0)
                        {
                            l1 = t2l[j - 1];
                        }
                        float l2 = t2l[j];
                        float t1 = (float)j / t2l.Length;
                        float t2 = (float)(j + 1) / t2l.Length;
                        float theta = (l - l1) / (l2 - l1);
                        l2t[i] = Mathf.Lerp(t1, t2, theta);
                        break;
                    }
                }
            }
            l2t[l2t.Length - 1] = 1;
        }
        public void transformPoints(Transform tr)
        {
            P0 = tr.TransformPoint(P0);
            P1 = tr.TransformPoint(P1);
            P2 = tr.TransformPoint(P2);
            P3 = tr.TransformPoint(P3);
            updateLenght();
        }
        public void InverseTransformPoints(Transform tr)
        {
            P0 = tr.InverseTransformPoint(P0);
            P1 = tr.InverseTransformPoint(P1);
            P2 = tr.InverseTransformPoint(P2);
            P3 = tr.InverseTransformPoint(P3);
            updateLenght();
        }
    }
    [Serializable]
    public class SplineBase
    {
        public List<SplineSegment> segments;
        public bool clockwise;
        public int count = 6;
        public float radius = 1;
        public float length;
        public float minLength;
        /// <summary>
        /// <br>This array is for calculating the spline point by length.</br>
        /// <br>1. Each item of the array corresponds to a length l = (i + 1) / n * L</br>
        /// <br>2. The value of each element is the index of the spline segment that contains the point l.</br>
        /// <br>3. Each spline segment contains at least one array point.</br>
        /// </summary>
        private int[] l2segN;
        public List<Turn> turns;
        public float minTurnLength;
        [SerializeField]

        public void init()
        {
            if (l2segN == null || l2segN.Length == 0)
                updateLength();
        }

        public float getSegT(float s, out int seg)
        {
            Profiler.BeginSample("getSegT");
            if (s != 0 && s != length)
                s %= length;
            if (s < 0)
                s += length;
            int i = Mathf.FloorToInt(s / length * l2segN.Length);
            if (s == length)
                i = l2segN.Length - 1;
            if (i < 0 || i > l2segN.Length - 1)
            { }
            seg = l2segN[i];
            SplineSegment ss = segments[seg];
            if (s < ss.offsetInSpline)
                seg--;
            if (s > ss.offsetInSpline + ss.length)
                seg++;
            if (seg < 0)
            {
                seg += segments.Count;
                s += length;
            }
            if (seg > segments.Count - 1)
            {
                seg -= segments.Count;
                s -= length;
            }
            ss = segments[seg];
            float lInSeg = s - ss.offsetInSpline;
            float t = ss.getT(lInSeg);
            Profiler.EndSample();
            return t;
        }
        public Vector3 getPoint(float s)
        {
            float t = getSegT(s, out int seg);
            Vector3 p = segments[seg].GetPoint(t);
            return p;
        }
        public Vector3 getDerivate1(float s)
        {
            float t = getSegT(s, out int seg);
            Vector3 p = segments[seg].getDerivate1(t);
            return p;
        }
        public float getCurvatureRadius2d(float s)
        {
            float t = getSegT(s, out int seg);
            return segments[seg].getCurvatureRadius2d(t);
        }
        public float getSignedRadius2d(float s)
        {
            float t = getSegT(s, out int seg);
            return segments[seg].getSignedRadius2d(t);
        }
        public float getCurvatureRadius3d(float l)
        {
            float t = getSegT(l, out int seg);
            return segments[seg].getCurvatureRadius3d(t);
        }
        public Vector3 getCurvatureVector2d(float s)
        {
            float t = getSegT(s, out int seg);
            return segments[seg].getCurvatureVector2d(t);
        }
        public void getLeftRight(float s, out Vector3 left, out Vector3 right)
        {
            float t = getSegT(s, out int seg);
            segments[seg].getLeftRight(t, out Vector3 l, out Vector3 r);
            left = l;
            right = r;
            return;
        }
        public Turn getTurn(float s)
        {
            Turn minTutn = null;
            float minD = 100000000;
            foreach (Turn turn in turns)
            {
                float d = getFromToS(s, turn.endS);
                if (d < minD)
                {
                    minD = d;
                    minTutn = turn;
                }
            }
            return minTutn;
        }
        public float getFromToS(float fromS, float toS)
        {
            if (fromS < toS)
                return toS - fromS;
            else
                return toS - fromS + length;
        }
        public float getClosestDistance(float fromS, float toS)
        {
            float d = toS - fromS;
            if (Mathf.Abs(d) < length / 2)
                return d;
            else
                return d - length * Mathf.Sign(d);
        }

        public Vector3 getClosestL(Vector3 point, out float closestL, float lastL = -1, float clamp = 4)
        {
            //float t1 = Time.realtimeSinceStartup;
            Profiler.BeginSample("getClosestL");
            closestL = 0;
            int n = Mathf.FloorToInt(length / clamp);

            if (lastL < 0)
            {
                float dist = Mathf.Infinity;
                for (int i = 0; i < n; i++)
                {
                    float l = (float)i / n * length;
                    Vector3 p = getPoint(l);
                    float d = (p - point).magnitude;
                    if (d < dist)
                    {
                        dist = d;
                        closestL = l;
                    }
                }
            }
            else
            {
                closestL = lastL;
            }
            for (int i = 0; i < 10; i++)
            {
                Vector3 p = getPoint(closestL);
                Vector3 dir = point - p;
                Vector3 tangent = getDerivate1(closestL);
                float dl = Vector3.Dot(dir, tangent.normalized);
                dl = Mathf.Clamp(dl, -clamp, clamp);
                closestL += dl;
                if (Mathf.Abs(dl) < 0.1f)
                    break;
                clamp /= 2;
            }
            closestL = clampS(closestL, length);
            Vector3 closestP = getPoint(closestL);
            Profiler.EndSample();
            //float t2 = Time.realtimeSinceStartup;
            //Debug.Log("getClosest " + (t2 - t1));
            return closestP;
        }
        public float getLength()
        {
            return length;
        }

        public void updateLength(bool soft = false)
        {
            if (soft && l2segN != null)
                return;
            minLength = Mathf.Infinity;
            length = 0;
            foreach (SplineSegment s in segments)
            {
                s.updateLenght();
                s.offsetInSpline = length;
                length += s.length;
                minLength = Mathf.Min(minLength, s.length);
            }

            int n = Mathf.FloorToInt(length / minLength) + 2;
            l2segN = new int[n];
            for (int i = 0; i < n; i++)
            {
                float l = (float)(i + 1) / n * length;
                float sl = 0;
                for (int j = 0; j < segments.Count; j++)
                {
                    sl = segments[j].offsetInSpline + segments[j].length;
                    if (sl > l)
                    {
                        l2segN[i] = j;
                        break;
                    }
                }
            }
            l2segN[n - 1] = segments.Count - 1;
        }
        public void updateTurns(float minRadius, float minTurnAngle, float trackWidth, bool mergeTurns)
        {
            trackWidth *= 0.8f;
            /*
            Debug.Log("trackWidth = " + trackWidth);
            for (int i= 10; i < 180; i += 10)
            {
                float r = getSmoothRadius2d(100, (float)i, trackWidth);
                Debug.Log("i = " + i + " r = " + r);
            }
            */
            if (minRadius == -1)
                minRadius = radius / 3;
            List<Turn> turns = new List<Turn>();
            int n = Mathf.RoundToInt(length / minLength * 30);
            float r1 = getCurvatureRadius2d((float)(n - 2) / n * length);
            float r2 = getCurvatureRadius2d((float)(n - 1) / n * length);
            int i2 = n - 1;
            float s2 = (float)i2 / n * length;
            for (int i = 0; i < n; i++)
            {
                float s = (float)i / n * length;
                if (s > 550)
                { }
                float r3 = getCurvatureRadius2d(s);
                if (r2 < r1 && r2 < r3 && r2 < minRadius)
                {
                    Turn turn = new Turn();
                    turn.radius = r2;
                    turn.signedRadius = getSignedRadius2d(s2);
                    turn.s = s2;

                    Vector3 cross = Vector3.Cross(getCurvatureVector2d(s2), getDerivate1(s2));
                    turn.leftRight = (int)Mathf.Sign(cross.y);
                    turns.Add(turn);
                }
                r1 = r2;
                r2 = r3;
                s2 = s;
            }

            minTurnLength = Mathf.Infinity;
            foreach (Turn turn in turns)
            {
                int index = turns.IndexOf(turn);
                getSegT(turn.s, out int seg);
                float step = minLength / 30;
                n = Mathf.RoundToInt(turn.radius / step * 2);
                float s = turn.s;
                float r;
                float prevR = turn.radius;
                float sign = Mathf.Sign(turn.signedRadius);
                for (int i = 1; i < n; i++)
                {
                    s -= step;
                    r = getCurvatureRadius2d(s);
                    float sgn = Mathf.Sign(getSignedRadius2d(s));
                    if (r > turn.radius * 4 || r < turn.radius || sgn != sign)
                        break;
                    prevR = r;
                }
                if (s < 0)
                    s += length;
                turn.startS = s;
                s = turn.s;
                prevR = turn.radius;
                for (int i = 1; i < n; i++)
                {
                    s += step;
                    r = getCurvatureRadius2d(s);
                    float sgn = Mathf.Sign(getSignedRadius2d(s));
                    if (r > turn.radius * 4 || r < turn.radius || sgn != sign)
                        break;
                    prevR = r;
                }
                if (s > length)
                    s -= length;
                turn.endS = s;
                turn.angle = Vector3.Angle(getDerivate1(turn.startS), getDerivate1(turn.endS));
                turn.updateVectors(this);
                turn.smoothRadius = getSmoothRadius2d(turn.radius, turn.angle, trackWidth);

                float l = Mathf.Abs(turn.endS - turn.startS);
                minTurnLength = Mathf.Min(minTurnLength, l);
            }

            if (mergeTurns)
            {
                List<Turn> merge = new List<Turn>(turns.Count);
                for (int i = 0; i < turns.Count - 1; i++)
                {
                    Turn turn = turns[i];
                    Turn next = turns[i + 1];
                    if (next.s <= turn.endS || next.startS <= turn.s) //inclusion
                    {
                        if (turn.radius < next.radius)
                        {
                            merge.Add(new Turn(turn));
                            if (i == turns.Count - 3)
                                merge.Add(new Turn(turns[i + 2]));
                        }
                        else
                        {
                            merge.Add(new Turn(next));
                        }
                        i++;
                        continue;
                    }
                    else if (next.startS <= turn.endS) //intersection
                    {
                        float intersection = turn.endS - next.startS;
                        float k1 = intersection / (turn.endS - turn.s);
                        float k2 = intersection / (next.s - next.startS);
                        if (k1 > 0.5f && k2 > 0.5f)
                        {
                            Turn m = new Turn(turn, next);
                            m.updateVectors(this);
                            merge.Add(m);
                        }
                        else
                        {
                            Turn m1 = new Turn(turn);
                            Turn m2 = new Turn(next);
                            float s;
                            if (k1 < k2)
                                s = next.startS;
                            else
                                s = turn.endS;
                            m1.endS = s;
                            m2.startS = s;
                            m1.updateVectors(this);
                            m2.updateVectors(this);
                            merge.Add(m1);
                            merge.Add(m2);
                        }
                        i++;
                        continue;
                    }
                    else
                    {
                        merge.Add(new Turn(turn));
                        if (i == turns.Count - 2)
                            merge.Add(new Turn(next));
                    }
                }
                turns = merge;
            }

            this.turns = new List<Turn>();
            foreach (Turn turn in turns)
                if (turn.angle > minTurnAngle)
                    this.turns.Add(turn);

            for (int i = 0; i < this.turns.Count; i++)
            {
                Turn turn = this.turns[i];
                Turn prevTurn = this.turns[prevI(i, this.turns.Count)];
                Turn nextTurn = this.turns[nextI(i, this.turns.Count)];

                float r = getSmoothRadius2d(turn.radius, turn.angle, trackWidth);
                float mid = (turn.startS + turn.endS) / 2;
                float d1 = mid - prevTurn.endS;
                float d2 = nextTurn.startS - mid;
                float d = Mathf.Min(d1, d2);
                if (d > 0)
                    r = Mathf.Min(r, d * turn.angle * Mathf.Deg2Rad);
                //r = Mathf.Clamp(r, 0, turn.radius * 5);
                r = Mathf.Lerp(r, turn.radius, 0.3f);
                turn.smoothRadius = r;
            }
        }
        private float getSmoothRadius2d(float r, float angle, float trackWidth)
        {
            angle *= Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle / 2);
            float r1 = r - trackWidth / 2;
            float r2 = r + trackWidth / 2;
            float d = r2 - r1 * cos;
            float smoothR = d / (1 - cos);
            return smoothR;
        }
        public int nextI(int i, int count = -1)
        {
            if (count == -1)
                count = this.count;
            i++;
            if (i > count - 1)
                i -= count;
            return i;
        }
        public int prevI(int i, int count = -1)
        {
            if (count == -1)
                count = this.count;
            i--;
            if (i < 0)
                i += count;
            return i;
        }
        public static float clampS(float s, float length)
        {
            s %= length;
            if (s < 0)
                s += length;
            return s;
        }


        [Serializable]
        public class SPoint
        {
            public int seg;
            public float t;
            public float s;
            public SplineSegment ss;
            public float scalarRadius;
            public Vector3 radius;
            public SPoint()
            {
                seg = 0;
                t = 0;
                s = -1;
            }
            public SPoint(int seg, float t, SplineSegment ss, float s)
            {
                this.seg = seg;
                this.t = t;
                this.ss = ss;
                this.s = s;
            }
            /// <summary>
            /// <image src="radius.png" scale="3.5" />
            /// </summary>
            /// <param name="prevSP"></param>
            /// <param name="nextSP"></param>
            public void setCenterXZ(SPoint prevSP, SPoint nextSP)
            {
                Vector3 t1 = prevSP.getTangent();
                Vector3 t2 = nextSP.getTangent();
                t1.y = 0;
                t2.y = 0;
                t1.Normalize();
                t2.Normalize();
                Vector3 c = nextSP.getPos() - prevSP.getPos();
                Vector3 dir = new Vector3(c.z, 0, -c.x).normalized;
                float sign = Mathf.Sign(Vector3.Dot(t2 - t1, dir));
                if (sign == 0)
                    sign = Mathf.Sign(prevSP.scalarRadius);

                float cosC = Vector3.Dot(t1, t2);
                float sinSqr = Mathf.Max((1f - cosC) / 2, 0); // half angle formula
                float sinC2 = Mathf.Sqrt(sinSqr);
                if (sinC2 != 0)
                    scalarRadius = c.magnitude / 2 / sinC2;
                else
                    scalarRadius = 10000;
                scalarRadius *= sign;

                radius = dir * scalarRadius;
            }
            public Vector3 getPos()
            {
                return ss.GetPoint(t);
            }
            public Vector3 getTangent()
            {
                return ss.getDerivate1(t);
            }
        }
        public class SP1
        {
            public int seg;
            public float t;
            public float s;
            public float l;
            public SP1()
            {
                seg = 0;
                t = 0;
                s = 0;
                l = 0;
            }
            public SP1(int seg, float t, float s, float l)
            {
                this.seg = seg;
                this.t = t;
                this.s = s;
                this.l = l;
            }
        }
        [Serializable]
        public class Turn
        {
            public float s;
            public float startS;
            public float endS;
            public int leftRight;
            public Vector3 position;
            public Vector3 startPoint;
            public Vector3 endPoint;

            public float radius;
            public float signedRadius;
            public float smoothRadius;
            public float maxV;
            public float angle;
            public Turn()
            {

            }
            public Turn(Turn source)
            {
                s = source.s;
                startS = source.startS;
                endS = source.endS;
                leftRight = source.leftRight;
                position = source.position;
                startPoint = source.startPoint;
                endPoint = source.endPoint;

                radius = source.radius;
                smoothRadius = source.smoothRadius;
                signedRadius = source.signedRadius;
                maxV = source.maxV;
                angle = source.angle;
            }
            public Turn(Turn source1, Turn source2)
            {
                s = (source1.s + source2.s) / 2;
                startS = Mathf.Min(source1.startS, source2.startS);
                endS = Mathf.Max(source1.endS, source2.endS);
                leftRight = source1.leftRight;
                signedRadius = (source1.signedRadius + source2.signedRadius) / 2;

                radius = (source1.radius + source2.radius) / 2;
                smoothRadius = (source1.smoothRadius + source2.smoothRadius) / 2;
                signedRadius = (source1.signedRadius + source2.signedRadius) / 2;
                maxV = (source1.maxV + source1.maxV) / 2;
                angle = source1.angle;
            }

            public void updateVectors(SplineBase spline)
            {
                position = spline.getPoint(s);
                startPoint = spline.getPoint(startS);
                endPoint = spline.getPoint(endS);
                angle = Vector3.Angle(spline.getDerivate1(startS), spline.getDerivate1(endS));
            }
        }
    }
    [Serializable]
    public class Spline1 : SplineBase
    {
        public Spline1(int count, float radius, bool clockwise = false)
        {
            segments = new List<SplineSegment>();
            reset(count, radius, clockwise);
        }
        public void reset(int count, float radius, bool clockwise = false)
        {
            if (count < 3)
                count = 3;
            this.count = count;
            this.radius = radius;
            this.clockwise = clockwise;
            segments.Clear();
            float alpha = Mathf.PI * 2 / count;
            if (clockwise)
                alpha *= -1;

            for (int i = 0; i < count; i++)
            {
                float a0 = alpha * i;
                float a3 = alpha * nextI(i);
                Vector3 p0 = new Vector3(radius * Mathf.Cos(a0), 0, radius * Mathf.Sin(a0));
                Vector3 p3 = new Vector3(radius * Mathf.Cos(a3), 0, radius * Mathf.Sin(a3));

                segments.Add(new SplineSegment(p0, p3));
            }

            for (int i = 0; i < count; i++)
            {
                setTangent(i);
            }
        }
        public void setP0(Vector3 pos, int index)
        {
            SplineSegment s = segments[index];
            SplineSegment prev = segments[prevI(index)];
            Vector3 diff = pos - s.P0;

            s.P0 = pos;
            s.P1 += diff;
            prev.P3 = pos;
            prev.P2 += diff;

            setTangent(index);
            setTangent(prevI(index));
            setTangent(nextI(index));
        }
        public void setP1(Vector3 pos, int index)
        {
            SplineSegment s = segments[index];
            SplineSegment prev = segments[prevI(index)];
            s.P1 = pos;
            prev.P2 = prev.P3 + (s.P0 - s.P1);
        }
        public void setP2(Vector3 pos, int index)
        {
            SplineSegment s = segments[index];
            SplineSegment next = segments[nextI(index)];
            s.P2 = pos;
            next.P1 = next.P0 + (s.P3 - s.P2);
        }
        public void setTangent(int index)
        {
            SplineSegment s = segments[index];
            SplineSegment prev = segments[prevI(index)];
            Vector3 dir0 = (prev.P0 - s.P0).normalized;
            Vector3 dir3 = (s.P3 - s.P0).normalized;
            Vector3 bisector = (dir0 + dir3).normalized;
            Vector3 prevNext = (s.P3 - prev.P0).normalized;
            Vector3 normal = Vector3.Cross(bisector, prevNext);
            Vector3 dir = Vector3.Cross(normal, bisector).normalized;

            float m1 = (s.P3 - s.P0).magnitude / 3;
            float m2 = (prev.P0 - s.P0).magnitude / 3;

            s.P1 = s.P0 + m1 * dir;
            prev.P2 = prev.P3 - m2 * dir;
        }
        public void addSegment(int index)
        {
            if (index > segments.Count)
                return;
            SplineSegment s = segments[index];
            Vector3 p0 = s.GetPoint(0.5f);
            Vector3 p3 = s.P3;
            s.P3 = p0;
            SplineSegment sNew = new SplineSegment(p0, p3);
            segments.Insert(nextI(index), sNew);
            count = segments.Count;
            updateLength();
            updateTurns(radius / 3, 45, 10, true);
        }
        public void removeSegment(int index)
        {
            if (segments.Count < 4 || index > segments.Count - 1)
                return;
            int prev = prevI(index);
            segments[prev].P3 = segments[index].P3;
            segments[prev].P2 = segments[index].P2;
            segments.RemoveAt(index);
            count = segments.Count;
            updateLength();
            updateTurns(radius / 3, 45, 10, true);
        }
    }
    [Serializable]
    public class Spline2 : SplineBase
    {
        public List<Boor> boor;

        public Spline2(int count, float radius, float randomH, float randomV)
        {
            reset(count, radius, randomH, randomV);
        }
        public void reset(int count, float radius, float randomH, float randomV)
        {
            segments = new List<SplineSegment>();
            boor = new List<Boor>();
            turns = new List<Turn>();

            this.count = count;
            this.radius = radius;
            if (count < 3)
                count = 3;
            segments.Clear();
            boor.Clear();
            float alpha = Mathf.PI * 2 / count;
            float sin = Mathf.Sin(alpha / 2);
            float bRadius = radius / (1 - sin * sin * 2 / 3);

            for (int i = 0; i < count; i++)
            {
                float a = alpha * i;
                Vector3 b = new Vector3(bRadius * Mathf.Cos(a), 0, bRadius * Mathf.Sin(a));
                float hd = UnityEngine.Random.Range(-randomH, randomH);
                float vd = UnityEngine.Random.Range(0, randomV);
                Vector3 hDev = b.normalized * hd;
                Vector3 vDev = Vector3.up * vd;
                b += hDev + vDev;

                boor.Add(new Boor(b));
                segments.Add(new SplineSegment());
            }
            for (int i = 0; i < count; i++)
                updateSegment(i);

            update();
        }
        public void update(bool soft = false)
        {
            updateLength(soft);
            updateTurns(radius / 3, 45, 10, true);
        }
        public void createFromTurns(SplineBase spline, float width)
        {
            List<Boor> boor1 = new List<Boor>();

            radius = spline.radius;

            foreach (Turn turn in spline.turns)
            {
                Vector3 p1 = turn.startPoint;
                Vector3 p2 = turn.endPoint;
                spline.getLeftRight(turn.startS, out Vector3 l1, out Vector3 r1);
                spline.getLeftRight(turn.endS, out Vector3 l2, out Vector3 r2);
                r1 *= turn.leftRight;
                r2 *= turn.leftRight;
                p1 += r1 * width / 2 * turn.angle / 180; // outside of turn
                p2 += r2 * width / 2 * turn.angle / 180;

                Vector3 forward = Vector3.Cross(p1 - p2, Vector3.up).normalized;
                float angle = Mathf.Clamp(turn.angle, -90, 90) / 2 * Mathf.Deg2Rad;
                float toSpline = (p2 - p1).magnitude / 2 * (1f / Mathf.Sin(angle) - 1f / Mathf.Tan(angle));
                forward *= toSpline * turn.leftRight;
                p1 += forward;
                p2 += forward;

                Boor b1 = new Boor(p1);
                Boor b2 = new Boor(p2);
                b1.turn = true;
                boor1.Add(b1);
                boor1.Add(b2);
                segments.Add(new SplineSegment());
                segments.Add(new SplineSegment());
            }
            count = segments.Count;
            boor.Clear();
            for (int i = 0; i < boor1.Count; i++)
            {
                if (boor1[i].turn)
                {
                    int j1 = (i + 1) / 2;
                    int j2 = nextI(j1, spline.turns.Count);
                    Turn turn1 = spline.turns[j1];
                    Turn turn2 = spline.turns[j2];
                    int i1 = nextI(i, boor1.Count);
                    int i2 = nextI(i1, boor1.Count);
                    int i3 = nextI(i2, boor1.Count);
                    Boor b = boor1[i];
                    Boor b1 = boor1[i1];
                    Boor b2 = boor1[i2];
                    Boor b3 = boor1[i3];
                    boor.Add(b);
                    boor.Add(b1);
                    float turn1end = turn1.endS;
                    float turn2start = turn2.startS;
                    if (turn2start < -spline.length / 2)
                        turn2start += spline.length;
                    float l1 = (b1.p - b.p).magnitude;
                    float l2 = turn2start - turn1end + width;
                    if (l2 < -spline.length / 2)
                        l2 += spline.length;
                    float l3 = (b3.p - b2.p).magnitude;
                    float l = (l1 + l3) / 2;
                    float s1 = turn1end + l;
                    float s2 = turn1end + l * 2;
                    float s3 = turn2start - l * 2;
                    float s4 = turn2start - l;
                    float sMid = (turn2start + turn1end) / 2;

                    Vector3 p1 = spline.getPoint(s1);
                    Vector3 p2 = spline.getPoint(s2);
                    Vector3 p3 = spline.getPoint(s3);
                    Vector3 p4 = spline.getPoint(s4);
                    Vector3 pMid = spline.getPoint(sMid);
                    /*
                    spline.getLeftRight(s1, out Vector3 left1, out Vector3 right1);
                    spline.getLeftRight(s4, out Vector3 left4, out Vector3 right4);
                    right1 *= turn1.leftRight;
                    right4 *= turn2.leftRight;
                    p1 += right1 * width / 2;
                    p4 += right4 * width / 2;
                    */

                    if (l2 > l * 6)
                    {
                        s2 = Mathf.Max(s2, (sMid + s1 * 2) / 3);
                        s3 = Mathf.Min(s3, (sMid + s3 * 2) / 3);
                        p2 = spline.getPoint(s2);
                        p3 = spline.getPoint(s3);
                        Vector3 v14 = (p4 - p1);
                        v14.y = 0;
                        v14.Normalize();
                        Vector3 m2 = p1 + v14 * (s2 - s1);
                        Vector3 mM = p1 + v14 * (sMid - s1);
                        Vector3 m3 = p1 + v14 * (s3 - s1);
                        m2.y = p2.y;
                        mM.y = pMid.y;
                        m3.y = p3.y;
                        float d2 = Vector3.Cross(v14, p2 - m2).magnitude;
                        float dM = Vector3.Cross(v14, pMid - mM).magnitude;
                        float d3 = Vector3.Cross(v14, p3 - m3).magnitude;
                        boor.Add(new Boor(p1));
                        if (d2 > width / 10)
                            boor.Add(new Boor(p2));
                        if (dM > width / 10)
                            boor.Add(new Boor(pMid));
                        if (d3 > width / 10)
                            boor.Add(new Boor(p3));
                        boor.Add(new Boor(p4));
                    }
                    else if (l2 > l * 5)
                    {
                        boor.Add(new Boor(p1));
                        boor.Add(new Boor(p2));
                        boor.Add(new Boor(p3));
                        boor.Add(new Boor(p4));
                    }
                    else if (l2 > l * 4)
                    {
                        boor.Add(new Boor(p1));
                        boor.Add(new Boor(pMid));
                        boor.Add(new Boor(p4));
                    }
                    else if (l2 > l * 3)
                    {
                        boor.Add(new Boor(p1));
                        boor.Add(new Boor(p4));
                    }
                    else if (l2 > l * 2)
                    {
                        boor.Add(new Boor(pMid));
                    }
                    else if (l2 < l)
                    {
                        float l123 = (l1 + l2 + l3) / 3;
                        float d1 = l1 - Mathf.Clamp(l1, 0, l123);
                        float d3 = l3 - Mathf.Clamp(l3, 0, l123);
                        b.p += (b1.p - b.p).normalized * d1 / 2;
                        b1.p += (b.p - b1.p).normalized * d1 / 2;
                        b2.p += (b3.p - b2.p).normalized * d3 / 2;
                        b3.p += (b2.p - b3.p).normalized * d3 / 2;
                    }

                }
            }


            segments.Clear();
            foreach (Boor b in boor)
                segments.Add(new SplineSegment());
            count = segments.Count;
            for (int i = 0; i < segments.Count; i++)
                updateSegment(i);
            update();
        }
        public void fitToPeaks(SplineBase spline, SplineBase splineL, SplineBase splineR, float width, float peacksInOut)
        {
            updateLength();
            spline.updateLength();
            splineL.updateLength();
            splineR.updateLength();
            int index = 0;
            foreach (Boor b in boor)
            {
                if (b.turn)
                {
                    Boor bNext = boor[boor.IndexOf(b) + 1];
                    Turn turn = spline.turns[index];
                    Vector3 mid = spline.getPoint((turn.startS + turn.endS) / 2);
                    SplineBase innerSide, outerSide;
                    if (turn.leftRight == 1)
                    {
                        innerSide = splineL;
                        outerSide = splineR;
                    }
                    else
                    {
                        innerSide = splineR;
                        outerSide = splineL;
                    }
                    float s = turn.s / spline.length * innerSide.length;
                    Vector3 closest = spline.getClosestL(mid, out float cl);
                    Vector3 inClosest = innerSide.getClosestL(mid, out float cl1, s, width * 2);
                    Vector3 thisClosest = getClosestL(mid, out float cl2);
                    Vector3 outClosest = outerSide.getClosestL(mid, out float cl3, s, width * 2);
                    Vector3 target = Vector3.Lerp(inClosest, outClosest, peacksInOut);
                    Vector3 offset = target - thisClosest;
                    b.p += offset;
                    bNext.p += offset;
                    index++;
                }
                else
                {
                    int i = boor.IndexOf(b);
                    if (i > 0 && !boor[i - 1].turn)
                    {
                        Vector3 thisClosest = getClosestL(b.p, out float clThis);
                        float lastL = clThis * spline.length / length;
                        Vector3 closest = spline.getClosestL(thisClosest, out float cl1);
                        b.p += closest - thisClosest;
                    }
                }
            }
            for (int i = 0; i < count; i++)
                updateSegment(i);
            update();
        }
        public void setPos(Vector3 pos, int index)
        {
            boor[index].p = pos;
            int i0 = index;
            int i1 = prevI(i0);
            int i2 = prevI(i1);
            int i3 = prevI(i2);
            int i4 = prevI(i3);
            updateSegment(i0);
            updateSegment(i1);
            updateSegment(i2);
            updateSegment(i3);
            updateSegment(i4);
        }
        private void updateSegment(int index)
        {
            SplineSegment s = segments[index];
            int i0 = index;
            int i1 = nextI(i0);
            int i2 = nextI(i1);
            int i3 = nextI(i2);

            s.P0 = (boor[i0].p + 4 * boor[i1].p + boor[i2].p) / 6;
            s.P1 = (2 * boor[i1].p + boor[i2].p) / 3;
            s.P2 = (boor[i1].p + 2 * boor[i2].p) / 3;
            s.P3 = (boor[i1].p + 4 * boor[i2].p + boor[i3].p) / 6;

            s.updateLenght();
        }

        [Serializable]
        public class Boor
        {
            public Vector3 p;
            public bool turn;
            public Boor(Vector3 p)
            {
                this.p = p;
            }
        }
    }
/*
    [Serializable]
    public class SplineMesh
    {
        public SplineSegment[,] edges;
        public Vector2Int shape;
        public Vector2 size;
        public SplineMesh(Vector2Int shape, Vector2 size)
        {
            reset(shape, size);
        }
        public void reset(Vector2Int shape, Vector2 size)
        {
            this.shape = shape;
            this.size = size;
        }
    }
*/
}