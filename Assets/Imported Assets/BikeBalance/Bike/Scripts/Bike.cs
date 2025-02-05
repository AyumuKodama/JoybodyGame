using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    /// <summary>
    /// Bike allows you to control a bike, which consists of two WheelColliders and a RigidBody.
    /// </summary>
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#111-bike")]
    public class Bike : MonoBehaviour, IVehicle
    {
        /// <summary>
        /// Front WheelCollider
        /// </summary>
        [Tooltip("Front WheelCollider")]
        public WheelCollider frontCollider;
        /// <summary>
        /// Rear WheelCollider
        /// </summary>
        [Tooltip("Rear WheelCollider")]
        public WheelCollider rearCollider;
        /// <summary>
        /// Limits steering angle
        /// </summary>
        [Tooltip("Limits steering angle")]
        public float maxSteer = 25;
        /// <summary>
        /// Limits bike lean
        /// </summary>
        [Tooltip("Limits bike lean")]
        public float maxLean = 45;
        /// <summary>
        /// Determines the height of the center of mass above the ground. Lowering the center of mass makes the bike more stable.
        /// </summary>
        [Tooltip("Determines the height of the center of mass above the ground. Lowering the center of mass makes the bike more stable.")]
        public float centerOfMassY = 0.6f;
        /// <summary>
        /// Curves are generated automatically at runtime.
        /// </summary>
        [Tooltip("Curves are generated automatically at runtime.")]
        public Curves curves;
        /// <summary>
        /// These fields are generated automatically at runtime.
        /// </summary>
        [Tooltip("These variables are updated at runtime.")]
        public Info info;

        public Geometry geometry;

        private Wheel wheelFront;
        private Wheel wheelRear;
        private Rigidbody rb;
        private float mass;

        private Vector3 startingPosition;
        private Quaternion startingRotation;
        private Vector3 startingVelocity;
        private Vector3 startingAV;

        private float avzSmooth;
        private float deltaSmooth;
        private float steerSmooth;

        /// <summary>
        /// 1
        /// </summary>
        private float sidefriction0 = 1;
        /// <summary>
        /// 0.03
        /// </summary>
        private float sidefrictionVeloFactor = 0.03f;

        void Start()
        {
            Init();
        }
        void Update()
        {
            frontCollider.GetGroundHit(out WheelHit hit);
            info.frontWheelSidewaysSlip = hit.sidewaysSlip;
            info.frontWheelforwardSlip = hit.forwardSlip;
            rearCollider.GetGroundHit(out WheelHit hitR);
            info.rearWheelforwardSlip = hitR.forwardSlip;
            info.rpm1 = frontCollider.rpm;
            info.rpm2 = rearCollider.rpm;
        }
        private void FixedUpdate()
        {
            WheelFrictionCurve wfc;

            float f = getSidewaysFriction(rb.velocity.magnitude);
            //f = 1;
            wfc = frontCollider.sidewaysFriction;
            wfc.extremumValue = f;
            wfc.asymptoteValue = f * 0.5f;
            frontCollider.sidewaysFriction = wfc;
            rearCollider.sidewaysFriction = wfc;

            //rb.angularDrag = rb.velocity.magnitude * 0.2f; //0.2

            float a = rearCollider.motorTorque / rearCollider.radius / mass;
            info.motorAcceleration = Mathf.Lerp(info.motorAcceleration, a, 0.1f);
            a = -(frontCollider.brakeTorque / frontCollider.radius + rearCollider.brakeTorque / rearCollider.radius) / mass;
            info.brakeAcceleration = Mathf.Lerp(info.brakeAcceleration, a, 0.1f);

            if (rb.velocity.magnitude < 0.1f && transform.up.y < 0.3f)
            {
                getup();
            }

            steerSmooth = Mathf.Lerp(steerSmooth, frontCollider.steerAngle, 0.1f);
        }
        public void Init()
        {
            if (wheelFront != null)
                return;
            wheelFront = new Wheel(frontCollider);
            wheelRear = new Wheel(rearCollider);

            rb = GetComponent<Rigidbody>();
            mass = rb.mass + frontCollider.mass + rearCollider.mass;

            Vector3 centerOfMass = rb.centerOfMass;
            centerOfMass.y = centerOfMassY;
            centerOfMass.z = 0;
            rb.centerOfMass = centerOfMass;

            frontCollider.GetWorldPose(out Vector3 pos1, out Quaternion rot1);
            rearCollider.GetWorldPose(out Vector3 pos2, out Quaternion rot2);
            float wheelbase = (pos1 - pos2).magnitude;
            float h = rb.centerOfMass.y;
            Vector3 offset = rb.centerOfMass - transform.InverseTransformPoint(pos2);
            offset.y = 0;
            geometry = new Geometry(wheelbase, h, offset.magnitude, transform, maxSteer);

            float x = h / 2;
            float y = h / 2;
            float z = wheelbase / 2;
            x *= x;
            y *= y;
            z *= z;
            rb.inertiaTensor = new Vector3(y + z, x + z, x + y) * rb.mass / 2;

            startingPosition = transform.position;
            startingRotation = transform.rotation;
            startingVelocity = rb.velocity;
            startingAV = rb.angularVelocity;

            //float maxForce = -(rb.mass / 2 + rearCollider.mass) * Physics.gravity.y * 0.5f; // 0.3
            //maxTorque = maxForce * rearCollider.radius;

            sidefriction0 = 1;
            sidefrictionVeloFactor = 0.03f;

            //curves.log();
            curves.init();
            //curves.log();
        }
        public Rigidbody getRigidbody()
        {
            return rb;
        }
        /// <summary>
        /// The steer angle required to maintain balance at the current speed and the current lean.
        /// </summary>
        /// <returns>degrees</returns>
        public float GetBalanceSteer()
        {
            float v = transform.InverseTransformVector(rb.velocity).z;

            info.currentLean = getLean();
            info.balanceSteer = geometry.getSteer(info.currentLean, v);
            info.currentSteer = frontCollider.steerAngle;

            return info.balanceSteer;
        }
        /// <summary>
        /// Sets rear wheel torque acording to the given acceleration.
        /// </summary>
        /// <param name="value">Acceleration [-1, 1]</param>
        public void setAcceleration(float value)
        {
            float maxA = getMaxForwardAcceleration();
            value = Mathf.Clamp(value, -maxA, maxA);

            float rpm = (rb.velocity.magnitude + 1) * 30 / Mathf.PI / rearCollider.radius;
            float k = rearCollider.rpm / rpm * 0.75f;
            if (k > 1)
                value /= k;

            float force = value * mass;
            rearCollider.motorTorque = force * rearCollider.radius;
        }
        /// <summary>
        /// Sets front brake according to the given acceleration. Clamps acceleration to minimize slipping.
        /// </summary>
        /// <param name="acceleration"></param>
        public void frontBrake(float acceleration)
        {
            float maxA = getMaxForwardAcceleration();
            acceleration = Mathf.Clamp(acceleration, -maxA, maxA);
            float force = acceleration * mass;
            frontCollider.brakeTorque = force * rearCollider.radius;
        }
        /// <summary>
        /// Sets rear brake according to the given acceleration. Clamps acceleration to minimize slipping.
        /// </summary>
        /// <param name="acceleration"></param>
        public void rearBrake(float acceleration)
        {
            float maxA = getMaxForwardAcceleration();
            acceleration = Mathf.Clamp(acceleration, -maxA, maxA);
            float force = acceleration * mass;
            rearCollider.brakeTorque = force * rearCollider.radius;
        }
        /// <summary>
        /// Sets both brakes according to the given acceleration. Clamps the brakes to minimize slip and prevent rollovers.
        /// </summary>
        /// <param name="acceleration"></param>
        public void safeBrake(float acceleration)
        {
            float f = acceleration * mass;
            float g = -Physics.gravity.y;

            Vector3 hitPoint1 = wheelFront.getTargetHitPoint() + frontCollider.transform.localPosition;
            Vector3 hitPoint2 = wheelRear.getTargetHitPoint() + rearCollider.transform.localPosition;

            float h = rb.centerOfMass.y - (hitPoint1.y + hitPoint2.y) / 2;
            float wheelbase = hitPoint1.z - rb.centerOfMass.z;
            float k = h / wheelbase;
            float k1 = 1f + k;
            float k2 = 1f - k;
            if (Mathf.Abs(frontCollider.steerAngle) > 15 || Mathf.Abs(getLean()) > 30)
            {
                k1 = 0;
                k2 = 1;
            }

            float weight1 = (rb.mass / 2 + frontCollider.mass) * g * k1;
            float weight2 = (rb.mass / 2 + rearCollider.mass) * g * k2;
            float maxF1 = weight1 * frontCollider.forwardFriction.extremumValue * 1.2f; //0.5
            float maxf2 = weight2 * rearCollider.forwardFriction.extremumValue * 1.2f; // 0.65
            float f1 = Mathf.Clamp(f * k1 / 2, -maxF1, maxF1);
            float f2 = Mathf.Clamp(f * k2 / 2, -maxf2, maxf2);
            /*
            float kRpm = 10;
            if (frontCollider.rpm != 0)
                kRpm = rearCollider.rpm / frontCollider.rpm;
            if (kRpm > 1)
                rearCollider.motorTorque = -getMaxBrakeAcceleration() * (kRpm + 0.2f) * 20;
            else
                rearCollider.motorTorque = 0;
            */
            /*
            if (Mathf.Abs(frontCollider.steerAngle) < 15 && Mathf.Abs(getLean()) < 30)
                frontCollider.brakeTorque = f1 * frontCollider.radius;
            else
                frontCollider.brakeTorque = 0;
            */
            frontCollider.brakeTorque = f1 * frontCollider.radius;
            rearCollider.brakeTorque = f2 * rearCollider.radius;

            float a = (f1 + f2) / mass;

            if (!frontCollider.GetGroundHit(out WheelHit hit1))
                frontCollider.brakeTorque = 0;
            if (!rearCollider.GetGroundHit(out WheelHit hit2))
            {
                frontCollider.brakeTorque = 0;
                rearCollider.brakeTorque = 0;
            }
        }
        /// <summary>
        /// Releases both brakes.
        /// </summary>
        public void releaseBrakes()
        {
            frontCollider.brakeTorque = 0;
            rearCollider.brakeTorque = 0;
        }

        /// <summary>
        /// Sets steering angle to given.
        /// </summary>
        /// <param name="steer">degrees</param>
        public void setSteerDirectly(float steer)
        {
            frontCollider.steerAngle = steer;
            updateSafeLean();
        }
        /// <summary>
        /// Brings the bike closer to the desired lean by slightly off balance. This method is useful for high speed control.
        /// </summary>
        /// <param name="targetLean">degrees</param>
        public void setLean(float targetLean)
        {
            targetLean = Mathf.Clamp(targetLean, -maxLean, maxLean);

            updateSafeLean();
            targetLean = Mathf.Clamp(targetLean, -info.safeLean, info.safeLean);

            float newSteer = GetBalanceSteer();

            // Let's deviate a little from the balance position to get closer to the required lean.
            float delta = deltaSteer(targetLean);
            newSteer += delta;
            newSteer = Mathf.Clamp(newSteer, -maxSteer, maxSteer);

            setSteerDirectly(newSteer);
        }
        /// <summary>
        /// Brings the steer angle closer to the required steer by a small deviation from the balance steer.
        /// </summary>
        /// <param name="targetSteer">degrees</param>
        public void setSteer(float targetSteer)
        {
            info.targetSteer = targetSteer;

            targetSteer = Mathf.Clamp(targetSteer, -maxSteer, maxSteer);

            updateSafeLean();

            // A safe steer angle is a angle that does not require too much lean.
            float v = transform.InverseTransformVector(rb.velocity).z;
            float safeSteer = info.safeSteer * 0.8f;
            targetSteer = Mathf.Clamp(targetSteer, -safeSteer, safeSteer);

            // We need to steer in the opposite direction to the target.
            // This will lean the bike in the direction of the turn.
            float balanceSteer = GetBalanceSteer();
            float diff = targetSteer - balanceSteer;
            float delta = Mathf.Clamp(diff * 1.0f, -10, 10); // 1
            float newSteer = balanceSteer - delta;
            newSteer = Mathf.Clamp(newSteer, -maxSteer, maxSteer);
            newSteer += damper();// We need a damper to keep the bike from swing.
            if (v < 1)
                newSteer = Mathf.Lerp(newSteer, frontCollider.steerAngle, 0.9f);
            if (v < 0.1f)
                newSteer = 0;
            //newSteer = Mathf.Clamp(newSteer, frontCollider.steerAngle - 10, frontCollider.steerAngle + 10);
            setSteerDirectly(newSteer);
        }
        /// <summary>
        /// Brings the steer angle closer to the required steer by a small deviation from the balance steer. First calculate lean then call setLean.
        /// </summary>
        /// <param name="targetSteer"></param>
        public void setSteerByLean(float targetSteer)
        {
            info.targetSteer = targetSteer;
            targetSteer = Mathf.Clamp(targetSteer, -maxSteer, maxSteer);
            float v = transform.InverseTransformVector(rb.velocity).z;
            float targetLean = geometry.getLean(targetSteer, v);
            info.targetLean = targetLean;
            setLean(targetLean);
        }
        private void updateSafeLeanIld()
        {
            // A safe lean is a lean that does not cause slippage
            // and does not require too much steering.
            float friction = Mathf.Min(frontCollider.sidewaysFriction.extremumValue, rearCollider.sidewaysFriction.extremumValue);
            float safeLean1 = Mathf.Atan(friction) * Mathf.Rad2Deg;
            float v = Mathf.Abs(transform.InverseTransformVector(rb.velocity).z);
            float safeLean2 = Mathf.Abs(geometry.getLean(maxSteer * 1.0f, v)); // 0.7
            float safeK = 1.0f;// 0.6f;
            float v0 = 7;
            if (v > v0)
                safeK = Mathf.Lerp(1, safeK, v0 / v);
            //safeK = safeK + (1 - v0 / v) * 0.5f;
            /*
            if (frontCollider.GetGroundHit(out WheelHit hit))
            {
                float slip = Mathf.Abs(hit.sidewaysSlip / frontCollider.sidewaysFriction.extremumValue);
                safeK = 1 - slip;
            }
            */
            info.safeLean = Mathf.Min(safeLean1, safeLean2) * safeK;
            info.safeSteer = Mathf.Abs(geometry.getSteer(info.safeLean, v));
        }
        private void updateSafeLean()
        {
            // A safe lean is a lean that does not cause slippage
            // and does not require too much steering.
            float v = Mathf.Abs(transform.InverseTransformVector(rb.velocity).z);
            
            float friction = frontCollider.sidewaysFriction.extremumValue;
            float safeLean1 = Mathf.Atan(friction) * Mathf.Rad2Deg;
            float safeSteer = Mathf.Abs(geometry.getSteer(safeLean1, v));
            friction *= Mathf.Cos(safeSteer * Mathf.Deg2Rad);
            safeLean1 = Mathf.Atan(friction) * Mathf.Rad2Deg;
            safeSteer = Mathf.Abs(geometry.getSteer(safeLean1, v));

            float safeLean2 = Mathf.Abs(geometry.getLean(maxSteer * 1.0f, v)); // 0.7
            float safeK = 1.0f;// 0.6f;
            float v0 = 7;
            if (v > v0)
                safeK = Mathf.Lerp(1, safeK, v0 / v);
            //safeK = safeK + (1 - v0 / v) * 0.5f;
            /*
            if (frontCollider.GetGroundHit(out WheelHit hit))
            {
                float slip = Mathf.Abs(hit.sidewaysSlip / frontCollider.sidewaysFriction.extremumValue);
                safeK = 1 - slip;
            }
            */
            info.safeLean = Mathf.Min(safeLean1, safeLean2) * safeK;
            info.safeSteer = safeSteer;
        }
        /// <summary>
        /// BikeController changes SidewaysFriction depend on velocity. This method returns SidewaysFriction for the given speed.
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public float getSidewaysFriction(float velocity)
        {
            return sidefrictionVeloFactor * velocity + sidefriction0;
        }
        private float deltaSteer(float targetLean)
        {
            float o = transform.InverseTransformVector(rb.angularVelocity).z;
            avzSmooth = Mathf.Lerp(avzSmooth, o, 0.5f);
            o = avzSmooth;
            float dTh = (targetLean - getLean()) * Mathf.Deg2Rad;
            float v = Mathf.Clamp(rb.velocity.magnitude, 1, 100);
            float deltaO = -o * curves.dumping.Evaluate(v); // dumper
            float deltaTh = dTh * curves.theta.Evaluate(v);

            float t = Mathf.Clamp01(4 / (v + 0.1f));
            deltaSmooth = Mathf.Lerp(deltaSmooth, deltaTh, t);
            deltaTh = deltaSmooth;

            float delta = (deltaO + deltaTh) * Mathf.Rad2Deg;// 0.1 v=20;
            float maxDelta = 150 / (v + 10);
            delta = Mathf.Clamp(delta, -maxDelta, maxDelta);
            info.delta = delta;
            return delta;
        }
        public float damper()
        {
            Vector3 av = transform.InverseTransformVector(rb.angularVelocity);
            avzSmooth = Mathf.Lerp(avzSmooth, av.z, 0.1f);
            //avy = Mathf.Lerp(avy, av.y, 0.1f);
            float veloFactor = rb.velocity.magnitude;// Mathf.Pow(rb.velocity.magnitude, 0.5f);
            float damper = -avzSmooth * 0.4f * veloFactor; // 0.4
            damper = Mathf.Clamp(damper, -10, 10);
            return damper;
        }
        /// <summary>
        /// Returns current lean.
        /// </summary>
        /// <returns></returns>
        public float getLean()
        {
            float v = transform.localEulerAngles.z;
            if (v > 180)
                v -= 360;
            return v;
        }
        /// <summary>
        /// Returns max forward acceleration. Acceleration is limited by slipping and the possibility of rolling over.
        /// </summary>
        /// <returns></returns>
        public float getMaxForwardAcceleration()
        {
            float mm = (rb.mass / 2 + rearCollider.mass) / mass;
            float a = -Physics.gravity.y * mm * rearCollider.forwardFriction.extremumValue * 1.0f; //0.85
            float wheelbase = (frontCollider.transform.position - rearCollider.transform.position).magnitude;
            float h = rb.centerOfMass.y;
            float safeA = -Physics.gravity.y * wheelbase / 2 / h * 1.0f; //0.3
            a = Mathf.Min(a, safeA);
            return a;
        }
        /// <summary>
        /// Returns max brake acceleration. Acceleration is limited by slipping and the possibility of rolling over.
        /// </summary>
        /// <returns></returns>
        public float getMaxBrakeAcceleration()
        {
            float f1 = frontCollider.forwardFriction.extremumValue;
            float f2 = rearCollider.forwardFriction.extremumValue;
            float a = -Physics.gravity.y * (f1 + f2) / 2;
            return a * 0.5f;
        }
        /// <summary>
        /// Returns max sideways acceleration for the given velocity. Acceleration is limited by sideways friction.
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public float getMaxSidewaysAcceleration(float velocity)
        {
            float friction = getSidewaysFriction(velocity);
            float a = -Physics.gravity.y * friction;
            return a;
        }
        /// <summary>
        /// Returns the maximum velocity for a given turning radius.
        /// </summary>
        /// <param name="radius">turning radius</param>
        /// <returns></returns>
        public float getMaxVelocity(float radius)
        {
            if (sidefriction0 == 0)
                sidefriction0 = 1;
            if (sidefrictionVeloFactor == 0)
                sidefrictionVeloFactor = 0.03f;

            float f0 = sidefriction0;
            float k = sidefrictionVeloFactor;
            float r = radius;
            float g = -Physics.gravity.y;
            float k2 = k * k;
            float g2 = g * g;
            float r2 = r * r;

            // a = g(kv + f0)
            // a = v2 / r
            // =>
            // v2 - gkrv - gf0  = 0 
            float v = g * k * r / 2 + Mathf.Sqrt(g2 * k2 * r2 / 4 + g * f0 * r);

            float maxA = -Mathf.Tan(maxLean * Mathf.Deg2Rad) * Physics.gravity.y;
            float maxV = Mathf.Sqrt(maxA * r);
            v = Mathf.Min(v, maxV);

            return v;
        }
        public float getMotorA()
        {
            float a = rearCollider.motorTorque / rearCollider.radius / mass;
            return a;
        }
        public float getRadiusSteer(float radius)
        {
            return geometry.getSteer(radius);
        }
        public float getTurnRadius()
        {
            return geometry.getRadius(frontCollider.steerAngle);
        }
        public float getTurnDir()
        {
            return -Mathf.Sign(frontCollider.steerAngle);
        }
        /// <summary>
        /// Returns bike to the starting position.
        /// </summary>
        public void reset()
        {
            transform.position = startingPosition;
            transform.rotation = startingRotation;
            rb.velocity = startingVelocity;
            rb.angularVelocity = startingAV;

            frontCollider.steerAngle = 0;
            rearCollider.motorTorque = 0;
        }
        /// <summary>
        /// Getting bike up. Use if the bike falls over.
        /// </summary>
        /// <param name="h"></param>
        /// <param name="turn"></param>
        public void getup(float h = 0.1f, float turn = 0)
        {
            Vector3 pos = transform.position;
            pos.y += h;
            transform.position = pos;

            Vector3 e = transform.eulerAngles;
            e.x = 0;
            e.y += turn;
            e.z = 0;
            transform.eulerAngles = e;

            foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
            {
                r.velocity = transform.forward * 0;
                r.angularVelocity = Vector3.zero;
            }

            frontCollider.steerAngle = 0;
            rearCollider.motorTorque = 0;
        }
        /// <summary>
        /// Returns the midpoint between the front and back touch points.
        /// </summary>
        /// <returns></returns>
        public Vector3 getHitPoint()
        {
            Vector3 p1 = wheelFront.getHitPoint();
            Vector3 p2 = wheelRear.getHitPoint();
            //Debug.DrawLine(p1, p2, Color.green);
            return (p1 + p2) / 2;
        }
        public float getSmoothSteer()
        {
            return steerSmooth;
        }
        public class Wheel
        {
            private WheelCollider collider;
            public Vector3 getHitPoint()
            {
                if (collider.GetGroundHit(out WheelHit hit))
                {
                    return hit.point;
                }
                else
                    return collider.transform.position + collider.transform.TransformVector(getTargetHitPoint());
            }
            public Vector3 getTargetHitPoint()
            {
                float y = collider.radius + collider.suspensionDistance * collider.suspensionSpring.targetPosition;
                return new Vector3(0, -y, 0);
            }
            public Wheel(WheelCollider collider)
            {
                this.collider = collider;
            }
        }
        [Serializable]
        public class Geometry
        {
            private float wheelbase;
            private float cmHeight;
            private float cmOffset;
            private Transform bike;
            private float maxSteer;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="wheelbase">Wheelbase</param>
            /// <param name="cmHeight">Center of mass height</param>
            /// <param name="cmOffset">Center of mass horizontal offset relative rear wheel</param>
            public Geometry(float wheelbase, float cmHeight, float cmOffset, Transform bike, float maxSteer)
            {
                this.wheelbase = wheelbase;
                this.cmHeight = cmHeight;
                this.cmOffset = cmOffset;
                this.bike = bike;
                this.maxSteer = maxSteer;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="lean">Angle of lean</param>
            /// <param name="v">Linear velocity.</param>
            /// <returns></returns>
            public float getSteer(float lean, float v)
            {
                if (Mathf.Abs(lean) < 0.000001f)
                    return 0;
                lean *= Mathf.Deg2Rad;
                // first we calculate the acceleration needed to compensate for the lean of the bike
                float a = Physics.gravity.y * Mathf.Tan(lean);
                float r = v * v / a;   // required radius of rotation of the center of mass

                float d = Mathf.Abs(cmHeight * Mathf.Sin(lean)); // Horizontal displacement of the center of mass.
                float dR2 = r * r - cmOffset * cmOffset;
                if (dR2 <= 0)
                    return maxSteer * Mathf.Sign(-lean);
                float R = d + Mathf.Sqrt(r * r - cmOffset * cmOffset); // rear wheel radius of rotation
                float steer = Mathf.Atan2(wheelbase, R) * Mathf.Rad2Deg * Mathf.Sign(-lean);

                //draw(steer, R);
                return steer;
            }
            public float getLean(float steer, float v)
            {
                if (Mathf.Abs(steer) < 0.000001f)
                    return 0;

                // first iteration
                // As a result of the tilt of the bike, the center of mass shifts slightly
                // towards the direction of the turn. This leads to a decrease in the radius of rotation.
                // In the first iteration, we do not take into account the radius reduction.
                steer *= Mathf.Deg2Rad;
                float R = Mathf.Abs(wheelbase / Mathf.Tan(steer));
                float r = Mathf.Sqrt(R * R + cmOffset * cmOffset);
                float a = v * v / r;

                // second iteration
                // Now we can approximately calculate the horizontal displacement of the center of mass.
                float g = Physics.gravity.y;
                float d = cmHeight * a / Mathf.Sqrt(g * g + a * a);
                r = Mathf.Sqrt((R - d) * (R - d) + cmOffset * cmOffset);
                a = v * v / r;
                float lean = Mathf.Atan2(a, -Physics.gravity.y) * Mathf.Sign(-steer) * Mathf.Rad2Deg;

                return lean;
            }
            public float getSteer(float radius)
            {
                float steer = Mathf.Atan2(wheelbase, radius) * Mathf.Rad2Deg;
                if (steer > 90)
                    steer -= 180;
                return steer;
            }
            public float getRadius(float steer)
            {
                float tan = Mathf.Tan(Mathf.Abs(steer * Mathf.Deg2Rad));
                if (tan != 0)
                    return wheelbase / tan;
                else
                    return 10000;
            }
            public float getLeanRV(float r, float v)
            {
                float a = v * v / r;
                float g = -Physics.gravity.y;
                float lean = Mathf.Atan2(a, g) * Mathf.Rad2Deg;
                if (lean > 90)
                    lean -= 180;
                return lean;
            }
            private void draw(float steer, float R)
            {
                Vector3 frontPos = new Vector3(0, 0, cmOffset);
                Vector3 rearPos = new Vector3(0, 0, -cmOffset);
                Vector3 center = new Vector3(R, 0, -cmOffset) * Mathf.Sign(steer);

                Vector3 from = bike.TransformPoint(center);
                Vector3 to1 = bike.TransformPoint(frontPos);
                Vector3 to2 = bike.TransformPoint(rearPos);
                from.y = 0;

                Debug.DrawLine(from, to1);
                Debug.DrawLine(from, to2);
            }
        }
        [System.Serializable]
        public class Info
        {
            public float targetSteer;
            public float balanceSteer;
            public float currentSteer;
            public float safeSteer;
            public float currentLean;
            public float targetLean;
            public float safeLean;
            public float frontWheelSidewaysSlip;
            public float frontWheelforwardSlip;
            public float rearWheelforwardSlip;
            public float rpm1;
            public float rpm2;
            public float delta;
            public float motorAcceleration;
            public float brakeAcceleration;
        }
        [System.Serializable]
        public class Curves
        {
            public AnimationCurve dumping;
            public AnimationCurve theta;
            public void init()
            {
                float w = 1f / 3;
                Keyframe[] dumpingKeys = new Keyframe[3];
                Keyframe[] thetaKeys = new Keyframe[3];

                dumpingKeys[0] = new Keyframe(1, 0.05f, 0, 0, w, w);
                dumpingKeys[1] = new Keyframe(20, 0.05f, 0, 0, 0.14f, 0.44f);
                dumpingKeys[2] = new Keyframe(100, 0.3f, 0.003125f, 0.000694f, 0.092f, w);

                thetaKeys[0] = new Keyframe(1, 1, 0, -0.2367f, w, 0.119f);
                thetaKeys[1] = new Keyframe(20, 0.1f, 0, 0, 1, 0.03f);
                thetaKeys[2] = new Keyframe(100, 0.3f, 0.0025f, 0, w, w);

                dumpingKeys[0].weightedMode = WeightedMode.None;
                dumpingKeys[1].weightedMode = WeightedMode.None;
                dumpingKeys[2].weightedMode = WeightedMode.None;

                thetaKeys[0].weightedMode = WeightedMode.Out;
                thetaKeys[1].weightedMode = WeightedMode.In;
                thetaKeys[2].weightedMode = WeightedMode.None;

                dumping.keys = dumpingKeys;
                theta.keys = thetaKeys;
            }
            public void log()
            {
                logKey(dumping[0], "dumping[0]");
                logKey(dumping[1], "dumping[1]");
                logKey(dumping[2], "dumping[2]");
                logKey(theta[0], "theta[0]");
                logKey(theta[1], "theta[1]");
                logKey(theta[2], "theta[2]");
            }
            private void logKey(Keyframe key, string id)
            {
                string s = id + " ";
                s += "t=" + key.time;
                s += " v=" + key.value;
                s += " inT=" + key.inTangent;
                s += " outT=" + key.outTangent;
                s += " inW=" + key.inWeight;
                s += " outW=" + key.outWeight;
                s += " mode=" + key.weightedMode;
                Debug.Log(s);
            }
        }
    }
}