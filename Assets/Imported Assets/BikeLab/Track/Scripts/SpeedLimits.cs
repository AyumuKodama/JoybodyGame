using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#38-speedlimits")]
    public class SpeedLimits : MonoBehaviour
    {
        public GameObject prefab;
        public TrackSpline trackSpline;
        public void addRoadSigns()
        {
            removeRoadSigns();

            Bike bikeController = FindObjectOfType<Bike>();
            TrackController trackController = bikeController.GetComponent<TrackController>();
            float maxSideA = -(1 + 0.03f * 5) * Physics.gravity.y; //5

            foreach (SplineBase.Turn turn in trackSpline.spline.turns)
            {
                float r;
                if (trackController.useSmoothRadius)
                    r = turn.smoothRadius;
                else
                    r = turn.radius;
                float steerFactor = 1;
                if (r < 10) //10
                    steerFactor = Mathf.Pow(r * 0.1f, 0.6f); // Pow(r * 0.1f, 0.6f);
                                                             //float maxV = Mathf.Sqrt(maxSideA * r) * steerFactor * 3.6f;
                float maxV = bikeController.getMaxVelocity(r) * steerFactor * 3.6f;

                Vector3 pos = trackSpline.transform.TransformPoint(turn.position);
                pos.y += 2;
                Vector3 dir = trackSpline.transform.TransformVector(turn.endPoint - turn.startPoint);
                Vector3 lookAt = pos + dir;
                GameObject go = Instantiate(prefab, transform);
                go.transform.position = pos;
                go.transform.LookAt(lookAt, Vector3.up);
                go.transform.position += go.transform.right * (trackSpline.trackWidth / 4 + 3);

                TMPro.TextMeshPro textMesh = go.GetComponent<TMPro.TextMeshPro>();
                textMesh.text = maxV.ToString("0");
            }
        }
        public void removeRoadSigns()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(prefab.tag);
            foreach (GameObject go in objects)
                GameObject.DestroyImmediate(go);
        }
    }
}