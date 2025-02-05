using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    [HelpURL("https://github.com/V-Kudryashov/md-Files/blob/main/BikeLab/BikeLab.md#134-damper")]
    public class Damper : MonoBehaviour
    {
        [Tooltip("This Transform is located on the swingarm")]
        public Transform damperBottom;
        public Transform spring;
        public float springLength = 0.2f;
        [Header("Visual model objects")]
        public Transform modelTop;
        public Transform modelBottom;
        public Transform modelSpring;

        private float springWidth = 0.08f;
        private float damperLength;
        void Start()
        {
            damperLength = (damperBottom.position - transform.position).magnitude;
            modelTop.parent = transform;
            modelBottom.parent = damperBottom;
            modelSpring.parent = spring;
        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(damperBottom, transform.up);
            damperBottom.LookAt(transform, damperBottom.up);
            spring.LookAt(damperBottom, spring.up);

            float curLength = (damperBottom.position - transform.position).magnitude;
            float scale = (curLength - damperLength) / springLength + 1;
            spring.localScale = new Vector3(1, 1, scale);
        }
        public void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3(0, 0, springLength / 2);
            Vector3 size = new Vector3(springWidth, springWidth, springLength);
            Gizmos.matrix = spring.localToWorldMatrix;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, size);
        }
        public void lookAt()
        {
            transform.LookAt(damperBottom, transform.up);
            damperBottom.LookAt(transform);

            Vector3 pos = spring.localPosition;
            pos.x = 0;
            spring.SetLocalPositionAndRotation(pos, Quaternion.identity);
        }
    }
}