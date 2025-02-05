using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using VK.BikeLab.Segway;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#36-trackdispatcher")]
    public class TrackDispatcher : MonoBehaviour
    {
        public TrackSpline spline;
        [Range(0.01f, 3)]
        public float timeScale = 1;
        public bool slowJump = false;
        [Range(0.05f, 1)]
        public float jumpTimeScale = 1;
        [Range(0, 20)]
        public float toCenter = 0;
        [Range(0, 0.5f)]
        public float randomSpeed = 0;
        [Range(0, 1)]
        public float avoidCollisions = 0;

        private Transform mean;
        private List<TrackController> controllers;

        private List<TrafficLight.Track> lights;

        private TrackController user;
        private int userIndex;
        private float updateTime;
        private bool slow = false;
        void Start()
        {
            Screen.SetResolution(1280, 720, false, 60);

            spline.updateSpline(true);

            controllers = new List<TrackController>();
            TrackController[] arr = FindObjectsOfType<TrackController>();
            foreach (TrackController c in arr)
                if (c.isActiveAndEnabled && c.trackSpline == spline)
                    controllers.Add(c);

            for (int i = 0; i < controllers.Count; i++)
            {
                TrackController c = controllers[i];
                Camera123 camera = c.GetComponentInChildren<Camera123>();
                if (camera != null && camera.isActiveAndEnabled)
                {
                    user = c;
                    userIndex = i;
                }
                Vector3 point = spline.transform.InverseTransformPoint(c.transform.position);
                spline.spline.getClosestL(point, out float l);
                c.dd.startingL = l;
                c.dd.l = l;
                c.dd.zeroCounter = 0;

                c.distanceToRedLight = -1;
                c.stopline = 0;
            }

            TrafficLight[] arrT = FindObjectsOfType<TrafficLight>();
            lights = new List<TrafficLight.Track>();
            foreach (TrafficLight s in arrT)
                foreach (TrafficLight.Track track in s.tracks)
                    if (track.track == spline)
                        lights.Add(track);

            updateTime = Time.time - 4;
        }

        void Update()
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                TrackController c = controllers[i];
                Camera123 camera = c.GetComponentInChildren<Camera123>();
                if (camera != null && camera.isActiveAndEnabled)
                {
                    user = c;
                    userIndex = i;
                }
            }

            bool sKey, rKey, pKey, tKey;
#if ENABLE_INPUT_SYSTEM
            sKey = Keyboard.current.sKey.wasPressedThisFrame;
            rKey = Keyboard.current.rKey.wasPressedThisFrame;
            pKey = Keyboard.current.pKey.wasPressedThisFrame;
            tKey = Keyboard.current.tKey.wasPressedThisFrame;
#else
            sKey = Input.GetKey(KeyCode.S);
            rKey = Input.GetKey(KeyCode.R);
            pKey = Input.GetKey(KeyCode.P);
            tKey = Input.GetKey(KeyCode.T);
#endif
            if (sKey)
            {
                start();
            }
            if (rKey)
            {
                reset();
            }
            if (pKey)
            {
                //swichBike();
            }
            if (tKey)
            {
                //setTimeScale();
            }
        }
        private void FixedUpdate()
        {
            foreach (TrackController c in controllers)
            {
                float l = c.getL();
                if (l < 10 && c.dd.l > spline.spline.length - 10)
                    c.dd.zeroCounter++;
                if (c.dd.l < 10 && l > spline.spline.length - 10)
                    c.dd.zeroCounter--;
                c.dd.l = l;
                c.dd.fullL = l + c.dd.zeroCounter * c.trackSpline.spline.length;

                float s = c.dd.l - c.dd.startingL;
                if (s < 10 && c.dd.s > spline.spline.length - 10)
                    c.dd.lapCounter++;
                if (c.dd.s < 10 && s > spline.spline.length - 10)
                    c.dd.lapCounter--;
                c.dd.s = s;
            }
            if (Time.time - updateTime > 5)
            {
                updateTime = Time.time;
                updateSpeed();
            }
            if (avoidCollisions > 0)
                bypass();
            updateRedLights();

        }
        public void start()
        {
            foreach (TrackController c in controllers)
            {
                c.start();
            }
        }
        public void reset()
        {
            foreach (TrackController c in controllers)
                c.reset();
        }
        private void updateSpeed()
        {
            if (controllers.Count == 0)
                return;
            float meanS = 0;
            float s;
            float ds;
            foreach (TrackController c in controllers)
            {
                s = c.dd.fullL;
                meanS += s;// + (c.score.lap - 1) * c.spline.spline.length;
            }
            meanS /= controllers.Count;

            float mean2 = 0;
            int count = 0;
            foreach (TrackController c in controllers)
            {
                s = c.dd.fullL;
                ds = meanS - s;
                if (ds < 100)
                {
                    mean2 += s;// + (c.score.lap - 1) * c.spline.spline.length;
                    count++;
                }
            }
            if (count != 0)
                mean2 /= count;

            float normS = mean2 / controllers[0].trackSpline.spline.length;
            normS -= Mathf.FloorToInt(normS);
            normS *= controllers[0].trackSpline.spline.length;

            if (mean != null)
                mean.position = spline.transform.TransformPoint(spline.spline.getPoint(normS));

            foreach (TrackController c in controllers)
            {
                //if (c == user)
                //    continue;
                float length = c.trackSpline.spline.length;
                ds = Mathf.Clamp(mean2 - c.dd.fullL, -0.5f * length, 0);
                c.dd.currentSpeed = 1 + ds / length * toCenter;

                float r = Random.Range(-randomSpeed * 1.5f, randomSpeed * 0.5f);
                c.dd.currentSpeed += r;
            }
        }
        private void bypass()
        {
            foreach (TrackController c in controllers)
            {
                c.steerIncrement = 0;
                foreach (TrackController other in controllers)
                {
                    if (other != c)
                    {
                        Vector3 p = c.transform.InverseTransformPoint(other.transform.position);
                        float x2 = p.x * p.x;
                        float z2 = p.z * p.z;
                        if (z2 < 100 && x2 < 4)
                        {
                            float fx = -Mathf.Exp(-x2);
                            float fz = -Mathf.Exp(-z2 * 0.05f);
                            float vFactor = 100f / (c.rb.velocity.magnitude + 1);
                            c.steerIncrement += fx * fz * avoidCollisions * vFactor * Mathf.Sign(-p.x);
                        }
                    }
                }
            }

        }
        private void updateRedLights()
        {
            foreach (TrackController trackController in controllers)
            {
                float s = trackController.getL();
                TrafficLight.Track minL = null;
                float minD = 100000;
                foreach (TrafficLight.Track light in lights)
                {
                    if (light.trafficColor == TrafficColor.Green)
                        continue;
                    float d = spline.spline.getFromToS(s, light.s);
                    if (d < minD)
                    {
                        minD = d;
                        minL = light;
                    }
                }
                if (minL != null)
                {
                    trackController.distanceToRedLight = minD;
                    trackController.stopline = minL.stopLine;
                }
                else
                {
                    trackController.distanceToRedLight = -1;
                    trackController.stopline = 0;
                }
            }
        }
        public void swichBike()
        {
            if (user == null)
                return;
            user.camera123.gameObject.SetActive(false);
            userIndex++;
            if (userIndex >= controllers.Count)
                userIndex = 0;
            user = controllers[userIndex];
            user.camera123.gameObject.SetActive(true);
        }
        public void setTimeScale()
        {
            slow = !slow;
            setInterpolation();
        }
        private void setInterpolation()
        {
            foreach (TrackController c in controllers)
            {
                Bike b = c.gameObject.GetComponent<Bike>();
                WheelColliderInterpolator fI = b.frontCollider.GetComponent<WheelColliderInterpolator>();
                WheelColliderInterpolator rI = b.rearCollider.GetComponent<WheelColliderInterpolator>();
                if (fI != null)
                {
                    fI.enabled = slow;
                    if (slow)
                        fI.reset();
                }
                if (rI != null)
                {
                    rI.enabled = slow;
                    if (slow)
                        rI.reset();
                }

                foreach (Rigidbody rb in b.GetComponentsInChildren<Rigidbody>())
                    if (slow)
                        rb.interpolation = RigidbodyInterpolation.Interpolate;
                    else
                        rb.interpolation = RigidbodyInterpolation.None;
            }
        }
    }
}