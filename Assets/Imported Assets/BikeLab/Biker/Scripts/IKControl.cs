using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#21-ikcontrol")]
    [RequireComponent(typeof(Animator))]
    public class IKControl : MonoBehaviour
    {
        public enum Mode { Bike, Bicycle, Motoball }
        public enum ControlMode { LowLevel, HighLevel, None }
        public enum OnLeanAction { None, LeftRightMotion, FootDown, Both }
        [Space]
        public bool lookAt = true;
        public Transform target;
        public Transform ball;
        [Space]
        public bool accelerator = true;
        public bool Break = true;
        //public bool forwardBackwardMotion = true;
        public Transform leftHandHandle;
        public Transform rightHandHandle;
        public Transform leftFootHandle;
        public Transform rightFootHandle;
        [Space]
        public bool getOnFootpegs = true;
        public Rigidbody bikerRigidbody;
        public Rigidbody bikeRigidbody;
        [Space]
        public Mode mode;
        public Transform frame;
        public Transform leftPedalFootHandle;
        public Transform rightPedalFootHandle;
        public Transform leftPedalVisualModel;
        public Transform rightPedalVisualModel;
        public float shiftUpAcc = 7;
        public float shiftHeight = 0.18f;
        [Space]
        public OnLeanAction onLeanAction;
        [Range(0, 1)]
        public float spineToSteer;
        public float hipsRotationX;
        public float spineRotationX;
        [Header("IK hints")]
        public Transform lElbow;
        public Transform rElbow;
        public Transform lKnee;
        public Transform rKnee;
        [Space]
        //public ControlMode controlMode;
        public InputData inputData;

        [HideInInspector]
        public float currentLean;

        private Animator animator;

        private Vector3 startingPosition;
        private Quaternion hipsL;
        private Quaternion hipsR;
        private Quaternion hipsR1;
        private Quaternion spineL;
        private Quaternion spineR;
        private Quaternion spineR1;
        private Quaternion chestL;
        private Quaternion chestR;
        private Quaternion chestR1;
        private Quaternion upperChestL;
        private Quaternion upperChestR;
        private Quaternion upperChestR1;
        private int spineCount;

        private ConfigurableJoint leftJoint;
        private ConfigurableJoint rightJoint;
        private ConfigurableJoint bikerJoint;

        private Vector3 hipsOffset;
        private Vector3 hipsRotation;
        private Vector3 spineRotation;

        private Vector3 shiftOffset;
        private Vector3 leftRightOffset;
        private float shiftTime;
        private Vector3 shiftDir;
        private float kneeWhait;
        private bool pedalShiftUp;
        private float frameRot = 0;

        private float currentStoop;

        //private Foot leftFoot;
        //private Foot rightFoot;

        private Transform lFootHandle; // new handle interpolated between footHandle and pedalHandle
        private Transform rFootHandle;
        private Transform lPedal;
        private Transform rPedal;
        private Transform lFootPedal; // new pedal attached to foot
        private Transform rFootPedal;
        private float leftPedalFactor;
        private float rightPedalFactor;

        private Bike bikeController;
        private Feet feet;
        private float motorAcceleration;
        private float brakeAcceleration;

        void Start()
        {
            animator = GetComponent<Animator>();

            //bikerRigidbody.inertiaTensor = new Vector3(0.04f, 0.03f, 0.01f);
            ConfigurableJoint[] joints = bikeRigidbody.GetComponents<ConfigurableJoint>();
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

            bikerRigidbody.centerOfMass = new Vector3(0, 0.5f, 0);
            bikerJoint = bikerRigidbody.GetComponent<ConfigurableJoint>();
            
            if (mode == Mode.Bicycle)
            {
                rPedal = rightPedalFootHandle.parent;
                lPedal = leftPedalFootHandle.parent;
                rFootPedal = addPedal(HumanBodyBones.RightFoot);
                lFootPedal = addPedal(HumanBodyBones.LeftFoot);
            }

            bikeController = bikeRigidbody.GetComponent<Bike>();
            feet = bikeRigidbody.GetComponent<Feet>();

            GameObject lGO = leftFootHandle.gameObject;
            GameObject rGO = rightFootHandle.gameObject;
            lFootHandle = Instantiate(lGO, lGO.transform.parent).transform;
            rFootHandle = Instantiate(rGO, rGO.transform.parent).transform;
            lFootHandle.parent = leftFootHandle.parent;
            rFootHandle.parent = rightFootHandle.parent;

            if (leftPedalVisualModel != null)
                leftPedalVisualModel.parent = leftPedalFootHandle.parent;
            if (rightPedalVisualModel != null)
                rightPedalVisualModel.parent = rightPedalFootHandle.parent;

            Collider collider1 = leftFootHandle.parent.GetComponent<Collider>();
            Collider collider2 = rightFootHandle.parent.GetComponent<Collider>();
            Physics.IgnoreCollision(collider1, collider2);

            startingPosition = transform.localPosition;
            shiftOffset = Vector3.zero;
            leftRightOffset = Vector3.zero;
            shiftDir = new Vector3(0, 3, 1).normalized;

            spineRotation.x = spineRotationX;
            currentStoop = spineRotationX;
            hipsRotation.x = hipsRotationX;

            spineCount = 0;
            Quaternion r1 = Quaternion.Inverse(transform.rotation);
            Transform tr = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (tr != null)
            {
                hipsL = tr.localRotation;
                hipsR = r1 * tr.parent.rotation;
                hipsR1 = Quaternion.Inverse(hipsR);
                spineCount++;
            }
            tr = animator.GetBoneTransform(HumanBodyBones.Spine);
            if (tr != null)
            {
                spineL = tr.localRotation;
                spineR = r1 * tr.parent.rotation;
                spineR1 = Quaternion.Inverse(spineR);
                spineCount++;
            }
            tr = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (tr != null)
            {
                chestL = tr.localRotation;
                chestR = r1 * tr.parent.rotation;
                chestR1 = Quaternion.Inverse(chestR);
                spineCount++;
            }
            tr = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (tr != null)
            {
                upperChestL = tr.localRotation;
                upperChestR = r1 * tr.parent.rotation;
                upperChestR1 = Quaternion.Inverse(upperChestR);
                spineCount++;
            }

        }
        private void Update()
        {
            updateIK();
        }
        private void FixedUpdate()
        {
        }
        private void updateIK()
        {
            currentLean = bikeController.getLean();
            motorAcceleration = bikeController.info.motorAcceleration;
            brakeAcceleration = bikeController.info.brakeAcceleration;

            float pedalShift = getPedalShift();
            setShift(pedalShift);

            if (onLeanAction == OnLeanAction.LeftRightMotion || onLeanAction == OnLeanAction.Both)
                setLeftRightMotion();
            else
                setSpineToSteer();
            if (accelerator)
                setAcc();
            if (Break)
                setBreak();

            bool footDown = onLeanAction == OnLeanAction.FootDown;
            if (onLeanAction == OnLeanAction.Both && bikeRigidbody.velocity.magnitude < 5)
                footDown = true;

            if (mode == Mode.Bicycle)
            {
                lPedal.rotation = lFootPedal.rotation;
                rPedal.rotation = rFootPedal.rotation;
                Foot.State stateL = feet.getStateL();
                Foot.State stateR = feet.getStateR();
                if (stateL == Foot.State.Locked || stateL == Foot.State.Locking)
                    leftPedalFactor += Time.fixedDeltaTime * 10;
                else
                    leftPedalFactor -= Time.fixedDeltaTime * 10;
                if (stateR == Foot.State.Locked || stateR == Foot.State.Locking)
                    rightPedalFactor += Time.fixedDeltaTime * 10;
                else
                    rightPedalFactor -= Time.fixedDeltaTime * 10;
                leftPedalFactor = Mathf.Clamp01(leftPedalFactor);
                rightPedalFactor = Mathf.Clamp01(rightPedalFactor);
                leftPedalFactor = Mathf.Clamp01(leftPedalFactor);
                rightPedalFactor = Mathf.Clamp01(rightPedalFactor);


                lFootHandle.position = Vector3.Lerp(leftFootHandle.position, leftPedalFootHandle.position, leftPedalFactor);
                rFootHandle.position = Vector3.Lerp(rightFootHandle.position, rightPedalFootHandle.position, rightPedalFactor);
                lFootHandle.rotation = Quaternion.Lerp(leftFootHandle.rotation, leftPedalFootHandle.rotation, leftPedalFactor);
                rFootHandle.rotation = Quaternion.Lerp(rightFootHandle.rotation, rightPedalFootHandle.rotation, rightPedalFactor);
            }
            if (mode == Mode.Motoball)
            {
                /*
                    lFootHandle.localPosition = Vector3.Lerp(
                        lFootHandle.localPosition,
                        leftFootHandle.localPosition + leftFoot.holdOffset, 0.1f);
                    rFootHandle.localPosition = Vector3.Lerp(
                        rFootHandle.localPosition,
                        rightFootHandle.localPosition + rightFoot.holdOffset, 0.1f);
                */
            }

            hipsOffset = shiftOffset + leftRightOffset;
            currentStoop = spineRotationX + shiftOffset.y * 150 / (spineCount - 1);
            spineRotation.x = currentStoop;

            transform.localPosition = startingPosition + hipsOffset + bikerRigidbody.transform.localPosition;
            if (frame != null)
            {
                Vector3 euler = frame.localEulerAngles;
                euler.z = frameRot;
                frame.localEulerAngles = euler;
            }
        }
        public void reset()
        {
            motorAcceleration = 0;
            brakeAcceleration = 0;
            pedalShiftUp = false;
            shiftOffset.x = 0;
        }
        public bool pedal()
        {
            if (mode != Mode.Bicycle)
                return false;
            if (feet == null)
                return false;
            Foot.State s0 = feet.getStateL();
            Foot.State s1 = feet.getStateR();
            bool p0 = s0 == Foot.State.Locked || s0 == Foot.State.Locking;
            bool p1 = s1 == Foot.State.Locked || s1 == Foot.State.Locking;
            return  p0 && p1;
        }
        /*
        public bool locked()
        {
            bool p0 = rightFoot.getState() == Foot.State.Locked;
            bool p1 = leftFoot.getState() == Foot.State.Locked;
            return p0 && p1;
        }
        */
        private void setShift(float pedalShift)
        {
            float shift = 0;
            if (inputData.shiftUp || pedalShiftUp)
            {
                shift = shiftHeight + pedalShift;
                shiftTime = Time.fixedTime;
            }
            else if (Time.fixedTime > shiftTime + 0.5f)
            {
                shift = 0;
            }
            /*
            if ((!rightFoot.locked() || !leftFoot.locked()) && !pedal())
                shift = 0;
            */
            Vector3 offset = shiftDir * shift;
            offset.z += Mathf.Clamp(motorAcceleration * 0.002f, -0.05f, 0.05f);
            shiftOffset = Vector3.Lerp(shiftOffset, offset, 0.1f);
        }
        private float getPedalShift()
        {
            pedalShiftUp = mode == Mode.Bicycle && getOnFootpegs && motorAcceleration > shiftUpAcc;
            float shift = 0;
            float r = 0;
            if (pedalShiftUp)
            {
                float rot = inputData.pedalsRotation * Mathf.Deg2Rad;
                shift = 0.05f * Mathf.Cos((rot - 0.3f) * 2);
                r = -Mathf.Sin(rot) * 6;// Mathf.Clamp(forwardAcceleration - 1.5f, 0, 10);
            }
            frameRot = Mathf.Lerp(frameRot, r, 0.1f);
            return shift;
        }
        private void setLeftRightMotion()
        {
            float threehold = 20;
            float excess = 0;
            if (inputData.targetLean > threehold)
                excess = inputData.targetLean - threehold;
            if (inputData.targetLean < -threehold)
                excess = inputData.targetLean + threehold;
            excess = Mathf.Clamp(excess, -35, 35);
            float a = excess * Mathf.Deg2Rad;
            float r = 0.2f;

            leftRightOffset.x = -r * Mathf.Sin(a);
            leftRightOffset.y = r * (Mathf.Cos(a) - 1);

            kneeWhait = Mathf.Clamp01(Mathf.Abs(leftRightOffset.x) * 10);

            float k = 2f / Mathf.Sqrt(spineCount);
            hipsRotation.x = hipsRotationX;
            hipsRotation.y = excess * k;
            hipsRotation.z = excess * k;
            spineRotation.y = -excess * k * 0.25f;
            //leftRightOffset.x = Mathf.Clamp(excess * 0.01f, -0.1f, 0.1f);
        }
        public void setSpineToSteer()
        {
            float a3 = inputData.steer * spineToSteer / (spineCount - 1);
            spineRotation.y = a3;
            spineRotation.z = a3 * 0.5f;
        }
        private void setAcc()
        {
            float acc = Mathf.Clamp(motorAcceleration, 0, 10);
            Vector3 accAxis = rightHandHandle.parent.localEulerAngles;
            accAxis.x = -acc * 6 + 30;
            rightHandHandle.parent.localEulerAngles = accAxis;
        }
        private void setBreak()
        {
            if (feet.getStateR() == Foot.State.Locked)
            {
                float breakA = Mathf.Clamp(brakeAcceleration, -10, 0);
                Vector3 e = rightJoint.targetRotation.eulerAngles;
                e.x = -breakA * 5;
                if (breakA != 0)
                { }
                rightJoint.angularXMotion = ConfigurableJointMotion.Limited;
                rightJoint.targetRotation = Quaternion.Euler(e);
            }
            else
            {
                rightJoint.targetRotation = Quaternion.identity;
            }

        }
        private Transform addPedal(HumanBodyBones bone)
        {
            GameObject go = new GameObject("Pedal");
            Transform pedal = go.transform;
            pedal.parent = animator.GetBoneTransform(bone);
            pedal.localPosition = Vector3.zero;

            Vector3 pos = pedal.position;
            pos.y = transform.position.y;
            pos += transform.forward * 0.05f;
            pedal.position = pos;

            Vector3 target = pedal.position + transform.forward;
            pedal.LookAt(target);
            return pedal;
        }

        void OnAnimatorIK()
        {
            Quaternion rot = hipsR1 * Quaternion.Euler(hipsRotation) * hipsR * hipsL;
            animator.SetBoneLocalRotation(HumanBodyBones.Hips, rot);

            Quaternion spineRot = Quaternion.Euler(spineRotation);
            rot = spineR1 * spineRot * spineR * spineL;
            animator.SetBoneLocalRotation(HumanBodyBones.Spine, rot);
            if (spineCount > 2)
            {
                rot = chestR1 * spineRot * chestR * chestL;
                animator.SetBoneLocalRotation(HumanBodyBones.Chest, rot);
            }
            if (spineCount > 3)
            {
                rot = upperChestR1 * spineRot * upperChestR * upperChestL;
                animator.SetBoneLocalRotation(HumanBodyBones.UpperChest, rot);
            }

            animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, kneeWhait);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, kneeWhait);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            if (inputData.waitStart)
            {
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1.0f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1.0f);
            }
            else
            {
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.4f);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.4f);
            }
            if (lookAt)
                animator.SetLookAtWeight(1);


            animator.SetIKHintPosition(AvatarIKHint.LeftKnee, lKnee.position);
            animator.SetIKHintPosition(AvatarIKHint.RightKnee, rKnee.position);
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, lElbow.position);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, rElbow.position);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHandle.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandHandle.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandHandle.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandHandle.rotation);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, lFootHandle.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, lFootHandle.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rFootHandle.position);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rFootHandle.rotation);

            if (lookAt)
                animator.SetLookAtPosition(target.position + transform.forward);

        }

        [System.Serializable]
        public class InputData
        {
            public float velocity;
            public float steer;
            public float targetLean;
            public bool waitStart;
            public float safeLean;
            public bool shiftUp;
            /// <summary>
            /// Radians
            /// </summary>
            public float pedalsRotation;
            public bool win;
        }
        /*
        public class Pedal
        {
            private Transform footHandle;
            private Transform pedalFootHandle;
            private Transform footPedal; // added to foot
            private Foot foot;
            private Transform footHandleA; // Additional handle
            private Transform pedal;
            
            private float pedalFactor;
            public Pedal(Transform footHandle, Transform pedalFootHandle, Transform footPedal, Foot foot, Transform footHandleA)
            {
                this.footHandle = footHandle;
                this.pedalFootHandle = pedalFootHandle;
                this.footPedal = footPedal;
                this.foot = foot;
                this.footHandleA = footHandleA;
                pedal = pedalFootHandle.parent;
            }
            public void update()
            {
                pedal.rotation = footPedal.rotation;

                Foot.State state = foot.getState();
                if (state == Foot.State.Locked || state == Foot.State.Locking)
                    pedalFactor += Time.fixedDeltaTime * 10;
                else
                    pedalFactor -= Time.fixedDeltaTime * 10;
                pedalFactor = Mathf.Clamp01(pedalFactor);

                footHandleA.position = Vector3.Lerp(footHandle.position, pedalFootHandle.position, pedalFactor);
                footHandleA.rotation = Quaternion.Lerp(footHandle.rotation, pedalFootHandle.rotation, pedalFactor);
            }
        }
        */
    }
}
