using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#122-bikeinput")]
    public class BikeInput : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private InputDevice inputDevice;
        public KeyboardSettings keyboardSettings;
        public JoystickSettings joystickSettings;
#else
        [Tooltip("User input sensitivity.")]
        [Range(0.01f, 0.1f)]
        public float sensitivity = 0.01f;
        [Tooltip("Determines the speed at which the return to zero occurs.")]
        [Range(0, 0.1f)]
        public float toZero = 0.01f;
#endif
        /// <summary>
        /// Output of this script.
        /// </summary>
        [Tooltip("Output of this script.")]
        public float xAxis;
        /// <summary>
        /// Output of this script.
        /// </summary>
        [Tooltip("Output of this script.")]
        public float yAxis;

        private void Start()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputDevice == null)
                inputDevice = InputSystem.devices[0];
#endif
        }
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputDevice is Keyboard)
                keyboard((Keyboard)inputDevice);
            if (inputDevice is Mouse)
                mouse((Mouse)inputDevice);
            if (inputDevice is Joystick)
                joystick((Joystick)inputDevice);
            if (inputDevice is Gamepad)
                gamepad((Gamepad)inputDevice);
#else
            oldInput();
            xAxis *= (1 - toZero);
#endif
            xAxis = Mathf.Clamp(xAxis, -1, 1);
            yAxis = Mathf.Clamp(yAxis, -1, 1);
        }
        private void FixedUpdate()
        {
        }
#if ENABLE_INPUT_SYSTEM
        private void keyboard(Keyboard kb)
        {
            xAxis *= (1 - keyboardSettings.toZero);

            if ((kb.aKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad4Key.isPressed && keyboardSettings.numpad) ||
                (kb.leftArrowKey.isPressed && keyboardSettings.arrows))
                xAxis -= keyboardSettings.sensitivityX;

            if ((kb.dKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad6Key.isPressed && keyboardSettings.numpad) ||
                (kb.rightArrowKey.isPressed && keyboardSettings.arrows))
                xAxis += keyboardSettings.sensitivityX;

            if ((kb.wKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad8Key.isPressed && keyboardSettings.numpad) ||
                (kb.upArrowKey.isPressed && keyboardSettings.arrows))
            {
                yAxis = Mathf.Max(yAxis, 0);
                yAxis += keyboardSettings.sensitivityY;
            }

            if ((kb.sKey.isPressed && keyboardSettings.AWSD) ||
                (kb.numpad2Key.isPressed && keyboardSettings.numpad) ||
                (kb.downArrowKey.isPressed && keyboardSettings.arrows))
                if (yAxis > 0)
                    yAxis -= keyboardSettings.sensitivityY;

            if (kb.spaceKey.isPressed)
            {
                yAxis = Mathf.Min(yAxis, 0);
                yAxis -= keyboardSettings.sensitivityY;
            }
        }
        private void mouse(Mouse mouse)
        {
            if (mouse.leftButton.isPressed)
                xAxis -= keyboardSettings.sensitivityX;
            if (mouse.rightButton.isPressed)
                xAxis += keyboardSettings.sensitivityX;

            Vector2 scroll = mouse.scroll.ReadValue();
            if (scroll.y > 0)
                yAxis += keyboardSettings.sensitivityY * 10;
            if (scroll.y < 0)
                yAxis -= keyboardSettings.sensitivityY * 10;

            xAxis *= (1 - keyboardSettings.toZero);
        }
        private void joystick(Joystick joystick)
        {
            InputSystem.settings.defaultDeadzoneMin = joystickSettings.deadZoneMin;
            Vector2 stick = joystick.stick.ReadValue();
            xAxis = stick.x * joystickSettings.sensitivityX;
            yAxis = stick.y * joystickSettings.sensitivityY;
        }
        private void gamepad(Gamepad gamepad)
        {
            xAxis = gamepad.leftStick.x.ReadValue();
            yAxis = gamepad.rightStick.y.ReadValue();
        }
        public void setInputDevice(InputDevice inputDevice)
        {
            this.inputDevice = inputDevice;
        }
#else
        private void oldInput()
        {
            xAxis += Input.GetAxis("Horizontal") * sensitivity;
            yAxis += Input.GetAxis("Vertical") * sensitivity;
        }
#endif
        [System.Serializable]
        public class KeyboardSettings
        {
            public bool AWSD = true;
            public bool arrows = true;
            public bool numpad = true;
            [Space]
            [Range(0.01f, 1.0f)]
            public float sensitivityX = 0.01f;
            [Range(0.01f, 1.0f)]
            public float sensitivityY = 0.01f;
            [Range(0, 0.5f)]
            public float toZero = 0.01f;
        }
        [System.Serializable]
        public class JoystickSettings
        {
            [Range(0, 1)]
            public float sensitivityX = 1;
            [Range(0, 1)]
            public float sensitivityY = 1;
            [Range(0, 0.9f)]
            public float deadZoneMin = 0;
        }
    }
}