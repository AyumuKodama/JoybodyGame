using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [RequireComponent(typeof(WheelCollider))]
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#143-wheelcolliderinterpolator")]
    public class WheelColliderInterpolator : MonoBehaviour
    {
        [HideInInspector] public Vector3 posInterpolated;
        [HideInInspector] public Quaternion rotInterpolated;
        [HideInInspector] public float steerInterpolated;

        private WheelCollider wheelCollider;
        private Quaternion prevRot = Quaternion.identity;
        private Quaternion lastRot = Quaternion.identity;
        private Vector3 prevPos = Vector3.zero;
        private Vector3 lastPos = Vector3.zero;
        private float prevSteer;
        private float lastSteer;
        void Start()
        {
            wheelCollider = GetComponent<WheelCollider>();
            reset();
        }

        void Update()
        {
            GetWorldPose(out Vector3 pos, out Quaternion rot);
            posInterpolated = pos;
            rotInterpolated = rot;
            steerInterpolated = getSteer();
        }
        private void FixedUpdate()
        {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            prevPos = lastPos;
            prevRot = lastRot;
            prevSteer = lastSteer;
            lastPos = pos;
            lastRot = rot;
            lastSteer = wheelCollider.steerAngle;
        }
        public void GetWorldPose(out Vector3 pos, out Quaternion rot)
        {
            float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            pos = Vector3.Lerp(prevPos, lastPos, t);
            rot = Quaternion.Slerp(prevRot, lastRot, t);
        }
        public float getSteer()
        {
            float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            return Mathf.Lerp(prevSteer, lastSteer, t);
        }
        public void reset()
        {
            wheelCollider = GetComponent<WheelCollider>();
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            prevPos = pos;
            prevRot = rot;
            prevSteer = wheelCollider.steerAngle;
            lastPos = pos;
            lastRot = rot;
            lastSteer = wheelCollider.steerAngle;
            posInterpolated = pos;
            rotInterpolated = rot;
            steerInterpolated = wheelCollider.steerAngle;
        }
    }
}