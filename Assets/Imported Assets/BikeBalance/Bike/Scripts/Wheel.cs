using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#133-wheel")]
    public class Wheel : MonoBehaviour
    {
        public WheelCollider wheelCollider;
        public Transform wheelVisualModel;
        private TrailRenderer trail;
        private void Start()
        {
            if (wheelVisualModel != null)
                wheelVisualModel.parent = transform;

            trail = GetComponentInChildren<TrailRenderer>();
            if (trail != null)
                trail.alignment = LineAlignment.TransformZ;
        }
        private void FixedUpdate()
        {
            wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            transform.SetPositionAndRotation(pos, rot);

            if (trail != null)
            {
                trail.emitting = wheelCollider.GetGroundHit(out WheelHit hit);
                trail.transform.position = hit.point + Vector3.up * 0.01f;
                trail.transform.rotation = Quaternion.Euler(90, 0, 0);
                Debug.DrawLine(hit.point, hit.point + hit.normal * hit.force / 982);
            }
        }
    }
}