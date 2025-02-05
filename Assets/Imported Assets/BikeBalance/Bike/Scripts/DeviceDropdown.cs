using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#123-devicedropdown")]
    public class DeviceDropdown : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [Tooltip("This Dropdown will be contain available Input Devices.")]
        public Dropdown deviceDropdown;
        //public ManualControl manualControl;
        public BikeInput bikeInput;
        void Start()
        {
            InputSystem.onDeviceChange += deviceChanged;
            fillDropdown();

            deviceDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(deviceDropdown); });
            DropdownValueChanged(deviceDropdown);
        }
        void Update()
        {

        }
        private void deviceChanged(InputDevice device, InputDeviceChange change)
        {
            fillDropdown();
        }
        private void fillDropdown()
        {
            deviceDropdown.ClearOptions();
            foreach (InputDevice d in InputSystem.devices)
            {
                deviceDropdown.options.Add(new Dropdown.OptionData(d.displayName));
            }
            deviceDropdown.RefreshShownValue();
        }
        private void DropdownValueChanged(Dropdown change)
        {
            int index = Mathf.Clamp(change.value, 0, InputSystem.devices.Count - 1);
            InputDevice inputDevice = InputSystem.devices[index];
            bikeInput.setInputDevice(inputDevice);
        }
#endif
    }
}