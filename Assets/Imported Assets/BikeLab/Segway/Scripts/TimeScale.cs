using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab.Segway
{
    public class TimeScale : MonoBehaviour
    {
        [Range(0.1f, 1)]
        public float timeScale = 1;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Time.timeScale = timeScale;
        }
    }
}