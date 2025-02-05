using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void ClearState(bool cleared)
    {
        
        if (cleared) //green
        { 
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }
        else //red
        {
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
