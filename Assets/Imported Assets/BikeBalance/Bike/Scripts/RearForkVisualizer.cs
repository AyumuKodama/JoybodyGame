using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class RearForkVisualizer : MonoBehaviour
    {
        [Tooltip("Rear wheel visual object.")]
        public Transform wheel;
        void Update()
        {
            transform.LookAt(wheel, transform.parent.up);
        }
    }
}