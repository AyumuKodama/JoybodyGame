using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#22-footcontact")]
    public class FootContact : MonoBehaviour
    {
        public bool collisionStay;
        public ContactPoint contactPoint;
        void Start()
        {
            collisionStay = false;
        }
        private void OnCollisionExit(Collision collision)
        {
            collisionStay = false;
        }
        private void OnCollisionStay(Collision collision)
        {
            collisionStay = true;
            contactPoint = collision.GetContact(0);
        }
    }
}