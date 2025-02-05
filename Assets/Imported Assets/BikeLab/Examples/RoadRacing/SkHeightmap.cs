using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class SuzukaHeightmap : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(" 1 " + pix2m(72));
            Debug.Log("min " + pix2m(68));
            Debug.Log(" 2 " + pix2m(72));
            Debug.Log(" 3 " + pix2m(81));
            Debug.Log(" 4 " + pix2m(93));
            Debug.Log(" 5 " + pix2m(102));
            Debug.Log(" 6 " + pix2m(113));
            Debug.Log(" 7 " + pix2m(137));
            Debug.Log(" 8 " + pix2m(133));
            Debug.Log(" 9 " + pix2m(124));
            Debug.Log("10 " + pix2m(137));
            Debug.Log("11 " + pix2m(149));
            Debug.Log("12 " + pix2m(151));
            Debug.Log("13 " + pix2m(153));
            Debug.Log("14 " + pix2m(152));
            Debug.Log("15 " + pix2m(136));
            Debug.Log("16 " + pix2m(147));
            Debug.Log("17 " + pix2m(145));
            Debug.Log("18 " + pix2m(128));
        }

        // Update is called once per frame
        void Update()
        {

        }
        private float pix2m(float pix)
        {
            float m = pix * 40 / 87 - 31;
            //Debug.Log("pix=" + pix + " m=" + m);
            return m;
        }
    }
}