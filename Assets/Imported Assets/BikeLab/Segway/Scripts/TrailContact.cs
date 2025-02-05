using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab.Segway
{
    public class TrailContact : MonoBehaviour
    {
        public TrailRenderer trail;
        private SphereCollider thisCollider;
        void Start()
        {
            thisCollider = GetComponent<SphereCollider>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnCollisionStay(Collision collision)
        {
            if (collision.contacts.Length > 0)
                if (collision.contacts[0].thisCollider == thisCollider)
                    trail.emitting = true;
        }
        private void OnCollisionExit(Collision collision)
        {
            trail.emitting = false;
        }
    }
}