using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#131-fork")]
    public class Fork : MonoBehaviour
    {
        [Tooltip("front WheelCollider")]
        public WheelCollider frontCollider;
        [Tooltip("The lower part of the fork. The moving part of the dampers and the wheel axis are located here.")]
        public Transform axis;
        [Tooltip("Front wheel visual object.")]
        public Transform frontWheel;

        [Header("Visual model objects")]
        public Transform frontForkModel;
        public Transform frontAxisModel;
        public Transform frontWheelModel;

        private Vector3 pos;
        private Quaternion rot;
        private float steer;
        private float steerSmooth;

        private Quaternion startingThisRotation;
        private Vector3 startingAxisPosition;
        private Vector3 restPos;
        private float sin;
        private float cos;
        void Start()
        {
            if (frontForkModel != null)
                frontForkModel.parent = transform;
            if (frontAxisModel != null)
                frontAxisModel.parent = axis;
            if (frontWheelModel != null)
                frontWheelModel.parent = frontWheel;

            startingThisRotation = transform.localRotation;
            startingAxisPosition = axis.localPosition;

            cos = Vector3.Dot(frontCollider.transform.up, transform.up);
            sin = Mathf.Sqrt(1 - cos * cos);
            frontCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            restPos = frontCollider.transform.InverseTransformPoint(pos);
        }

        void Update()
        {
            updateFork();
        }
        private void updateFork()
        {
            WheelColliderInterpolator interpolator = frontCollider.GetComponent<WheelColliderInterpolator>();
            if (interpolator != null && interpolator.isActiveAndEnabled)
            {
                interpolator.GetWorldPose(out Vector3 wPos, out Quaternion wRot);
                pos = wPos;
                rot = wRot;
                steer = interpolator.getSteer();
            }
            else
            {
                frontCollider.GetWorldPose(out Vector3 wPos, out Quaternion wRot);
                pos = wPos;
                rot = wRot;
                steer = frontCollider.steerAngle;
            }
            steerSmooth = Mathf.Lerp(steerSmooth, steer, 0.1f);

            float offset = (frontCollider.transform.InverseTransformPoint(pos) - restPos).y;

            transform.localRotation = startingThisRotation;
            Quaternion rot0 = transform.rotation;
            transform.Rotate(Vector3.up, steerSmooth, Space.Self);

            axis.localPosition = startingAxisPosition;
            axis.Translate(0, offset / cos, 0, transform);

            float angle = offset / frontCollider.radius * sin * Mathf.Rad2Deg;
            Quaternion r = Quaternion.AngleAxis(-angle, transform.right);
            frontWheel.rotation = r * rot0 * Quaternion.Inverse(frontCollider.transform.rotation) * rot;
            float yAngle = Vector3.SignedAngle(frontWheel.right, axis.right, axis.up);
            frontWheel.Rotate(axis.up, yAngle, Space.World);
        }
    }
}