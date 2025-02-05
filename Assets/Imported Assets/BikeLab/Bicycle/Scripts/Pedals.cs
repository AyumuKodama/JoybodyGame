using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#136-pedals")]
    public class Pedals : MonoBehaviour
    {
        public WheelCollider rearCollider;
        [Header("Visual model objects")]
        public Transform pedals;
        [Space]
        [Range(0.3f, 1)]
        public float minRps;
        [Range(1.1f, 3)]
        public float maxRps;

        private IKControl iKControl;
        private float ratio = 5;
        private float rotation;
        void Start()
        {
            if (pedals != null)
                pedals.parent = transform;
            ratio = 5;
            iKControl = rearCollider.gameObject.transform.parent.GetComponentInChildren<IKControl>(false);
        }

        // Update is called once per frame
        void Update()
        {
            rotation = rotate();
            iKControl.inputData.pedalsRotation = rotation;
        }
        private void FixedUpdate()
        {
        }
        private float rotate()
        {
            float rps = rearCollider.rpm / 60 / ratio;
            if (rps > 0.1f)
            {
                if (rps > maxRps)
                    ratio *= maxRps / minRps;
                if (rps < minRps)
                    ratio *= minRps / maxRps;
            }
            
            float step = rps * 360 * Time.deltaTime;
            float lean = iKControl.currentLean;
            if ((lean < -45 && (rotation < 180 - step && rotation > 0)) || 
                (lean > 45 && (rotation < -step)))
            {
                transform.RotateAround(transform.position, transform.right, step);
            }
            else if (rearCollider.GetGroundHit(out WheelHit hit) && 
                rearCollider.motorTorque > 0 && 
                iKControl.pedal() && 
                lean < 45 && lean > -45)
            {
                transform.RotateAround(transform.position, transform.right, step);
            }
            Vector3 from = transform.parent.up;
            Vector3 to = transform.up;
            Vector3 axis = transform.right;
            float angle = Vector3.SignedAngle(from, to, axis);
            return angle;
        }
    }
}