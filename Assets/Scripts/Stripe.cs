using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Stripe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void SwitchVisibility(bool state)
    {
        gameObject.SetActive(state);
    }

    public void Turn(float rotateSpeed)
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    public void SetOpacity(float opacity)
    {
        Color c = gameObject.GetComponent<Renderer>().material.color;
        c.a = opacity;
        gameObject.GetComponent<Renderer>().material.color = c;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
