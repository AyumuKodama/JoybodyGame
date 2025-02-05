using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#112-feet")]
    public class Feet : MonoBehaviour
    {
        public Transform ball;
        public Vector3 footTargetPosition = new Vector3(0.25f, -0.02f, -0.1f);
        public bool footDown;
        public bool waitStart;
        public bool motoball;

        private Rigidbody bikeRigidbody;
        private ConfigurableJoint leftJoint;
        private ConfigurableJoint rightJoint;
        private Foot leftFoot;
        private Foot rightFoot;
        void Start()
        {
            bikeRigidbody = GetComponent<Rigidbody>();
            ConfigurableJoint[] joints = GetComponents<ConfigurableJoint>();
            if (joints[0].anchor.x < 0)
            {
                leftJoint = joints[0];
                rightJoint = joints[1];
            }
            else
            {
                leftJoint = joints[1];
                rightJoint = joints[0];
            }

            leftFoot = new Foot(leftJoint, bikeRigidbody, footTargetPosition, motoball, ball);
            rightFoot = new Foot(rightJoint, bikeRigidbody, footTargetPosition, motoball, ball);

            Collider collider1 = leftJoint.connectedBody.GetComponent<Collider>();
            Collider collider2 = rightJoint.connectedBody.GetComponent<Collider>();
            Physics.IgnoreCollision(collider1, collider2);
        }
        private void FixedUpdate()
        {
            leftFoot.update(waitStart, footDown);
            rightFoot.update(waitStart, footDown);
        }
        public void reset()
        {
            waitStart = true;
        }
        public void start()
        {
            waitStart = false;
        }
        public Foot.State getStateL()
        {
            return leftFoot.getState();
        }
        public Foot.State getStateR()
        {
            return rightFoot.getState();
        }
        public Foot getLeftFoot()
        {
            return leftFoot;
        }
        public Foot getRightFoot()
        {
            return rightFoot;
        }
    }
    [System.Serializable]
    public class Foot
    {
        public enum State { Locked, Locking, WeitStart, FootDown, StrongDown, Pedal }

        public Vector3 holdOffset = Vector3.zero;

        private ConfigurableJoint joint;
        private Rigidbody rbBike;
        private Vector3 footTargetPosition;
        private bool motoball;
        private Transform ball;

        private Bike bikeController;
        private Rigidbody rbFoot;
        private Rigidbody rbBall;
        private SphereCollider collider;
        private FootContact footContact;
        private TrailRenderer trail;
        private Vector3 startingPos;
        private float leftRight;

        private State state;

        public Transform pedal;
        public Transform footPedal;
        private float pedalFactor;

        private Vector3 targetPosition;
        private JointDrive drive;

        private float hysteresis;
        private float minSafeLean;
        public Foot(ConfigurableJoint joint, Rigidbody rbBike, Vector3 footTargetPosition, bool motoball, Transform ball)
        {
            this.joint = joint;
            this.rbBike = rbBike;
            this.footTargetPosition = footTargetPosition;
            this.motoball = motoball;
            this.ball = ball;

            bikeController = rbBike.GetComponent<Bike>();
            if (ball != null)
                rbBall = ball.GetComponent<Rigidbody>();
            //Vector3 ancor = joint.connectedBody.p
            joint.anchor = joint.transform.InverseTransformPoint(joint.connectedBody.position);
            rbFoot = joint.connectedBody;
            collider = rbFoot.GetComponent<SphereCollider>();
            footContact = rbFoot.GetComponent<FootContact>();
            trail = rbFoot.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
                trail.alignment = LineAlignment.TransformZ;
            targetPosition = new Vector3();
            drive = joint.yDrive;
            startingPos = rbFoot.transform.localPosition;
            leftRight = Mathf.Sign(-joint.anchor.x);

            footTargetPosition.x *= -leftRight;
            this.footTargetPosition = footTargetPosition - joint.anchor;
            float y = collider.center.y - collider.radius;
            this.footTargetPosition.y -= y;


        }
        public float update(bool weitStart, bool footDownMode)
        {
            state = State.Locked;
            if (weitStart)
                this.weitStart();
            else if (footDownMode)
                footDown();
            else
                lockMotion();

            if (motoball && (state == State.Locked || state == State.Locking || state == State.FootDown))
                hitBall();

            if (state == State.Pedal)
                pedalFactor += Time.fixedDeltaTime * 10;
            else
                pedalFactor -= Time.fixedDeltaTime * 10;
            pedalFactor = Mathf.Clamp01(pedalFactor);

            return pedalFactor;
        }
        public bool locked()
        {
            if (state == State.Locked)
            {
                return true;
            }
            return false;
        }
        public State getState()
        {
            return state;
        }

        private void lockMotion()
        {
            state = State.Locking;

            hysteresis = 0;
            rbFoot.mass = 0.1f;
            drive.positionSpring = 1000;
            drive.positionDamper = 100;

            targetPosition.x = 0;
            targetPosition.y = 0;
            targetPosition.z = 0;

            joint.targetPosition = targetPosition;
            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;

            if ((startingPos - rbFoot.transform.localPosition).magnitude < 0.01f)
            {
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                state = State.Locked;
            }
        }
        private void weitStart()
        {
            state = State.WeitStart;

            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            targetPosition.x = footTargetPosition.x;
            targetPosition.y = footTargetPosition.y;
            targetPosition.z = footTargetPosition.z;
            joint.targetPosition = targetPosition;

            if (footContact.collisionStay)
            {
                rbFoot.mass = 50; //20
                drive.positionSpring = 20000;//4000
                drive.positionDamper = drive.positionSpring / 5;

                joint.xDrive = drive;
                joint.yDrive = drive;
                joint.zDrive = drive;
            }
        }
        private void footDown()
        {
            float lean = bikeController.getLean() * leftRight;

            float v = Mathf.Clamp(rbBike.transform.InverseTransformVector(rbBike.velocity).z, 0, 100);
            float a = bikeController.getMotorA();
            float curSafeLean = Mathf.Clamp01(1 - v * 0.1f - a) * 12 + 3; // [3, 15]
            minSafeLean = Mathf.Lerp(minSafeLean, curSafeLean, 1.0f);
            float safeLean = bikeController.info.safeLean;
            safeLean = Mathf.Max(safeLean, minSafeLean);

            float excess = lean - safeLean;
            float advance = 0; //lowering the leg in advance
            bool getup = lean + hysteresis > 10 && v < 1; //5

            if (excess + advance + hysteresis > 0 || getup) // -3
            {
                if (leftRight == -1)
                { }
                state = State.FootDown;

                hysteresis = 5;
                joint.angularYMotion = ConfigurableJointMotion.Locked;

                float slip = 0;
                if (bikeController.frontCollider.GetGroundHit(out WheelHit hit))
                    slip = hit.sidewaysSlip * leftRight;
                slip = Mathf.Clamp01(slip / bikeController.frontCollider.sidewaysFriction.asymptoteSlip);
                if (slip > 0.7f)
                { }
                float dumper = Mathf.Clamp01(rbBike.angularVelocity.z * leftRight * 0.5f);
                float t = Mathf.Clamp01((excess + 1) * 0.1f + slip - dumper); // 0.1f
                if (t > 0.1f)
                    state = State.StrongDown;
                float v1 = Mathf.Clamp(v, 0.3f, 0.5f);
                float v2 = 0.5f - v1;
                Vector3 worldPoint = rbBike.worldCenterOfMass;
                worldPoint.y = rbBike.transform.position.y;
                Vector3 aside = Vector3.Cross(rbBike.transform.forward, Vector3.up) * leftRight;
                Vector3 down = -Vector3.up * 0.3f * (t - 0.15f); // -0.1f
                Vector3 backward = -rbBike.transform.forward * v2;
                worldPoint.y += collider.radius;
                worldPoint += aside.normalized * v1 + down + backward; //0.3
                Vector3 localPoint = rbBike.transform.InverseTransformPoint(worldPoint);
                targetPosition = localPoint - joint.anchor - collider.center;

                //targetPosition.y = Mathf.Clamp(targetPosition.y, -maxY, 1);
                joint.targetPosition = targetPosition;

                rbFoot.mass = 0.1f;
                drive.positionSpring = 1000;
                drive.positionDamper = 100;
                joint.xDrive = drive;
                joint.yDrive = drive;
                joint.zDrive = drive;
                joint.xMotion = ConfigurableJointMotion.Limited;
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.zMotion = ConfigurableJointMotion.Limited;

                if (footContact.collisionStay)
                {
#if ENABLE_INPUT_SYSTEM
                    if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed)
                        Debug.Log("excess = " + excess + "  t=" + t + "  slip=" + slip + " dumper=" + dumper);
#endif
                    //float vFactor = 10 / (rbBike.velocity.magnitude + 1) + 1;
                    float vFactor = 1;
                    if (rbBike.velocity.magnitude < 44)
                    {
                        vFactor = 5.0f;
                    }
                    float maxM = 10 * vFactor; //5
                    float maxF = 2000 * vFactor; //1000

                    rbFoot.mass = Mathf.Lerp(0.001f, maxM, t); //10
                    drive.positionSpring = Mathf.Lerp(0.2f, maxF, t);//2000
                    drive.positionDamper = drive.positionSpring / 10 / v1;

                    joint.xDrive = drive;
                    joint.yDrive = drive;
                    joint.zDrive = drive;
                }
            }
            else
            {
                if (leftRight == -1)
                { }
                lockMotion();
            }

            if (trail != null)
            {
                if (footContact.collisionStay)
                {
                    trail.transform.position = footContact.contactPoint.point + Vector3.up * 0.01f;
                    trail.transform.rotation = Quaternion.Euler(90, 0, 0);
                    trail.emitting = true;
                }
                else
                    trail.emitting = false;
            }
        }
        private void hitBall()
        {
            holdOffset.z = 0;
            Vector3 toBall = ball.position - rbBike.transform.position;
            if (toBall.magnitude > 3)
                return;
            toBall = rbBike.transform.InverseTransformVector(toBall);
            if (Mathf.Sign(toBall.x) == leftRight)
                return;
            holdOffset.z = collider.radius;

            float lean = bikeController.getLean() * leftRight;
            float a = 90 - lean;
            float y = (collider.radius + 0.1f) / Mathf.Tan(a / 2 * Mathf.Deg2Rad);
            float x = (collider.radius + 0.15f) * (-leftRight);
            float z = -0.2f;
            Vector3 pos = new Vector3(x, y, z);
            Vector3 targetPos = pos - joint.anchor - collider.center;
            joint.targetPosition = targetPos;
            rbFoot.mass = rbBall.mass * 10;
            drive.positionSpring = rbFoot.mass * 200;
            drive.positionDamper = drive.positionSpring / 5;

            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

            if (trail != null)
                trail.emitting = false;
        }
    }
}