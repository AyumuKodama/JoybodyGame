using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#132-swingarm")]
    public class Swingarm : MonoBehaviour
    {
        public enum ChainPitch { Pitch4, Pitch5, Pitch6 }

        public WheelCollider rearCollider;
        public Transform rearWheel;
        [Header("Visual model objects")]
        [Tooltip("Rear wheel visual object.")]
        public Transform rearWheelModel;
        [Tooltip("Swingarm visual object.")]
        public Transform swingarmModel;

        private Quaternion thisRotation;
        private Quaternion thisRotationI;

        private float startingAngle;
        private float length;

        private Vector3 pos;
        private Quaternion rot;
        private Chain chain;

        private void Start()
        {
            if (swingarmModel != null)
                swingarmModel.parent = transform;
            if (rearWheelModel != null)
                rearWheelModel.parent = rearWheel;

            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            transform.LookAt(pos);
            thisRotation = transform.localRotation;
            thisRotationI = Quaternion.Inverse(thisRotation);

            Vector3 toWheel = pos - transform.position;
            length = toWheel.magnitude;
            startingAngle = Vector3.SignedAngle(-transform.parent.forward, toWheel, transform.right);

            chain = GetComponentInChildren<Chain>();
        }
        void Update()
        {
            update2();
        }
        private void FixedUpdate()
        {
        }
        public void lookAtWheel()
        {
            rearCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            transform.LookAt(pos);
            rearWheel.SetPositionAndRotation(pos, rot);
        }
        private void update2()
        {
            WheelColliderInterpolator interpolator = rearCollider.GetComponent<WheelColliderInterpolator>();
            if (interpolator != null && interpolator.isActiveAndEnabled)
            {
                interpolator.GetWorldPose(out Vector3 wPos, out Quaternion wRot);
                pos = wPos;
                rot = wRot;
            }
            else
            {
                rearCollider.GetWorldPose(out Vector3 wPos, out Quaternion wRot);
                pos = wPos;
                rot = wRot;
            }

            Vector3 hitPoint = pos - rearCollider.transform.up * rearCollider.radius;
            float y = transform.parent.InverseTransformPoint(hitPoint).y - transform.localPosition.y + rearCollider.radius;
            float sin = Mathf.Clamp(-y / length, -1, 1);
            float forkAngle = Mathf.Asin(sin);
            //float z = length * Mathf.Cos(angle);
            forkAngle = forkAngle * Mathf.Rad2Deg - startingAngle;
            
            transform.localRotation = thisRotation;
            Vector3 pos1 = rearWheel.position;
            transform.RotateAround(transform.position, transform.right, forkAngle);
            Vector3 pos2 = rearWheel.position;

            Quaternion wheelRot = Quaternion.FromToRotation(rearCollider.transform.up, transform.parent.up) * rot;
            rearWheel.rotation = wheelRot;

            float dz = transform.parent.InverseTransformVector(pos2 - pos1).z;
            float da = dz / rearCollider.radius * Mathf.Rad2Deg;
            rearWheel.Rotate(Vector3.right, da, Space.Self);

            if (chain != null)
            {
                Vector3 forward = transform.TransformVector(thisRotationI * Vector3.forward);
                float wheelAngle = Vector3.SignedAngle(forward, rearWheel.forward, transform.right);
                chain.rotateChain(wheelAngle);
            }
        }
        /*
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos, 0.1f);
        }
        */
    }
}