using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#372-trafficlightbox")]
    public class TrafficLightBox : MonoBehaviour
    {
        public TrackSpline track;
        public bool inverse;
        public List<MeshRenderer> red;
        public List<MeshRenderer> yellow;
        public List<MeshRenderer> green;
        public Material redOff;
        public Material redOn;
        public Material yellowOff;
        public Material yellowOn;
        public Material greenOff;
        public Material greenOn;

        public void setColor(TrafficColor color )
        {
            if (inverse)
            {
                if (color == TrafficColor.Red)
                    color = TrafficColor.Green;
                else if (color == TrafficColor.Green)
                    color = TrafficColor.Red;
            }

            if (color == TrafficColor.Yellow && yellow.Count == 0)
                color = TrafficColor.Red;

            foreach (MeshRenderer r in red)
                if (color == TrafficColor.Red)
                    r.material = redOn;
                else
                    r.material = redOff;

            foreach (MeshRenderer r in yellow)
                if (color == TrafficColor.Yellow)
                    r.material = yellowOn;
                else
                    r.material = yellowOff;

            foreach (MeshRenderer r in green)
                if (color == TrafficColor.Green)
                    r.material = greenOn;
                else
                    r.material = greenOff;
        }
    }
}