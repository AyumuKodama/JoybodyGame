using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#142-sound")]
    public class Sound : MonoBehaviour
    {
        public AudioSource idle;
        public AudioSource run;
        public Bike bikeController;
        [Tooltip("RPS of run AudioClip")]
        public float runRPS = 33;
        [Tooltip("Full gear ratio")]
        public float gearRatio = 24; //3,611 * 1,75 * 3,786 = 24
        [Tooltip("Info field")]
        public float pitch;

        //private RacingDispatcher dispatcher;
        private WheelCollider wheel;
        public bool sound = true;
        private float rps;
        void Start()
        {
            wheel = bikeController.rearCollider;
            //dispatcher = FindObjectOfType<RacingDispatcher>();
        }

        // Update is called once per frame
        void Update()
        {
            bool oKey;
#if ENABLE_INPUT_SYSTEM
            oKey = Keyboard.current.oKey.wasPressedThisFrame;
#else
        oKey = Input.GetKey(KeyCode.O);
#endif
            if (oKey)
            {
                sound = !sound;
            }
            rps = Mathf.Abs(wheel.rpm / 60);
            if (sound)
            {
                if (rps < 0.001f)
                {
                    idle.mute = false;
                    run.mute = true;
                }
                else
                {
                    idle.mute = true;
                    run.mute = false;
                    updateRun();
                }
            }
            else
            {
                idle.mute = true;
                run.mute = true;
            }
        }
        private void updateRun()
        {
            float pitch = gearRatio * rps / runRPS;
            this.pitch = pitch;
            run.pitch = pitch;

            float a = bikeController.info.motorAcceleration;
            float t = a * 0.1f;
            float volume = Mathf.Lerp(0.1f, 1, t);
            run.volume = volume;
        }
    }
}