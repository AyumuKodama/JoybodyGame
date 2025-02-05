using System.Collections;
using System.Collections.Generic;
//using System.IO.
using UnityEngine.UI;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace VK.BikeLab
{
    /// <summary>
    /// ManualControl receives data from the BikeInput script and controls the BikeController script using apropriate methods.
    /// </summary>
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#121-manualcontrol")]
    [RequireComponent(typeof(BikeInput))]
    public class ManualControl : MonoBehaviour
    {
        [Tooltip("Optional field. The slider visualizes user input along the X axis.")]
        public Slider sliderX;
        [Tooltip("Optional field. The slider visualizes the current steering angle.")]
        public Slider sliderSteer;
        [Tooltip("BikeController")]
        public Bike bike;
        public IKControl iKControl;
        [Tooltip("Scale of X axis. If X axis = 1 velocity = maxVelocity.")]
        [Range(0, 200)]
        public float maxVelocity;
        [Space]
        [Tooltip("If true, balance is carried out automatically else the user must balance manually. In the last case, steering angle calculated as mix between user input and balanced steering angle + dumper.")]
        public bool fullAuto;
        [Tooltip("The interpolation value between user input and balanced steering angle.")]
        [Range(0, 1)]
        public float autoBalance;
        [Tooltip("Damper factor.")]
        [Range(0, 1)]
        public float damper;
        [Tooltip("These fields are calculated automatically at runtime.")]
        [Space]
        public Info info;
        //public BikeInput1 bikeInput1;

        private BikeInput bikeInput;
        private Rigidbody rb;
        private float goldVelocity;
        private float forwardAcceleration;

        void Start()
        {
            bike.Init();
            rb = bike.getRigidbody();

            Camera123 camera123 = bike.gameObject.GetComponentInChildren<Camera123>();

            bikeInput = GetComponent<BikeInput>();

            goldVelocity = goldV();
        }
        private void FixedUpdate()
        {
            setVelo();
            setSteer();

            updateIK();

            info.currentLean = bike.getLean();
            info.currentSteer = bike.frontCollider.steerAngle;
            info.currentVelocity = rb.velocity.magnitude;
        }
        void Update()
        {
            bool rKey;
#if ENABLE_INPUT_SYSTEM
            rKey = Keyboard.current.rKey.wasPressedThisFrame;
#else
        rKey = Input.GetKey(KeyCode.R);
#endif
            if (rKey)
                bike.reset();

            if (sliderX != null)
                sliderX.value = bikeInput.xAxis + 0.5f;
            if (sliderSteer != null)
                sliderSteer.value = bike.frontCollider.steerAngle / bike.maxSteer + 0.5f;
        }
        private void updateIK()
        {
            if (iKControl == null)
                return;
            iKControl.inputData.velocity = rb.velocity.magnitude;
            iKControl.inputData.steer = bike.frontCollider.steerAngle;
            iKControl.inputData.targetLean = bike.getLean();
            iKControl.inputData.safeLean = bike.info.safeLean;
        }

        private void setSteer()
        {
            //float steer = bikeInput.xAxis * bike.maxSteer;
            float steer = bikeInput.xAxis * bike.info.safeSteer * 0.8f;
            //steer = Mathf.Clamp(steer, -bike.info.safeSteer, bike.info.safeSteer);
            //steer = roundAngle(steer, 4);
            info.targetSteer = steer;
            if (fullAuto)
                setAutoSteer();
            else
                setMixedSteer();
        }
        private void setVelo()
        {
            info.targetVelocity = bikeInput.yAxis * maxVelocity;
            Vector3 localV = transform.InverseTransformVector(rb.velocity);
            float diff = info.targetVelocity - localV.z;
            float a = Mathf.Clamp(diff * 1.0f, -10f, 10f);

            if (a > 0)
            {
                bike.setAcceleration(a);
                bike.safeBrake(0);
            }
            else
            {
                bike.setAcceleration(0);
                bike.safeBrake(-a);
            }
            forwardAcceleration = a;
        }
        private void setAutoSteer()
        {
            float dmp = getDumper() * damper;
            if (rb.velocity.magnitude > goldVelocity)
                bike.setSteerByLean(info.targetSteer - dmp);
                //bike.setLean(-info.targetSteer / bike.maxSteer * bike.maxLean);
            else if (rb.velocity.magnitude < 1)
                bike.setSteer(0);
            else
                bike.setSteer(info.targetSteer - dmp);

        }
        private float setMixedSteer()
        {
            float balanceSteer = bike.GetBalanceSteer();
            if (rb.velocity.magnitude < 1)
                balanceSteer = 0;
            //float dumper = bike.damper() * (Mathf.Pow(localV.z * 1.0f, 0.4f) + 3);
            float dmp = getDumper() * damper;
            float mix = Mathf.Lerp(info.targetSteer, balanceSteer, autoBalance);
            bike.setSteerDirectly(mix + dmp);
            return balanceSteer;
        }
        private float getDumper()
        {
            Vector3 av = transform.InverseTransformVector(rb.angularVelocity);
            float veloFactor = 1 / (rb.velocity.magnitude + 1);
            float lean = bike.getLean();
            float damper = -(av.z * 100 + lean * 1.3f) * veloFactor;
            damper = Mathf.Clamp(damper, -20, 20);
            return damper;
        }
        private float goldV()
        {
            float minD = 1000;
            float gold = 0;
            for (int i = 30; i < 60; i++)
            {
                float v = (float)i * 0.1f;
                float d = 0;
                //Debug.Log("******** v = " + v);
                for (int j = 0; j < 30; j++)
                {
                    float lean = (float)j;
                    float steer = bike.geometry.getSteer(-lean, v);
                    d += Mathf.Abs(lean - steer);
                    //Debug.Log("l = " + lean + "  s = " + steer);
                }
                if (d < minD)
                {
                    minD = d;
                    gold = v;
                }
            }
            //Debug.Log("gold = " + gold);
            return gold;
        }
        /// <summary>
        /// Rounds angle to part of Pi
        /// </summary>
        /// <param name="angle">angle</param>
        /// <param name="PIpart">Part of Pi. If PIpart = 4, angle will be rounded to Pi/4</param>
        /// <returns>Rounded angle</returns>
        private float roundAngle(float angle, int PIpart)
        {
            float pi = angle * Mathf.Deg2Rad / Mathf.PI;
            float r = Mathf.Round(pi * PIpart) / PIpart * Mathf.PI * Mathf.Rad2Deg;
            return r;
        }

        [System.Serializable]
        public class Info
        {
            [Space]
            [Range(-30, 30)] public float targetSteer;
            [Range(-30, 30)] public float currentSteer;
            [Space]
            [Range(-70, 70)] public float targetLean;
            [Range(-70, 70)] public float currentLean;
            [Tooltip("m/s")]
            [Space]
            [Range(0, 200)] public float targetVelocity;
            [Range(0, 200)] public float currentVelocity;
        }
    }
}