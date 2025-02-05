using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private bool goalEntered = false;
    public bool GoalEntered
    {
        get { return goalEntered; }
        set { goalEntered = value; }
    }

    void OnTriggerExit(Collider other) {
        if (other.name == "Player") {
            goalEntered = true;
        }
    }

    public void SetGoalPos(Vector3 vector3)
    {
        transform.position = vector3;
    }
    public void SetGoalRot(Vector3 vector3)
    {
        transform.eulerAngles = vector3;
    }
    public void SetGoalScale(Vector3 vector3)
    {
        transform.localScale = vector3;
    }
}
