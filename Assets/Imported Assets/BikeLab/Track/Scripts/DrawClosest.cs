using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VK.BikeLab
{

    public class DrawClosest : MonoBehaviour
    {
        public TrackSpline spline;
        private float lastL;
        private Vector3 closest;
        void Start()
        {
            spline.updateSpline();

            Vector3 point = spline.transform.InverseTransformPoint(transform.position);
            closest = spline.spline.getClosestL(point, out float closestL);
            lastL = closestL;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 cl = spline.transform.TransformPoint(closest);
            Debug.DrawLine(transform.position, cl, Color.red);

            Vector3 point = spline.transform.InverseTransformPoint(transform.position);
            closest = spline.spline.getClosestL(point, out float newL, lastL);
            lastL = newL;

            cl = spline.transform.TransformPoint(closest);
            Debug.DrawLine(transform.position, cl, Color.black);

        }
    }
}
