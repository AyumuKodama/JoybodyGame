using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [RequireComponent(typeof(Bike))]
    public class BikeTrackController : TrackController
    {
        public enum TurnPhase { Entry, Peak, Exit}

        [Header("BikeTrackController fields")]
        [Tooltip("Incline dumper interpolation between speed and value\nDefault value 0.5")]
        [Range(0, 1)]
        public float zSpeedValue = 0.85f;
        [Tooltip("Default value 1")]
        [Range(0, 4)]
        public float zDumper = 1;
        [Tooltip("Rudder interpolation between interpolated track direction and direction to target\nDefault value 0.3")]
        [Range(0, 1)]
        public float trackSteer;
        [Space()]
        [Tooltip("Limits acceleration when drifting.\n0 - speed limited\n1 - speed is not limited")]
        [Range(0, 1)]
        public float drift;

        private Bike bike;
        private BikeInput bikeInput;
        private IKControl iKControl;
        private Feet feet;
        
        private float steerAngle;
        private float steerTime;
        private TurnPhase turnPhase = TurnPhase.Entry;
        void Start()
        {
            bike = GetComponent<Bike>();
            bike.Init();
            base.init(bike);

            bike.frontCollider.ConfigureVehicleSubsteps(5, 5, 2); // (5, 5, 1) default

            feet = GetComponent<Feet>();
            iKControl = GetComponentInChildren<IKControl>();

            dd.wait = iKControl.inputData.waitStart;
        }

        // Update is called once per frame
        void Update()
        {
            update();
        }
        private void FixedUpdate()
        {
            fixedUpdate();
            
            setSteer();

            if (dd.wait)
            {
                bike.setAcceleration(0);
                bike.frontCollider.brakeTorque = 1000;
                bike.rearCollider.brakeTorque = 1000;
            }
            else
            {
                setVelo();
            }

            if (iKControl != null)
                updateIK();

            if (startFlag)
                bikeStart();
            if (resetFlag)
                bikeReset();
        }
        private void bikeStart()
        {
            startFlag = false;

            if (feet != null)
                feet.start();
        }
        private void bikeReset()
        {
            resetFlag = false;

            if (feet != null)
                feet.reset();
        }
        private void setSteer()
        {
            float dumper = getDumper() * zDumper;
            TrackSpline.Jump jump = getJump();
            float l2 = getL2();
            jumpPeak = false;
            if (jump != null)
            {
                dumper = 0;
                jumpPeak = l2 < jump.endS && l2 > jump.peakS;
            }
            float steerToTarget = getSteerToTarget() + dumper;

            float v = Mathf.Max(transform.InverseTransformVector(rb.velocity).z, 0);
            float time = 0.5f;
            float ds = v * time;
            float tSteer = getTracktSteer(l2 + ds);

            float steer = Mathf.Lerp(tSteer, steerToTarget, trackSteer);
            steer += steerIncrement;
            //steer = blendSteerWithManual(steer);

            if ((jump != null && Mathf.Abs(steerToTarget) < 30) || v < 0.5f)
                steer = 0;
            if (v > 4)
                bike.setSteerByLean(steer);
            else
                bike.setSteer(steer);

        }
        private float getDumper()
        {
            float avz = bike.transform.InverseTransformVector(rb.angularVelocity).z * Mathf.Rad2Deg;
            float v = Mathf.Clamp(rb.velocity.magnitude, 30, 30);
            float z = bike.getLean();
            z = Mathf.Clamp(z, -30, 30);
            //z = Mathf.Sqrt(Mathf.Abs(z / 30)) * 30 * Mathf.Sign(z);
            avz *= (v) * 0.01f;
            z *= 0.9f * bike.centerOfMassY;
            float dumper = Mathf.Lerp(avz, z, zSpeedValue);
            //setScale(ind1, kDumper);
            return dumper * zDumper;
        }
        private void setVelo()
        {
            float velocity = getSafeVelocity();

            if (useConstantVelocity)
                velocity = constantVelocity;
            velocity *= dd.currentSpeed;
            //velocity = blendVelocityWithManual(velocity);

            Vector3 localV = rb.transform.InverseTransformVector(rb.velocity);
            float diff = velocity - localV.z;
            float a = diff * 10 / (Mathf.Abs(velocity) + 0.5f);
            a += accIncrement * 10;


            float dToStop = distanceToRedLight - stopline;
            if (dToStop > 0)
            {
                float s = rb.velocity.sqrMagnitude / 2 / maxBrakeA;
                if (s >= dToStop - 2)
                {
                    a = -rb.velocity.sqrMagnitude / 2 / dToStop;
                }
            }

            if (a < 0)
            {
                a *= 2;
                a = Mathf.Clamp(a, -maxBrakeA * 2, 0);
                bike.setAcceleration(0);
                bike.safeBrake(-a);
            }
            else
            {
                float excess = Mathf.Abs(bike.getLean()) - bike.info.safeLean;
                excess = Mathf.Clamp(excess + 2, 0, 60); // * (1 - drift);
                float limit = maxBrakeA / 0.8f / (excess + 1);
                limit = Mathf.Clamp(limit, maxBrakeA / (0.5f + localV.z), limit);
                a = Mathf.Clamp(a, 0, limit);

                bike.releaseBrakes();
                bike.setAcceleration(a);
            }
            forwardAcceleration = Mathf.Lerp(forwardAcceleration, a, 0.1f);

        }
        private void updateIK()
        {

            bool shiftUp = jump;

            iKControl.inputData.velocity = rb.velocity.magnitude;
            iKControl.inputData.steer = bike.getSmoothSteer();
            iKControl.inputData.targetLean = bike.getLean();
            iKControl.inputData.safeLean = bike.info.safeLean;
            iKControl.inputData.shiftUp = shiftUp;
            iKControl.inputData.waitStart = dd.wait;
        }
    }
}