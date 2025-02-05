using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#141-slipeffects")]
    public class SlipEffects : MonoBehaviour
    {
        /// <summary>
        /// Splashes from under the rear wheel.
        /// </summary>
        [Tooltip("Splashes or smoke from under the rear wheel.")]
        public bool useParticle;
        /// <summary>
        /// Rear wheel slip threshold for splashing.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Wheel slip threshold for particles.")]
        public float minParticleSlip = 0.5f;
        [Header("Slip trail")]
        [Tooltip("Emit trail When slipping backward.")]
        public bool backward;
        [Tooltip("Emit trail When braking.")]
        public bool forward;
        [Tooltip("Emit trail When slipping sideways")]
        public bool sideways;
        /// <summary>
        /// Wheel slip threshold for the appearance of a trail.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Wheel slip threshold for the appearance of a trail.")]
        public float minTralSlip = 0.5f;

        private WheelCollider wheelCollider;
        private TrailRenderer trail;
        private ParticleSystem particle;
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.MainModule mainModule;
        private float rpmToV;
        void Start()
        {
            wheelCollider = GetComponent<WheelCollider>();

            trail = GetComponentInChildren<TrailRenderer>();
            if (trail != null)
                trail.alignment = LineAlignment.TransformZ;

            particle = GetComponentInChildren<ParticleSystem>();
            if (particle != null)
            {
                emissionModule = particle.emission;
                emissionModule.enabled = false;
                mainModule = particle.main;
            }
            rpmToV = Mathf.PI / 30 * wheelCollider.radius;
        }

        // Update is called once per frame
        void Update()
        {
            if (trail != null)
            {
                if (wheelCollider.GetGroundHit(out WheelHit hit))
                {
                    if (backward && (hit.forwardSlip >= wheelCollider.forwardFriction.extremumSlip * minTralSlip) ||
                         forward && (hit.forwardSlip <= -wheelCollider.forwardFriction.extremumSlip * minTralSlip) ||
                         sideways && (Mathf.Abs(hit.sidewaysSlip) >= wheelCollider.sidewaysFriction.extremumSlip * minTralSlip))
                        trail.emitting = true;
                    else
                        trail.emitting = false;
                    trail.transform.parent.position = hit.point + Vector3.up * 0.01f;
                    trail.transform.rotation = Quaternion.Euler(90, 0, 0);
                    if (particle != null && useParticle)
                    {
                        if (hit.forwardSlip >= wheelCollider.forwardFriction.extremumSlip * minParticleSlip)
                        {
                            emissionModule.enabled = true;
                            mainModule.startSpeed = wheelCollider.rpm * rpmToV;
                        }
                        else
                            emissionModule.enabled = false;
                    }
                }
                else
                {
                    trail.emitting = false;
                    if (particle != null)
                        emissionModule.enabled = false;
                }
            }

        }
    }
}