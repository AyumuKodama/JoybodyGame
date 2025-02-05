using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.BikeLab
{
    public class PoseTransfer : MonoBehaviour
    {
        public Transform sourceRoot;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void transferPose()
        {
            Animator animator = GetComponent<Animator>();
            HumanPoseHandler handlerFrom = new HumanPoseHandler(animator.avatar, sourceRoot);
            HumanPoseHandler handlerTo = new HumanPoseHandler(animator.avatar, transform);
            HumanPose pose = new HumanPose();
            handlerFrom.GetHumanPose(ref pose);
            handlerTo.SetHumanPose(ref pose);
        }
    }
}    