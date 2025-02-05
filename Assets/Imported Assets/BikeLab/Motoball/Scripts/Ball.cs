using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class Ball : MonoBehaviour
    {
        public Transform wall;
        private List<Transform> walls;
        private Rigidbody rb;
        void Start()
        {
            BoxCollider[] colliders = wall.GetComponentsInChildren<BoxCollider>();
            walls = new List<Transform>();
            foreach (BoxCollider c in colliders)
                if (c.size.y > 2)
                    walls.Add(c.transform);
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector3 f = Vector3.zero;
            foreach (Transform w in walls)
            {
                float d = Vector3.Project(w.position - transform.position, w.forward).magnitude;
                f += w.forward / (d + 1);
            }
            rb.AddForce(f, ForceMode.Force);
        }
    }
}