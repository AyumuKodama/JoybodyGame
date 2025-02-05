using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab.Segway
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#51-segway")]
    public class Segway : MonoBehaviour, IVehicle
    {
        public HingeJoint jointL;
        public HingeJoint jointR;
        public Transform centerOfMass;
        public Transform bodyCenterOfMass;

        private Rigidbody rbL;
        private Rigidbody rbR;
        private Rigidbody rb;
        private SphereCollider colliderL;
        private SphereCollider colliderR;
        private Transform trL;
        private Transform trR;
        private float maxAngularVelocity = 30;
        private Vector3 rbCenterOfMass;
        private Vector3 startPos;
        private Quaternion startRot;

        private float targetAcceleration;
        private float turnAV;
        [HideInInspector] public float targetIncline;
        [HideInInspector] public float forwardAcc;
        [HideInInspector] public float wheelbase;
        [HideInInspector] public float rbSideInkline;
        [HideInInspector] public float maxVelocity;
        private bool initialized;
        void Start()
        {
            init();
        }
        void Update()
        {
        }
        private void FixedUpdate()
        {
            bodyCenterOfMass.position = rb.worldCenterOfMass;

            updateCenterOfMass();
            rbR.WakeUp();
            updateMotors();

            if (transform.up.y < 0.2f)
                getup();
        }
        public void init()
        {
            if (initialized)
                return;
            initialized = true;

            rbL = jointL.GetComponent<Rigidbody>();
            rbR = jointR.GetComponent<Rigidbody>();
            rbL.maxAngularVelocity = maxAngularVelocity * 1.5f;
            rbR.maxAngularVelocity = maxAngularVelocity * 1.5f;

            rb = GetComponent<Rigidbody>();
            rbCenterOfMass = rb.centerOfMass;

            colliderL = jointL.GetComponent<SphereCollider>();
            colliderR = jointR.GetComponent<SphereCollider>();
            trL = colliderL.transform;
            trR = colliderR.transform;
            wheelbase = trR.localPosition.x - trL.localPosition.x;

            maxVelocity = maxAngularVelocity * colliderL.radius;

            startPos = transform.position;
            startRot = transform.rotation;

            Collider colliderRB = rb.GetComponent<Collider>();
            Physics.IgnoreCollision(colliderRB, colliderL);
            Physics.IgnoreCollision(colliderRB, colliderR);
        }
        public void updateCenterOfMass()
        {
            Vector3 rbC = rb.transform.TransformPoint(rb.centerOfMass);
            Vector3 rbLC = rbL.transform.TransformPoint(rbL.centerOfMass);
            Vector3 rbRC = rbR.transform.TransformPoint(rbL.centerOfMass);
            centerOfMass.position = (rbC * rb.mass + rbLC * rbL.mass + rbRC * rbR.mass) / (rb.mass + rbL.mass + rbR.mass);
        }
        public void setSideIncline(float incline)
        {
            targetIncline = incline;
            if (rb.velocity.magnitude > 1.0f)
                rbSideInkline = Mathf.Lerp(rbSideInkline, incline * 0.5f, 0.05f);
            else
                rbSideInkline = Mathf.Lerp(rbSideInkline, 0, 0.05f);
            float rad = rbSideInkline * Mathf.Deg2Rad;
            float x = rbCenterOfMass.y * Mathf.Sin(rad);
            float y = rbCenterOfMass.y * Mathf.Cos(rad);
            rb.centerOfMass = new Vector3(x, y, 0);
            updateCenterOfMass();

            float a = -Physics.gravity.y * Mathf.Tan(incline * Mathf.Deg2Rad);
            float omega = a / (rb.velocity.magnitude + 1.0f);
            float v = getVelosity();
            if (v < -0.5f)
                omega = -omega;
            float k = wheelbase / colliderL.radius;
            turnAV = omega * k * Mathf.Rad2Deg * 0.5f;
        }
        public void setVelocity(float targetV)
        {
            float v = getVelosity();
            float tv = targetV * Mathf.Cos(targetIncline * 2 * Mathf.Deg2Rad);
            float dv = tv - v;
            //float a = 2 * dv / (Mathf.Abs(targetV) + 1);
            targetAcceleration = Mathf.Lerp(targetAcceleration, dv, 0.05f);
        }
        public float getVelosity()
        {
            Vector3 localV = transform.InverseTransformVector(rb.velocity);
            float v = rb.velocity.magnitude * Mathf.Sign(localV.z);
            return v;
        }
        public Rigidbody getRigidbody()
        {
            return rb;
        }
        private void updateMotors()
        {
            float av = getAV();
            float minA = -10;
            float maxA = 10;
            if (av > 0)
                maxA = (maxAngularVelocity - av) * 0.5f;
            else
                minA = (-maxAngularVelocity - av) * 0.5f;
            float targetA = Mathf.Clamp(targetAcceleration, minA, maxA);

            float balanceA = getBalanceAcc(out float leftA, out float rightA);
            float deltaA = (targetA - balanceA) * 1.0f;
            forwardAcc = balanceA - deltaA;
            float aL = leftA - deltaA;
            float aR = rightA - deltaA;
            float targetVelocityL = av + aL;
            float targetVelocityR = av + aR;

            JointMotor motor = jointL.motor;
            motor.targetVelocity = targetVelocityL * Mathf.Rad2Deg + turnAV;
            jointL.motor = motor;

            motor = jointR.motor;
            motor.targetVelocity = targetVelocityR * Mathf.Rad2Deg - turnAV;
            jointR.motor = motor;
        }
        private float getBalanceAcc(out float leftA, out float rightA)
        {
            Vector3 p0 = (jointL.transform.position + jointR.transform.position) / 2 - colliderL.radius * Vector3.up;
            Vector3 toCenter = centerOfMass.position - p0;
            Vector3 dx = Vector3.Project(toCenter, transform.right);
            toCenter -= dx;
            Debug.DrawLine(p0, p0 + toCenter);

            Vector3 forward = transform.forward;
            forward.y = 0;
            float z = Vector3.Dot(toCenter, forward.normalized);
            float acc = 0;
            if (toCenter.y != 0)
                acc = -Physics.gravity.y * z / toCenter.y;

            Vector3 loсalDx = transform.InverseTransformVector(dx);
            float x0 = loсalDx.x;
            x0 *= 0.2f;
            float armL = -trL.localPosition.x + x0;
            float armR = trR.localPosition.x - x0;
            leftA = acc * armR / wheelbase * 2;
            rightA = acc * armL / wheelbase * 2;

            return acc;
        }
        private float getAV()
        {
            Vector3 velocity = transform.InverseTransformVector(rb.velocity);
            float v = velocity.magnitude * Mathf.Sign(velocity.z);
            float av = v / colliderL.radius;
            return av;
        }
        private void getup()
        {
            Vector3 pos = transform.position;
            pos.y = 0;
            transform.position = pos;

            transform.rotation = Quaternion.identity;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rbL.velocity = Vector3.zero;
            rbL.angularVelocity = Vector3.zero;

            rbR.velocity = Vector3.zero;
            rbR.angularVelocity = Vector3.zero;

            rbSideInkline = 0;
            targetAcceleration = 0;

            updateCenterOfMass();
        }

        public void reset()
        {
            getup();
            transform.position = startPos;
            transform.rotation = startRot;
            updateCenterOfMass();
        }
        public float getMaxForwardAcceleration()
        {
            return 1;
        }
        public float getMaxBrakeAcceleration()
        {
            return 1.0f;
        }
        public float getMaxSidewaysAcceleration(float velocity)
        {
            return 4;
        }
        public float getMaxVelocity(float radius)
        {
            float a = 4;
            float v = Mathf.Sqrt(a * radius);
            return v;
        }
        public float getRadiusSteer(float radius)
        {
            return Mathf.Rad2Deg / (radius + 1);
        }
        public float getTurnRadius()
        {
            float a = -Physics.gravity.y * Mathf.Tan(targetIncline * Mathf.Deg2Rad);
            float r = rb.velocity.sqrMagnitude / (Mathf.Abs(a) + 0.01f);
            return r;
        }
        public float getTurnDir()
        {
            return Mathf.Sign(targetIncline);
        }
        public void getup(float h, float turn)
        {
            getup();
        }
        
    }
}