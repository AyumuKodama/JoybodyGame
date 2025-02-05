using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#37-ivehicle")]
    public class TrackController : MonoBehaviour
    {
        public enum Blending { Add, Interpolate }

        public TrackSpline trackSpline;
        public GameObject closest1GO;
        public GameObject closest2GO;
        public GameObject target;
        public Text speedText;
        public Score score;
        public float maxVelocity = 100;
        [Range(0.1f, 1)]
        public float speed = 1;
        [Tooltip("Rudder interpolation between track tangent and turning radius\n0 - for the cross\n1 - for the road")]
        [Range(0, 1)]
        public float tangentRadius;
        public float targetTimeout = 1.5f;
        public float targetOffset = 5;
        public bool useSmoothRadius;
        public bool useConstantVelocity;
        public float constantVelocity;

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Camera123 camera123;
        [HideInInspector] public float t05;
        [HideInInspector] public float forwardAcceleration;
        [HideInInspector] public float steerIncrement;
        [HideInInspector] public float accIncrement;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool jumpPeak;
        [HideInInspector] public float distanceToRedLight;
        [HideInInspector] public float stopline;
        public DispatcherData dd;
        [HideInInspector] public float maxBrakeA;

        [HideInInspector] public bool startFlag;
        [HideInInspector] public bool resetFlag;

        private float trackWidth;
        /// <summary>
        /// Velocity limited by the turning radius.
        /// </summary>
        private float[] veloR;
        /// <summary>
        /// Velocity allows you to slow down before turning.
        /// </summary>
        private float[] veloNext;

        private IVehicle vehicle;
        private SplineBase spline;

        private float closest1;
        private float closest2;
        private float startXoffset;
        private Vector3 closest2P; //Track space
        private Vector3 closest1P;
        private Vector3 targetP;
        public SplineBase.Turn turn;
        private TrackSpline.Jump currentJump;
        private float stopTime;
        private float fpsCount;
        private float fpsTime;
        private float safeVelocity;

        public void init(IVehicle vehicle)
        {
            this.vehicle = vehicle;

            rb = this.vehicle.getRigidbody();

            camera123 = GetComponentInChildren<Camera123>(true);
            if (camera123 != null)
                camera123.init();

            if (trackSpline.GetComponent<TrackMesh>() == null)
                trackWidth = trackSpline.trackWidth;
            else
                trackWidth = trackSpline.trackWidth / 2;

            TrackSpline2 trackSpline2 = trackSpline.GetComponent<TrackSpline2>();
            if (trackSpline2 != null && trackSpline2.enabled)
                spline = trackSpline2.spline;
            else
                spline = trackSpline.spline;
            spline.updateLength();
            spline.updateTurns(trackSpline.settings.maxTurnRadius, trackSpline.settings.minTurnAngle, trackWidth, true);
            trackSpline.splineIn.updateLength();
            trackSpline.splineOut.updateLength();
            
            Vector3 pos = trackSpline.transform.InverseTransformPoint(transform.position);
            closest1P = trackSpline.spline.getClosestL(pos, out float cl1);
            closest1 = cl1;
            closest2P = spline.getClosestL(pos, out float cl2);
            closest2 = cl2;
            updateClosest(true);
            Vector3 localCl = transform.InverseTransformPoint(trackSpline.transform.TransformPoint(closest2P));
            startXoffset = -localCl.x;

            turn = spline.getTurn(closest1);

            initVelo();

            score.start(trackSpline.startTo.l);
            stopTime = Time.fixedTime;
        }
        private void initVelo()
        {
            maxBrakeA = vehicle.getMaxBrakeAcceleration() * 0.8f;
            //float step = spline1.minTurnLength / 3;
            //step = Mathf.Clamp(step, 0.5f, 20);
            float step = Mathf.Min(1, spline.minTurnLength);
            int n = Mathf.RoundToInt(spline.length / step);
            veloR = new float[n];
            veloNext = new float[n];

            float t1 = Time.realtimeSinceStartup;
            for (int i = 0; i < veloR.Length; i++)
            {
                if (i == 649)
                {

                }
                float s = (float)i / veloR.Length * spline.length;
                float r = spline.getCurvatureRadius2d(s);// + 20;
                if (useSmoothRadius)
                {
                    SplineBase.Turn turn = spline.getTurn(s);
                    if (spline != null && s > turn.startS && s < turn.endS)
                        r = Mathf.Max(r, turn.smoothRadius);
                }

                float steerFactor = 1;
                if (r < 10) //10
                    steerFactor = Mathf.Pow(r * 0.1f, 0.6f); // Pow(r * 0.1f, 0.6f);
                float v = vehicle.getMaxVelocity(r);
                veloR[i] = v * steerFactor * speed;
                if (float.IsNaN(veloR[i]))
                {

                }
            }

            float t2 = Time.realtimeSinceStartup;
            for (int i = 0; i < veloNext.Length; i++)
            {
                float s = (float)i / veloNext.Length * spline.length;
                float turnS = 0;
                if (spline.turns.Count > 0)
                {
                    turnS = spline.turns[0].s;
                }
                foreach (SplineBase.Turn turn in spline.turns)
                    if (turn.s >= s)
                    {
                        turnS = turn.s;
                        break;
                    }
                int turnI = Mathf.FloorToInt(turnS / spline.length * veloR.Length) - i + 2; //spsp

                if (turnI <= 0)
                    turnI += veloR.Length;

                float minV = 10000;
                for (int j = 1; j < turnI; j++)
                {
                    s = spline.length / veloR.Length * j;
                    int k = i + j;
                    if (k >= veloR.Length)
                        k -= veloR.Length;
                    float vNext = veloR[k];
                    float v = vNext + Mathf.Sqrt(2 * maxBrakeA * s);
                    minV = Mathf.Min(minV, v);
                }
                veloNext[i] = minV * 1.0f;
                /*
                s = spline.length / veloR.Length * turnI;
                int l = i + turnI;
                if (l >= veloR.Length)
                    l -= veloR.Length;
                veloNext[i] = veloR[l] + Mathf.Sqrt(2 * maxBrakeA * s);
                */
            }
            float t3 = Time.realtimeSinceStartup;
            //Debug.Log("veloR:" + (t2 - t1));
            //Debug.Log("veloNext:" + (t3 - t2));
        }

        public void update()
        {
            fpsCount++;
            float dt = Time.realtimeSinceStartup - fpsTime;
            if (dt > 0.1f)
            {
                fpsTime = Time.realtimeSinceStartup;
                fpsCount = 0;
            }
            if (speedText != null && camera123.isActiveAndEnabled && fpsCount == 0)
            {
                speedText.text = "v " + (rb.velocity.magnitude).ToString("0.0");
                speedText.text += "\na " + forwardAcceleration.ToString("0.0");
                //speedText.text += "\nFPS " + fps.ToString("0.0");
                speedText.text += "\nsv " + (safeVelocity).ToString("0.00");
            }

        }
        public void fixedUpdate()
        {
            updateClosest();
            closest1GO.transform.position = trackSpline.transform.TransformPoint(closest1P);
            if (closest2GO != null)
                closest2GO.transform.position = trackSpline.transform.TransformPoint(closest2P);


            Vector3 closestWorld = trackSpline.transform.TransformPoint(closest2P);
            float offset = targetOffset + (closestWorld - transform.position).magnitude;
            updateTarget(targetTimeout, offset);

            Rigidbody rb = vehicle.getRigidbody();
            score.update(closest2, rb.velocity.magnitude * 3.6f);

            currentJump = trackSpline.getJump(closest1);
            jump = currentJump != null;

            turn = spline.getTurn(closest2);

            if (dd.wait)
            {
                stopTime = Time.fixedTime;
            }
            if (transform.position.y < trackSpline.transform.position.y - 10)
                reset();
        }
        private void updateClosest(bool fromScratch = false)
        {
            Vector3 pos = trackSpline.transform.InverseTransformPoint(transform.position);
            if (fromScratch)
            {
                closest1P = trackSpline.spline.getClosestL(pos, out float cl1);
                closest2P = spline.getClosestL(pos, out float cl2);
                closest1 = cl1;
                closest2 = cl2;
            }
            else
            {
                closest2P = spline.getClosestL(pos, out float cl2, closest2);
                closest1P = trackSpline.spline.getClosestL(closest2P, out float cl1, closest1);
                closest1 = cl1;
                closest2 = cl2;
            }
        }
        private void updateTarget(float time, float offset)
        {
            float v = vehicle.getRigidbody().velocity.magnitude + 0.1f;

            float nextS = closest2 + time * v + offset;
            if (nextS > trackSpline.spline.length)
                nextS -= trackSpline.spline.length;

            if (turn != null)
            {
                float k = 1f / (1f + 1.2f / turn.radius); //5f / turn.radius
                float shortS = closest2 + (time * v + offset) * k;
                if (nextS > turn.startS || (closest2 > turn.startS && closest2 < turn.endS))
                {
                    nextS = Mathf.Max(turn.startS, shortS);
                }
            }

            targetP = spline.getPoint(nextS);
            target.transform.position = trackSpline.transform.TransformPoint(targetP);
        }

        public void start()
        {
            score.start(trackSpline.startTo.l);
            dd.wait = false;
            startFlag = true;
        }
        public void reset()
        {
            vehicle.reset();

            foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
            {
                r.velocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }
            foreach (WheelColliderInterpolator i in GetComponentsInChildren<WheelColliderInterpolator>())
                i.reset();

            score.reset();

            dd.wait = true;
            dd.fullL = 0;
            dd.zeroCounter = 0;

            updateClosest(true);
            updateTarget(targetTimeout, targetOffset);

            turn = spline.getTurn(closest2);

            resetFlag = true;
        }
        public TrackSpline.Jump getJump()
        {
            return currentJump;
        }
        public SplineBase.Turn getTurn()
        {
            return turn;
        }
        public float getRadiusSteer(float s)
        {
            float r = spline.getSignedRadius2d(s);
            float rSteer = vehicle.getRadiusSteer(r);
            return rSteer;
        }
        public float getTangentSteer(float s)
        {
            Vector3 tangent = trackSpline.transform.TransformVector(spline.getDerivate1(s));
            float steer = Vector3.SignedAngle(transform.forward, tangent, Vector3.up);
            return steer;
        }
        public float getTracktSteer(float s)
        {
            float tangentSteer = getTangentSteer(s);
            float rSteer = getRadiusSteer(s);
            float steer = Mathf.Lerp(tangentSteer, rSteer, tangentRadius);
            return steer;
        }
        public float getSteerToTarget()
        {
            Vector3 next = transform.InverseTransformPoint(trackSpline.transform.TransformPoint(targetP));

            float width = trackSpline.startTrackWidth;
            if (dd.fullL < trackSpline.startTo.l)
            {
                width = 1;
            }
            else if (dd.fullL < trackSpline.startTrack.l)
            {
                float t = (trackSpline.startTrack.l - dd.fullL) / (trackSpline.startTrack.l - trackSpline.startTo.l);
                width = Mathf.Lerp(width, 1, t);
            }
            float xOffset = startXoffset * width;
            next.x += xOffset;

            next.y = 0;
            next.Normalize();
            float sin = next.x;
            if (next.z < 0)
                sin = Mathf.Sign(sin);
            float steer = Mathf.Asin(sin) * Mathf.Rad2Deg;

            return steer;
        }
        public float getSafeVelocity()
        {
            Rigidbody rb = vehicle.getRigidbody();
            float sqrV = rb.velocity.sqrMagnitude;
            float s = sqrV / 2 / maxBrakeA;
            float s2 = closest2 + s;

            float vToTurn = 9999;
            float r;
            if (turn != null)
            {
                if (useSmoothRadius)
                    r = turn.smoothRadius;
                else
                    r = turn.radius;
                float d = turn.startS - closest2;
                if (d < 0)
                    d += spline.length;
                float sToTurn = Mathf.Max(d - s, 0);
                float vOnTutn = vehicle.getMaxVelocity(r) * speed;
                vToTurn = vOnTutn + Mathf.Sqrt(maxBrakeA * sToTurn * 2);
            }

            int i1 = Mathf.FloorToInt(closest2 / spline.length * veloR.Length);//spsp
            int i2 = Mathf.FloorToInt(s2 / spline.length * veloR.Length);//spsp
            i2 %= veloR.Length;

            float v = maxVelocity;
            //v = Mathf.Min(v, veloR[i2]);
            //v = Mathf.Min(v, veloNext[i2]);
            v = Mathf.Min(v, veloR[i1]);
            v = Mathf.Min(v, veloNext[i1]);
            v = Mathf.Min(v, vToTurn);
            if (float.IsNaN(v))
            {

            }

            Vector3 clGlobal = trackSpline.transform.TransformPoint(closest2P);
            float toRoad = (clGlobal - transform.position).magnitude;
            float signC = Mathf.Sign(transform.InverseTransformPoint(clGlobal).x);
            float signI = vehicle.getTurnDir();
            if (toRoad > trackWidth)
            {
                r = vehicle.getTurnRadius();
                float veloSteer = vehicle.getMaxVelocity(r);
                v = Mathf.Min(v, veloSteer * 0.7f);
                //v = Mathf.Clamp(v, 0, 10);
            }
            else if (signI == signC)
            {
                toRoad = Mathf.Clamp(toRoad - trackWidth / 2, 0, trackWidth);
                float k = 1f / (1 + toRoad * 0.1f);
                v *= k;
            }

            trackSpline.spline.getSegT(closest1, out int seg1);
            v = Mathf.Min(v, trackSpline.nodes[seg1].maxVelocity);

            if (useConstantVelocity)
                v = constantVelocity;
            v *= dd.currentSpeed;
            //v = blendVelocityWithManual(velocity); //to do
            Vector3 localV = rb.transform.InverseTransformVector(rb.velocity);
            float diff = v - localV.z;
            float a = diff * 10 / (Mathf.Abs(v) + 0.5f);
            a += accIncrement * 10;

            bool redLight = distanceToRedLight > 0 && distanceToRedLight - stopline < 10;
            if (a > 0.1f && !dd.wait && rb.velocity.magnitude < 0.1f && v > 1 && !redLight)
            {
                if (Time.fixedTime - stopTime > 1 && Time.fixedTime > 20)
                {
                    vehicle.getup(1, 180);
                    stopTime = Time.fixedTime;
                }
            }
            else
                stopTime = Time.fixedTime;
            safeVelocity = v;
            if (i1 > 850 && i1 < 860)
            {

            }
            return v;
        }
        public float getL()
        {
            return closest1;
        }
        public float getL2()
        {
            return closest2;
        }
        public void OnDrawGizmosSelected()
        {
            //drawVeloR();
        }
        private void drawVeloR()
        {
#if UNITY_EDITOR
            if (veloR == null)
                return;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            for (int i = 0; i < veloR.Length; i++)
            {
                float s = (float)i / veloR.Length * spline.length;
                Vector3 pos = spline.getPoint(s);
                Vector3 wPos = trackSpline.transform.TransformPoint(pos);
                string l = "N=" + i;
                l += "\n S=" + s;
                l += "\n VR=" + veloR[i];
                l += "\n VN=" + veloNext[i];
                Handles.Label(wPos, l, style);
            }
#endif
        }
        [System.Serializable]
        public class Score
        {
            public int lap;
            public float lapTime;
            public float time;
            public float speed;
            public float lapMaxSpeed;
            public float maxSpeed;
            public float maxSpeedS;
            private float startS;
            private float curS;
            private float startLapTime;
            public void start(float s)
            {
                startS = s - 0.01f;
                startS = Mathf.Max(startS, 0);
                startLapTime = Time.fixedTime;
                curS = startS + 1000;
                lap = 0;
            }
            public void reset()
            {
                start(startS + 0.01f);
                lapTime = 0;
                time = 0;
                speed = 0;
                lapMaxSpeed = 0;
                maxSpeed = 0;
                maxSpeedS = 0;
            }
            public void update(float s, float velocity)
            {
                time = Time.fixedTime;
                speed = velocity;
                if (velocity > maxSpeed)
                {
                    maxSpeed = velocity;
                    maxSpeedS = s;
                }
                lapMaxSpeed = Mathf.Max(maxSpeed, velocity);
                if (curS < startS && s >= startS)
                {
                    lap++;
                    lapMaxSpeed = 0;
                    lapTime = Time.fixedTime - startLapTime;
                    startLapTime = Time.fixedTime;
                }
                curS = s;
            }
        }
        [System.Serializable]
        public class DispatcherData
        {
            public bool wait = true;
            public float currentSpeed = 1;
            public float startingL;
            /// <summary>
            /// Track length from the beginning of the spline to the current position
            /// </summary>
            [Tooltip("Track length from the beginning of the spline to the current position")]
            public float l;
            public float fullL;
            /// <summary>
            /// Length of the track from the start to the current position
            /// </summary>
            [Tooltip("Length of the track from the start to the current position")]
            public float s;
            public float fullS;
            public int zeroCounter;
            public int lapCounter;
            public float distanceToRedLight;
            public float stopline;
        }
    }
}