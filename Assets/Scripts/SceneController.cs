using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneController : MonoBehaviour
{
    private Player player;
    private Rigidbody rb;
    private Goal goal;
    private CenterPoint centerPoint;
    private DataController dataController;
    private GameStatus gameStatus = GameStatus.Idle;
    private Vector3 origPos;
    private Vector3 origRot;

    public enum GameStatus { Idle, Playing, GameOver, GameClear }

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        goal = FindObjectOfType<Goal>();
        centerPoint = FindObjectOfType<CenterPoint>();
        dataController = FindObjectOfType<DataController>();
        origPos = player.transform.position;
        origRot = player.transform.eulerAngles;
        rb = player.GetComponent<Rigidbody>();
        StartCoroutine(Reset());
        
    }

    IEnumerator Reset()
    {
        goal.GoalEntered = false;
        gameStatus = GameStatus.Idle;

        player.transform.position = origPos;
        player.transform.eulerAngles = origRot;
        rb.isKinematic = true;
        // player.enabled = false;
        centerPoint.ClearState(false);

        yield return new WaitUntil(() => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.RTouch));
        // yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        rb.isKinematic = false;
        // player.enabled = true;

        dataController.StartRecord();
    }

    // Update is called once per frame
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.E))
        {
            gameStatus = GameStatus.GameClear;
        }
        if (goal.GoalEntered)
        {
            
            if (gameStatus == GameStatus.Idle)
            {
                gameStatus = GameStatus.Playing;
                Debug.Log("playing");
            }
            else if (gameStatus == GameStatus.Playing)
            {
                gameStatus = GameStatus.GameClear;
                Debug.Log("clear");
            }
        }
        if (player.transform.position.y <= -100) {
            gameStatus = GameStatus.GameOver;
        }
        if (gameStatus == GameStatus.GameClear || gameStatus == GameStatus.GameOver) {
            // dataController.StopRecord();
            // centerPoint.ClearState(true);

            if (Input.GetKeyDown(KeyCode.R)) {
                //temporary - manual control
                dataController.StopRecord();
                centerPoint.ClearState(true);
                StartCoroutine(Reset());
            }
        }

        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneController))]
public class SceneControllerInspector : Editor
{
    private SceneController sceneController;
    bool dataSettings = false;
    bool moveSettings = false;
    bool stimSettings = false;
    bool stageSettings = false;

    private void OnEnable()
    {
        sceneController = target as SceneController;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        dataSettings = EditorGUILayout.BeginFoldoutHeaderGroup(dataSettings, "Data");
        if (dataSettings)
        {
            /*
            SceneController
            [SerializeField] private string testerName = "none";
            [SerializeField] private float timeStep = 0.25f;
            [SerializeField] private bool recordState = true;
            */
        
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        moveSettings = EditorGUILayout.BeginFoldoutHeaderGroup(moveSettings, "Movement");
        if (moveSettings)
        {
            /*
            Player
            [SerializeField] private float speed = 40f;
            [SerializeField] float frontPitch = 12f;
            [SerializeField] float backPitch = 7f;
            [SerializeField] float leftRoll = 167f;
            [SerializeField] float rightRoll = 171f;
            [SerializeField] float decelerate = 0.8f;
            [SerializeField] public ControlMode controlMode = new ControlMode();
            [SerializeField] private Port port = new Port();
            */
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        stimSettings = EditorGUILayout.BeginFoldoutHeaderGroup(stimSettings, "Stimulus");
        if (stimSettings)
        {
            /*
            OpticalController
            [SerializeField] private float rotateSpeed = 40f;
            [SerializeField] private Stimulus stim = Stimulus.None;
            [SerializeField] private RotateMode rotateMode = RotateMode.Fixed;
            [SerializeField] private RotateDir rotateDir = RotateDir.None;

            if stim = particles
                Particles
                [SerializeField] GameObject particlePrefab; // remove?
                [SerializeField] float cylinderDiameter; // 円柱の直径  // OpticalController?
                [SerializeField] float cylinderHeight; // 円柱の高さ // OpticalController?
                [SerializeField] int particleNum; // 生成する球体の数
                [SerializeField] float startAngle;
                [SerializeField] float endAngle;

                ParticlesInspector
                UpdateParticles
                ClearParticles

            if stim = stripe
                Stripe
                [SerializeField,Range(0f, 35.0f)] private float offset = 0f; //-z;
                [SerializeField,Range(0f, 1.0f)] private float opacity = 1f; // OpticalController?

            */
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        stageSettings = EditorGUILayout.BeginFoldoutHeaderGroup(stageSettings, "Stage");
        if (stageSettings)
        {
            /*
            StageController
            [SerializeField] private GameObject stageA;
            [SerializeField] private GameObject stageB;
            [SerializeField] private GameObject stageC;
            [SerializeField] private GameObject stagePractice;
            [SerializeField] private GameObject stageStraight;
            [SerializeField] private StageType stageType = StageType.Left;
            */
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
#endif
