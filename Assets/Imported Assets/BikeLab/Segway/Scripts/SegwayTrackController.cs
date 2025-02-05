using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VK.BikeLab;

namespace VK.BikeLab.Segway
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#52-segwaytrackcontroller")]
    public class SegwayTrackController : TrackController
    {
        public bool waitStart;

        private Segway segway;
        void Start()
        {
            segway = GetComponent<Segway>();
            segway.init();
            init(segway);
        }

        void Update()
        {
            update();
            setSteerToTarget();
            setVelosity();
        }
        private void FixedUpdate()
        {
            fixedUpdate();

            segway.updateCenterOfMass();

            if (startFlag)
                segwayStart();
            if (resetFlag)
                segwayReset();
        }
        private void setSteerToTarget()
        {
            Vector3 toTarget = target.transform.position - transform.position;
            Vector3 forward = transform.forward;
            toTarget.y = 0;
            forward.y = 0;
            toTarget.Normalize();
            forward.Normalize();
            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);
            float safeAngle = Mathf.Clamp(angle, -30, 30);
            segway.setSideIncline(safeAngle);
        }
        private void setVelosity()
        {
            float v = getSafeVelocity();
            v = Mathf.Min(v, segway.maxVelocity);

            float dToStop = distanceToRedLight - stopline;
            if (dToStop > 0)
            {
                float s = rb.velocity.sqrMagnitude / 2 / maxBrakeA;
                if (s >= dToStop - 2)
                    v = Mathf.Clamp((dToStop - s) * 10, -segway.maxVelocity, 0);
            }
            if (waitStart)
                v = 0;
            segway.setVelocity(v);
        }
        private void segwayStart()
        {
            startFlag = false;
            waitStart = false;
        }
        private void segwayReset()
        {
            resetFlag = false;
            segway.reset();
            waitStart = true;
        }
    }
}