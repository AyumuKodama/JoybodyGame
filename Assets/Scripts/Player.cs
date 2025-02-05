using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Linq;
using System.Management;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private SerialPort stream;
    private OpticalController opticalController;
    [SerializeField] private float controllerSpeed = 40f;
    [SerializeField] private float controllerTorque = 5f;
    
    [SerializeField] private float chairSpeed = 40f;
    [SerializeField] private float chairTorque = 5f;
    private float prevPitch = 0;
    private float prevRoll = 180;
    private float currPitch, currRoll, currYaw, currAccX, currAccY, currAccZ, currGyroX, currGyroY, currGyroZ, inputPitch, inputRoll;
    public float midPitch, midRoll;
    [SerializeField] float frontPitch = 12f;
    [SerializeField] float backPitch = 7f;
    [SerializeField] float leftRoll = 167f;
    [SerializeField] float rightRoll = 171f;
    [SerializeField] float decelerate = 0.8f;
    private List<string> tiltValues = new List<string>();
    
    public enum ControlMode { Chair, Chair2, Controller, Controller2, Keyboard }

    public enum Port { Plugged, Bluetooth }

    [SerializeField] public ControlMode controlMode = new ControlMode();
    [SerializeField] private Port port = new Port();
    private string dir = "right";

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        if (port == Port.Plugged) {
            stream = new SerialPort("COM3",115200);
        } else if (port == Port.Bluetooth) {
            stream = new SerialPort("COM11",115200);
        }
        stream.Open();
        
    }

    public void SetMidPointChair()
    {
        midPitch = currPitch;
        midRoll = currRoll;
        backPitch = midPitch - 1;
        frontPitch = midPitch - 0.7f;
        leftRoll = midRoll - 0.7f;
        rightRoll = midRoll + 0.7f;
    }
    public void SetMidPointBody()
    {
        midPitch = currPitch;
        midRoll = currRoll;
        backPitch = midPitch - 5;
        frontPitch = midPitch - 1;
        leftRoll = midRoll - 3;
        rightRoll = midRoll + 3;
    }
    public void SetPlayerPos(Vector3 vector3)
    {
        transform.position = vector3;
    }
    public void SetPlayerRot(Vector3 vector3)
    {
        transform.eulerAngles = vector3;
    }

    void Control(ControlMode controlMode) {

        if (controlMode == ControlMode.Keyboard)
        {

            if (Input.GetAxis("Horizontal") > 0)
            {
                rb.AddTorque(transform.up * controllerTorque);
                dir = "right";
            }
            if (Input.GetAxis("Horizontal") < 0)
            {
                rb.AddTorque(-transform.up * controllerTorque);
                dir = "left";
            }
            if (Input.GetAxis("Vertical") > 0)
            {
                rb.AddForce(transform.forward * controllerSpeed);
            }
            if (Input.GetAxis("Vertical") < 0)
            {
                rb.AddForce(-transform.forward * controllerSpeed);
            }             
        }
        else if (controlMode == ControlMode.Controller)
        {
            Vector2 inputOVR = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick,OVRInput.Controller.RTouch);
            rb.AddForce(transform.forward * controllerSpeed * inputOVR.y);
            

            rb.AddTorque(transform.up * controllerTorque * inputOVR.x);
            // rb.AddTorque(-transform.forward * controllerTorque * inputOVR.x);
            // transform.rotation = Quaternion.Euler(Mathf.Clamp(transform.rotation.z, -60, 60),transform.rotation.y,transform.rotation.x);
            // transform.rotation = Quaternion.Euler(transform.rotation.x,transform.rotation.y,transform.rotation.z);
        
            if (inputOVR.x > 0.5f)
            {
                dir = "right";
            }
            else if (inputOVR.x < -0.5f)
            {
                dir = "left";
            }
            if (stream != null && stream.IsOpen)
            {
                if (stream.BytesToRead > 0)
                {
                    tiltValues = stream.ReadLine().Split(',').ToList(); 
                    currPitch = float.Parse(tiltValues[0]);
                    currRoll = float.Parse(tiltValues[1]);
                }
            }
                
        }
        else if (controlMode == ControlMode.Controller2)
        {
            Vector2 inputOVR = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick,OVRInput.Controller.RTouch);
            rb.AddForce(transform.forward * controllerSpeed * 0.8f);
            

            rb.AddTorque(transform.up * controllerTorque * inputOVR.x);
            // rb.AddTorque(-transform.forward * controllerTorque * inputOVR.x);
            // transform.rotation = Quaternion.Euler(Mathf.Clamp(transform.rotation.z, -60, 60),transform.rotation.y,transform.rotation.x);
            // transform.rotation = Quaternion.Euler(transform.rotation.x,transform.rotation.y,transform.rotation.z);
        
            if (inputOVR.x > 0.5f)
            {
                dir = "right";
            }
            else if (inputOVR.x < -0.5f)
            {
                dir = "left";
            }
            if (stream != null && stream.IsOpen)
            {
                if (stream.BytesToRead > 0)
                {
                    tiltValues = stream.ReadLine().Split(',').ToList(); 
                    currPitch = float.Parse(tiltValues[0]);
                    currRoll = float.Parse(tiltValues[1]);
                }
            }
                
        }
        else if (controlMode == ControlMode.Chair)
        {
            
            if (stream != null && stream.IsOpen)
            {
                if (stream.BytesToRead > 0)
                {
                    
                    tiltValues = stream.ReadLine().Split(',').ToList(); 
                    currPitch = float.Parse(tiltValues[0]);
                    currRoll = float.Parse(tiltValues[1]);
                    // currYaw = float.Parse(tiltValues[2]);
                    // currAccX = float.Parse(tiltValues[3]);
                    // currAccY = float.Parse(tiltValues[4]);
                    // currAccZ = float.Parse(tiltValues[5]);
                    // currGyroX = float.Parse(tiltValues[6]);
                    // currGyroY = float.Parse(tiltValues[7]);
                    // currGyroZ = float.Parse(tiltValues[8]);
                    //pitch = 11, roll = 169
                    //still state - pitch:0, roll:180
                    //back - pitch decrease 
                    //front - pitch increase
                    //right - roll increase, 180 -> -180
                    //left - roll decrease
                    Debug.Log(currPitch+", "+currRoll);
                    // rb.AddForce(transform.forward * chairSpeed * (currPitch - frontPitch));


                    if (currPitch < backPitch)
                    {
                        // if (currPitch <= prevPitch + 0.2)
                        // {
                            // rb.AddForce(-transform.forward * (chairSpeed + backPitch - currPitch));
                            inputPitch = (backPitch - currPitch);
                            // rb.AddForce(-transform.forward * chairSpeed);
                            rb.AddForce(-transform.forward * chairSpeed * inputPitch);
                        // }

                        
                    }
                    else if (currPitch > frontPitch)
                    {
                        // if (currPitch >= prevPitch - 0.2)
                        // {
                            // rb.AddForce(transform.forward * (chairSpeed + currPitch - frontPitch));
                            inputPitch = (currPitch - frontPitch);
                            // rb.AddForce(transform.forward * chairSpeed);
                            rb.AddForce(transform.forward * chairSpeed * inputPitch);
                        // }
                    }
                    // if (currRoll > rightRoll || currRoll > prevRoll - 0.2)
                    if (currRoll > rightRoll)
                    {
                        // if (currRoll >= prevRoll - 0.2)
                        // {
                            // rb.AddForce(transform.right * (chairSpeed + currRoll - rightRoll));
                            inputRoll = (currRoll - rightRoll);
                            // rb.AddTorque(transform.up * chairTorque);
                            rb.AddTorque(transform.up * chairTorque * inputRoll);

                            dir = "right";
                        // }

                    }
                    // else if (currRoll < leftRoll || currRoll < prevRoll + 0.2)
                    else if (currRoll < leftRoll)
                    {
                        // if (currRoll <= prevRoll + 0.2)
                        // {
                            // rb.AddForce(-transform.right * (chairSpeed + leftRoll - currRoll));
                            inputRoll = (leftRoll - currRoll);
                            // rb.AddTorque(-transform.up * chairTorque);
                            rb.AddTorque(-transform.up * chairTorque * inputRoll);

                            dir = "left";
                        // }
                    }
                    prevPitch = currPitch;
                    prevRoll = currRoll;
                    // if (rb.velocity.magnitude > 10)
                    // {
                    //     rb.velocity = rb.velocity.normalized * 10;
                    // } 
                    if (rb.angularVelocity.magnitude > 0.1f)
                    {
                        rb.angularVelocity = rb.angularVelocity.normalized * 0.1f;
                    } 
                    
                }
            }
        }
        else if (controlMode == ControlMode.Chair2)
        {
            
            if (stream != null && stream.IsOpen)
            {
                if (stream.BytesToRead > 0)
                {
                    tiltValues = stream.ReadLine().Split(',').ToList(); 
                    currPitch = float.Parse(tiltValues[0]);
                    currRoll = float.Parse(tiltValues[1]);
                    // currYaw = float.Parse(tiltValues[2]);
                    // currAccX = float.Parse(tiltValues[3]);
                    // currAccY = float.Parse(tiltValues[4]);
                    // currAccZ = float.Parse(tiltValues[5]);
                    // currGyroX = float.Parse(tiltValues[6]);
                    // currGyroY = float.Parse(tiltValues[7]);
                    // currGyroZ = float.Parse(tiltValues[8]);
                    //pitch = 11, roll = 169
                    //still state - pitch:0, roll:180
                    //back - pitch decrease 
                    //front - pitch increase
                    //right - roll increase, 180 -> -180
                    //left - roll decrease
                    
                    // rb.AddForce(transform.forward * chairSpeed * (currPitch - frontPitch));
                    // rb.AddForce(transform.forward * chairSpeed);

                    rb.AddForce(transform.forward * chairSpeed * 5);


                    // if (currRoll > rightRoll || currRoll > prevRoll - 0.2)
                    if (currRoll > rightRoll)
                    {
                        // if (currRoll >= prevRoll - 0.2)
                        // {
                            // rb.AddForce(transform.right * (chairSpeed + currRoll - rightRoll));
                            inputRoll = (currRoll - rightRoll);
                            // rb.AddTorque(transform.up * chairTorque);
                            rb.AddTorque(transform.up * chairTorque * inputRoll);

                            dir = "right";
                        // }

                    }
                    // else if (currRoll < leftRoll || currRoll < prevRoll + 0.2)
                    else if (currRoll < leftRoll)
                    {
                        // if (currRoll <= prevRoll + 0.2)
                        // {
                            // rb.AddForce(-transform.right * (chairSpeed + leftRoll - currRoll));
                            inputRoll = (leftRoll - currRoll);
                            // rb.AddTorque(-transform.up * chairTorque);
                            rb.AddTorque(-transform.up * chairTorque * inputRoll);

                            dir = "left";
                        // }
                    }
                    prevPitch = currPitch;
                    prevRoll = currRoll;
                    // if (rb.velocity.magnitude > 10)
                    // {
                    //     rb.velocity = rb.velocity.normalized * 10;
                    // } 
                    if (rb.angularVelocity.magnitude > 0.1f)
                    {
                        rb.angularVelocity = rb.angularVelocity.normalized * 0.1f;
                    } 
                    
                }
            }
        }

    }

    void Slow(string axis, float slowRate)
    {
        Vector3 temp = rb.velocity;
        if (axis.Equals("x")) 
        {
            temp.x *= slowRate;
        }
        else if (axis.Equals("z"))
        {
            temp.z *= slowRate;
        }
        rb.velocity = temp;
    }

    public string GetDir { get {return dir;} }
    public float GetPitch { get {return currPitch;} }
    public float GetRoll { get {return currRoll;} }

    void Update()
    {
    }

    void FixedUpdate()
    {
        Control(controlMode);
        // Debug.Log("pitch:"+currPitch+" ,roll:"+currRoll+" ,vel:"+rb.velocity+" ,rot:"+rb.angularVelocity);
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]

public class PlayerInspector : Editor
{
    private Player player;

    private void OnEnable()
    {
        player = target as Player;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Calibrate midpoint (chair)"))
        {
            player.SetMidPointChair();
            Debug.Log(player.midPitch +", "+ player.midRoll);
        }
        if(GUILayout.Button("Calibrate midpoint (body)"))
        {
            player.SetMidPointBody();
            Debug.Log(player.midPitch +", "+ player.midRoll);
        }
    }
}

#endif

