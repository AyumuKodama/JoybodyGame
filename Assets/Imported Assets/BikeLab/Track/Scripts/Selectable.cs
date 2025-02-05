using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class Selectable : MonoBehaviour
    {
        private Camera123 cam123;
        private void Awake()
        {
            cam123 = GetComponentInChildren<Camera123>(true);
        }
        public bool selected()
        {
            if (cam123 == null)
                return false;
            return cam123.gameObject.activeSelf;
        }
        public bool select(bool select)
        {
            if (cam123 == null)
                return false;
            cam123.gameObject.SetActive(select);
            return select; 
        }
    }
}