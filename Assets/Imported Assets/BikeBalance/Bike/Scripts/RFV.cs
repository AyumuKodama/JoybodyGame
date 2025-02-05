using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class RFV : MonoBehaviour
    {
        public WheelCollider rearCollider;
        [Tooltip("Rear wheel visual object.")]
        public Transform wheel;
        public Transform rearFork;

        private Vector3 startingPosition;
        private Quaternion startingRotation;
        Vector3 startingToWheel;
        private void Start()
        {
            startingPosition = rearFork.localPosition;
            startingRotation = rearFork.localRotation;

            startingToWheel = rearCollider.transform.InverseTransformVector(wheel.position - transform.position);
        }
        void Update()
        {
            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);

            wheel.rotation = rot;
            wheel.position = pos + (wheel.parent.up - rearCollider.transform.up) * rearCollider.radius;

            Vector3 toWheel = rearCollider.transform.InverseTransformVector(wheel.position - transform.position);
            float angle = Vector3.SignedAngle(startingToWheel, toWheel, Vector3.right);

            rearFork.localPosition = startingPosition;
            rearFork.localRotation = startingRotation;
            rearFork.RotateAround(transform.position, rearCollider.transform.right, angle);

        }
        private void FixedUpdate()
        {
        }
    }
}