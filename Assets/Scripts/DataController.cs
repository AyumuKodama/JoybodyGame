using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

public class DataController : MonoBehaviour
{
    private Player player;
    private Rigidbody rb;
    private OpticalController opticalController;
    private StageController stageController;
    // public bool recordState = true;
    [SerializeField] private bool recordState = true;
    // [HideInInspector] public string testerName = "none";
    [SerializeField] private string testerName = "none";
    private float time = 0;
    [SerializeField] private float timeStep = 0.25f;
    // [HideInInspector] public float timeStep = 0.25f;
    

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        rb = player.GetComponent<Rigidbody>();
        opticalController = FindObjectOfType<OpticalController>();
        stageController = FindObjectOfType<StageController>();
    }
    public void StartRecord()
    {
        if (recordState)
        {
            string[] s1 = {"\r\nName","Stage","Stimulus","RotateMode","RotateAxis","RotateDir","Control","Time", "X", "Z","VelX","VelZ","pitch","roll"};
            string s2 = string.Join(",", s1);
            using (StreamWriter sw = new StreamWriter("data.csv", true))
            {
                sw.WriteLine(s2);
            }
            InvokeRepeating("Record",0f,timeStep);
        }
    }

    public void StopRecord()
    {
        if (recordState)
        {
            CancelInvoke("Record");
            time = 0;
        }
    }

    public bool RecordState { get{return recordState;} set{recordState = value;} }
    public string TesterName { get{return testerName;} set{testerName = value;} }
    public float TimeStep { get{return timeStep;} set{timeStep = value;} }

    void Record()
    {        
        string[] s1 = {testerName,
                        "x",// stageController.GetStageType.ToString() + stageController.GetStageSide.ToString(),
                        opticalController.GetStimulus.ToString(),
                        opticalController.GetRotateMode.ToString(),
                        opticalController.GetRotateAxis.ToString(),
                        opticalController.GetRotateDir.ToString(),
                        player.controlMode.ToString(),
                        time.ToString(),
                        player.transform.position.x.ToString(),
                        player.transform.position.z.ToString(),
                        rb.velocity.x.ToString(),
                        rb.velocity.z.ToString(),
                        player.GetPitch.ToString(),
                        player.GetRoll.ToString()};
        string s2 = string.Join(",", s1);
        using (StreamWriter sw = new StreamWriter("data.csv", true))
        {
            sw.WriteLine(s2);
        }
        time += timeStep;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// #if UNITY_EDITOR
// [CustomEditor(typeof(DataController))]

// public class DataControllerInspector : Editor
// {
//     private DataController dataController;

//     private void OnEnable()
//     {
//         dataController = target as DataController;
//     }
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//         if(dataController.recordState)
//         {
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("Tester Name", GUILayout.MaxWidth(120));
//             EditorGUILayout.TextField(dataController.testerName);
//             EditorGUILayout.EndHorizontal();

//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.LabelField("Time Step", GUILayout.MaxWidth(120));
//             EditorGUILayout.FloatField(dataController.timeStep);
//             EditorGUILayout.EndHorizontal();
//         }
//     }
// }

// #endif

