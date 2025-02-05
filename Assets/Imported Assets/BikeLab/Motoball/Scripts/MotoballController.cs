using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#41-motoballcontroller")]
    [RequireComponent(typeof(Bike))]
    public class MotoballController : MonoBehaviour
    {
        public enum Balltangent { ToBall, ToGoal, None }

        public Transform ball;
        public Transform goal;
        public Transform wall;
        public Transform target;
        public Balltangent ballTangent;
        [Range(0, 1)]
        public float zSpeedValue = 0.6f;
        [Range(0, 4)]
        public float zDumper = 1.6f;
        [Range(0, 10)]
        public float minGoalTangent = 0f;
        [Range(0, 10)]
        public float minBallTangent = 0f;

        private Bike bike;
        private IKControl iKControl;
        [HideInInspector]
        public  Camera123 camera123;
        private SplineSegment segment;
        private List<Transform> walls;
        private Rigidbody rb;
        private Rigidbody ballrb;
        private Feet feet;
        private Vector3 startPos;
        private Quaternion startRot;

        private float avzSmooth;
        private float targetSteer;

        private float farTime;
        [HideInInspector]
        public bool hold;
        [HideInInspector]
        public bool camRequest;

        private Vector3 goalL;
        private Vector3 goalR;
        void Start()
        {
            bike = GetComponent<Bike>();
            iKControl = GetComponentInChildren<IKControl>(false);
            camera123 = GetComponentInChildren<Camera123>(true);
            if (camera123 != null)
                camera123.init();
            segment = new SplineSegment();

            BoxCollider[] colliders = wall.GetComponentsInChildren<BoxCollider>();
            walls = new List<Transform>();
            foreach (BoxCollider c in colliders)
                if (c.size.y > 2)
                    walls.Add(c.transform);

            rb = GetComponent<Rigidbody>();
            ballrb = ball.GetComponent<Rigidbody>();
            feet = GetComponent<Feet>();

            startPos = transform.position;
            startRot = transform.rotation;

            camRequest = false;
            goalL = goal.position + goal.right * 3;
            goalR = goal.position - goal.right * 3;
        }

        void Update()
        {
        }
        private void FixedUpdate()
        {
            if ((ball.position - transform.position).magnitude > 1)
                farTime = Time.time;
            if (Time.time - farTime > 1)
            {
                if (!hold)
                    camRequest = true;
                hold = true;
            }
            else
                hold = false;
            
            

            updateSegment();
            target.position = getTarget(0.5f, 2);
            if (!iKControl.inputData.waitStart)
            {
                setSteer();
                setVelocity();
            }
            if (ball.position.y < 0 || ball.position.y > 10)
            {
                resetBall();
            }
            if (transform.position.y < -1 || transform.position.y > 10)
            {
                reset();
                start();
            }
        }
        public void updateSegment()
        {
            if (hold)
            {
                    updateSegmentToGoal();
            }
            else
            {
                updateSegmentBall();
            }
        }
        private void updateSegmentToGoal()
        {
            Vector3 p0, p1, p2, p3;
            float m;
            p0 = transform.position;
            p3 = goal.position;
            m = (p3 - p0).magnitude / 3;
            float t2 = Mathf.Max(m , minGoalTangent);
            p1 = p0 + transform.forward * m * 0.5f;
            Vector3 back = (p0 - p3).normalized;
            //p2 = p3 + (back + goal.forward).normalized * t2;
            p2 = p3 + goal.forward * t2;
            p1 += 20 * fromWalls(transform.position);
            segment.update(p0, p1, p2, p3);
        }
        private void updateSegmentBall()
        {
            Vector3 p0, p1, p2, p3;
            float m;
            p0 = transform.position;
            p3 = ball.position;
            m = (p3 - p0).magnitude / 3;
            float t2 = Mathf.Max(m, minBallTangent);
            p1 = p0 + transform.forward * m / 2;
            Vector3 back = (p0 - p3).normalized * t2;
            Vector3 toGoal = (goal.position - p3).normalized * t2;
            if (ballTangent == Balltangent.ToGoal)
                p2 = p3 - toGoal;
            else
                p2 = p3 + back;
            p1 += 20 * fromWalls(transform.position);
            segment.update(p0, p1, p2, p3);
        }
        public void start()
        {
            iKControl.inputData.waitStart = false;
            if (feet != null)
                feet.start();
        }
        public void reset()
        {
            transform.position = startPos;
            transform.rotation = startRot;
            foreach (Rigidbody r in GetComponentsInChildren<Rigidbody>())
            {
                r.velocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }
            foreach (SphereCollider c in GetComponentsInChildren<SphereCollider>())
                c.radius = 0.2f;
            foreach (WheelColliderInterpolator i in GetComponentsInChildren<WheelColliderInterpolator>())
                i.reset();
            iKControl.inputData.waitStart = true;
            if (feet != null)
                feet.reset();

            bike.frontCollider.steerAngle = 0;
            bike.rearCollider.motorTorque = 0;
            bike.frontCollider.brakeTorque = 1000;
            bike.rearCollider.brakeTorque = 1000;

            target.position = getTarget(1.5f, 5);

            iKControl.reset();

            resetBall();
        }
        private void resetBall()
        {
            ball.position = new Vector3(0, 5, 0);
            ballrb.velocity = Vector3.zero;
            ballrb.angularVelocity = Vector3.zero;
        }
        private void magicForce()
        {
            Vector3 toMe = transform.position - ball.position;
            Vector3 local = transform.InverseTransformVector(toMe);
            float d = toMe.magnitude;
            if (d < 1 && local.z < 0)
            {
                Vector3 f = toMe.normalized * 0.5f;// / (d + 1);
                ballrb.AddForce(f, ForceMode.Impulse);
            }
        }
        private Vector3 fromWalls(Vector3 pos)
        {
            Vector3 ret = Vector3.zero;
            foreach (Transform w in walls)
            {
                float d = Vector3.Project(w.position - pos, w.forward).magnitude;
                ret += w.forward / (d + 1);
            }
            return ret;
        }
        private void setSteer()
        {
            if (!hold && ballTangent == Balltangent.None)
                targetSteer = getSteerToBall();
            else
            {
                targetSteer = getSteerToTarget();
            }
            targetSteer += getDamper();
            float v = bike.getRigidbody().velocity.magnitude;
            if (v < 4)
                bike.setSteer(targetSteer);
            else
                bike.setSteerByLean(targetSteer);
        }
        private void setVelocity()
        {
            float r = segment.minRadius;
            float targetV = Mathf.Clamp(r * 1, 6, 20);
            float al = Vector3.SignedAngle(transform.forward, goalL - transform.position, Vector3.up);
            float ar = Vector3.SignedAngle(transform.forward, goalR - transform.position, Vector3.up);
            if (hold && al < -5 && ar > 5)
            {
                targetV *= 0.3f;
                ballrb.AddForce(transform.forward * 3 + Vector3.up, ForceMode.Impulse);
            }
            else
                magicForce();
            float v = bike.getRigidbody().velocity.magnitude + 0.1f;
            float diff = targetV - v;
            float a = diff * 10 / (Mathf.Abs(targetV) + 0.5f);
            a = Mathf.Clamp(a, -10, 10);

            if (a < 0)
            {
                bike.setAcceleration(0);
                bike.safeBrake(-a);
            }
            else
            {
                bike.releaseBrakes();
                bike.setAcceleration(a);
            }
        }
        private float getSteerToTarget()
        {
            Vector3 toTarget = transform.InverseTransformVector(target.position - transform.position);
            toTarget.y = 0;
            toTarget.Normalize();
            float steer = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            return steer;
        }
        private float getSteerToBall()
        {
            Vector3 toBall = transform.InverseTransformVector(ball.position - transform.position);
            toBall.y = 0;
            toBall.Normalize();
            float steer = Mathf.Atan2(toBall.x, toBall.z) * Mathf.Rad2Deg;
            return steer;
        }
        private Vector3 getTarget(float time, float offset)
        {
            float v = bike.getRigidbody().velocity.magnitude + 0.1f;
            float nextS = time * v + offset;
            Vector3 pos;
            if (nextS < segment.length)
            {
                pos = segment.GetPointL(nextS);
            }
            else
            {
                float l = nextS - segment.length;
                pos = segment.P3 + (segment.P3 - segment.P2).normalized * l;
            }
            pos.y = 0;
            return pos;
        }
        private float getDamper()
        {
            float avz = bike.transform.InverseTransformVector(rb.angularVelocity).z * Mathf.Rad2Deg;
            avzSmooth = Mathf.Lerp(avzSmooth, avz, 0.1f);
            float z = bike.getLean();
            z = Mathf.Clamp(z, -30, 30);
            z *= 0.9f * bike.centerOfMassY;
            float damper = Mathf.Lerp(avzSmooth * 0.3f, z, zSpeedValue);
            float v = rb.velocity.magnitude;
            float vFactor = v / 5;
            damper *= vFactor;
            //setScale(ind1, kDumper);
            return damper * zDumper;
        }
        public void draw()
        {
            drawSegment(segment, Color.red);
        }
        public void drawSegment(SplineSegment s, Color color)
        {
#if UNITY_EDITOR
            Vector3 p0 = s.P0;
            Vector3 p1 = s.P1;
            Vector3 p2 = s.P2;
            Vector3 p3 = s.P3;
            float size = HandleUtility.GetHandleSize(p0) * 0.05f;
            Handles.color = color;
            Handles.DrawSolidDisc(p0, Vector3.up, size);
            Handles.DrawSolidDisc(p1, Vector3.up, size);
            Handles.DrawSolidDisc(p2, Vector3.up, size);
            Handles.DrawSolidDisc(p3, Vector3.up, size);
            Handles.DrawLine(p0, p1, 2);
            Handles.DrawLine(p3, p2, 2);
            Handles.DrawBezier(p0, p3, p1, p2, color, null, 4);
#endif
        }
    }
}