using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class FrontForkVisualizer : MonoBehaviour
    {
        [Tooltip("front WheelCollider")]
        public WheelCollider frontCollider;
        [Tooltip("The upper part of the fork.")]
        public Transform fork;
        [Tooltip("The lower part of the fork. The moving part of the dumpers and the wheel axis are located here.")]
        public Transform axis;
        [Tooltip("Front wheel visual object.")]
        public Transform wheel;

        private Quaternion startingForkRotation;
        private Vector3 startingAxisPosition;
        void Start()
        {
            startingForkRotation = fork.localRotation;
            startingAxisPosition = axis.localPosition;
        }

        void Update()
        {
            fork.localRotation = startingForkRotation;
            fork.Rotate(Vector3.up, frontCollider.steerAngle, Space.Self);

            frontCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);

            axis.localPosition = startingAxisPosition;
            Vector3 offset = frontCollider.transform.InverseTransformVector(pos - axis.position);
            float cos = Vector3.Dot(frontCollider.transform.up, fork.up);
            axis.Translate(0, offset.y / cos, 0, Space.Self);

            wheel.localRotation = Quaternion.Inverse(frontCollider.transform.rotation) * rot;
            wheel.position = axis.position;

            // We need to turn the wheel a little more to simulate rolling during the wheelbase change.
            float targetPos = -frontCollider.suspensionDistance * frontCollider.suspensionSpring.targetPosition;
            float currentPos = frontCollider.transform.InverseTransformPoint(pos).y;
            float tan = Mathf.Sqrt(1 - cos * cos) / cos;
            float HorizontalOffset = (currentPos - targetPos) * tan;
            float rotAngle = -HorizontalOffset / frontCollider.radius * Mathf.Rad2Deg;
            wheel.Rotate(Vector3.right, rotAngle, Space.Self);

        }
    }
}