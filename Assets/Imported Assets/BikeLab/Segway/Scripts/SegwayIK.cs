using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab.Segway
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#54-segwayik")]
    [RequireComponent(typeof(Animator))]
    public class SegwayIK : MonoBehaviour
    {
        public Transform leftHandHandle;
        public Transform rightHandHandle;
        public Transform leftFootHandle;
        public Transform rightFootHandle;
        public Transform frame1;
        public Transform frame2;
        public Segway segway;
        public float forwardInclineThreshold = 10;

        private Animator animator;
        private float smoothA;

        private Vector3 startingPosition;
        private float hipsStartingY;
        private Vector3 hipsOffset;
        private Quaternion hipsL;
        private Quaternion hipsR;
        private Quaternion hipsR1;
        private Quaternion spineL;
        private Quaternion spineR;
        private Quaternion spineR1;

        private Vector3 hipsRotation;
        private Vector3 spineRotation;
        void Start()
        {
            animator = GetComponent<Animator>();

            startingPosition = transform.localPosition;
            startingPosition.x = 0;
            hipsOffset = Vector3.zero;

            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform lFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            hipsStartingY = (hips.position - (lFoot.position + rFoot.position) / 2).y;

            Quaternion r1 = Quaternion.Inverse(transform.rotation);
            Transform tr = animator.GetBoneTransform(HumanBodyBones.Hips);
            hipsL = tr.localRotation;
            hipsR = r1 * tr.parent.rotation;
            hipsR1 = Quaternion.Inverse(hipsR);

            tr = animator.GetBoneTransform(HumanBodyBones.Spine);
            spineL = tr.localRotation;
            spineR = r1 * tr.parent.rotation;
            spineR1 = Quaternion.Inverse(spineR);

            hipsRotation = Vector3.zero;
            spineRotation = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void FixedUpdate()
        {
            forwardIncline();
            sideIncline();

            transform.localPosition = startingPosition + hipsOffset;
        }
        private void forwardIncline()
        {
            smoothA = Mathf.Lerp(smoothA, segway.forwardAcc, 0.05f);
            Vector3 euler = frame1.localEulerAngles;
            euler.x = smoothA * 2;
            frame1.localEulerAngles = euler;

            float excess = Mathf.Max(Mathf.Abs(euler.x) - forwardInclineThreshold, 0);
            hipsRotation.x = excess * 2;
            spineRotation.x = excess * 2;

            hipsOffset.z = -excess * 0.02f;
            if (smoothA > 0)
                hipsOffset.y = -excess * 0.01f;
            else
                hipsOffset.y = 0;
        }
        private void sideIncline()
        {
            float angle = segway.rbSideInkline * Mathf.Deg2Rad;
            float r = hipsStartingY * 1.0f; //0.8
            float x = Mathf.Sin(angle) * r;
            float y = (Mathf.Cos(angle) - 1) * r;
            hipsOffset.x = x;
            hipsOffset.y += y;

            Vector3 euler = frame2.eulerAngles;
            euler.z = -segway.rbSideInkline;
            frame2.eulerAngles = euler;

            hipsRotation.z = -segway.rbSideInkline * 0.5f;
        }
        void OnAnimatorIK()
        {
            Quaternion rot = hipsR1 * Quaternion.Euler(hipsRotation) * hipsR * hipsL;
            animator.SetBoneLocalRotation(HumanBodyBones.Hips, rot);

            rot = spineR1 * Quaternion.Euler(spineRotation) * spineR * spineL;
            animator.SetBoneLocalRotation(HumanBodyBones.Spine, rot);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandHandle.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandHandle.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandHandle.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandHandle.rotation);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootHandle.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootHandle.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootHandle.position);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootHandle.rotation);
        }
    }
}