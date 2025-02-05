using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class PhysicsSettings : MonoBehaviour
    {
        public float fixedDeltaTime = 0.005f;
        void Start()
        {
            //Physics.defaultSolverIterations = 60; // * 10
            //Physics.defaultSolverVelocityIterations = 10;
            Time.fixedDeltaTime = fixedDeltaTime;


        }
    }
}