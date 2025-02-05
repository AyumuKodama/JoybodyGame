using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StageController : MonoBehaviour
{
    public enum StartPoint {A, B, C, Practice}
    // public enum StageType { A, B, C, Practice, Straight, StraightLong, Base }
    // public enum StageSide { Left, Right }
    // [SerializeField] private Stage stageA;
    // [SerializeField] private Stage stageB;
    // [SerializeField] private Stage stageC;
    // [SerializeField] private Stage stagePractice;
    // [SerializeField] private Stage stageStraight;
    // [SerializeField] private Stage stageStraightLong;
    // [SerializeField] private Stage stageBase;
    // [SerializeField] private StageType stageType = StageType.Base;
        [SerializeField] private StartPoint startPoint = StartPoint.A;
    // [SerializeField] private StageSide stageSide = StageSide.Left;
    // private Dictionary<string,Stage> stages = new Dictionary<string,Stage>();
    // private Stage currStage;
    // public List<int> stageList = new List<int>();

    private Goal goal;
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        // stages.Add("A",stageA);
        // stages.Add("B",stageB);
        // stages.Add("C",stageC);
        // stages.Add("Practice",stagePractice);
        // stages.Add("Straight",stageStraight);
        // stages.Add("StraightLong",stageStraightLong);
        // stages.Add("Base",stageBase);
        goal = FindObjectOfType<Goal>();
        player = FindObjectOfType<Player>();
        SelectStage();
    }

    // public StageType GetStageType { get {return stageType;} }
    // public StageSide GetStageSide { get {return stageSide;} }
    
    // public void RandomStage()
    // {   
    //     int number = Random.Range(0,6);
    //     if (stageList.Count == 6)
    //     {
    //         stageList.Clear();
    //     }
    //     while (stageList.Contains(number))
    //     {
    //         number = Random.Range(0,6);
    //     }

    //     stageList.Add(number);
    //     switch (number)
    //     {
    //         case 0:
    //             stageType = StageType.A;
    //             stageSide = StageSide.Left;
    //             break;
    //         case 1:
    //             stageType = StageType.A;
    //             stageSide = StageSide.Right;
    //             break;
    //         case 2:
    //             stageType = StageType.B;
    //             stageSide = StageSide.Left;
    //             break;
    //         case 3:
    //             stageType = StageType.B;
    //             stageSide = StageSide.Right;
    //             break;
    //         case 4:
    //             stageType = StageType.C;
    //             stageSide = StageSide.Left;
    //             break;
    //         case 5:
    //             stageType = StageType.C;
    //             stageSide = StageSide.Right;
    //             break;
    //     }        
    // }
    // public void ClearStage()
    // {   
    //     stageList.Clear();
        
    // }

    void SelectStage()
    {
        // foreach (string s in stages.Keys)
        // {
        //     if (s == stageType.ToString())
        //     {
        //         stages[s].SwitchVisibility(true);
        //         currStage = stages[s];
        //     }
        //     else
        //     {
        //         stages[s].SwitchVisibility(false);
        //     }
        // }
        if (startPoint == StartPoint.A)
        {
            goal.SetGoalPos(new Vector3(-51.91f,1.37f,-209.83f));
            goal.SetGoalRot(new Vector3(0f,30f,10f));
            player.SetPlayerPos(new Vector3(-54.63f,-1.23f,-214.97f));
            player.SetPlayerRot(new Vector3(0f,30f,10f));
        }
        if (startPoint == StartPoint.B)
        {
            goal.SetGoalPos(new Vector3(-302.14f,0.15f,154.16f));
            goal.SetGoalRot(new Vector3(0f,180f,1f));
            player.SetPlayerPos(new Vector3(23f,3f,1f));
            player.SetPlayerRot(new Vector3(0f,180f,0f));
        }
        if (startPoint == StartPoint.C)
        {
            goal.SetGoalPos(new Vector3(-302.14f,0f,-120.7f));
            goal.SetGoalRot(new Vector3(0f,180f,0f));
            player.SetPlayerPos(new Vector3(-302.18f,-2.9f,-115.86f));
            player.SetPlayerRot(new Vector3(0f,180f,0f));
        }
        if (startPoint == StartPoint.Practice)
        {
            goal.SetGoalPos(new Vector3(-37.12f,0,13.8f));
            goal.SetGoalRot(new Vector3(0f,0f,0f));
            player.SetPlayerPos(new Vector3(-37.88f,-2.9f,9.53f));
            player.SetPlayerRot(new Vector3(0f,0f,0f));
        }
    }

    // void FlipStage()
    // {
    //     if (stageSide == StageSide.Left)
    //     {
    //         if (currStage.GetComponent<Transform>().localScale.x < 0)
    //         {
    //             currStage.FlipX();
    //         }
    //     }
    //     else if (stageSide == StageSide.Right)
    //     {
    //         if (currStage.GetComponent<Transform>().localScale.x > 0)
    //         {
    //             currStage.FlipX();
    //         }
    //     }
    // }

    // Update is called once per frame
    void Update()
    {
        // SelectStage();
        // FlipStage();
           
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StageController))]

public class StageControllerInspector : Editor
{
    private StageController stageController;

    private void OnEnable()
    {
        stageController = target as StageController;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // if (GUILayout.Button("Random Stage"))
        // {
        //     stageController.RandomStage();
        // }
        // if (GUILayout.Button("Clear Stage Backlog"))
        // {
        //     stageController.ClearStage();
        // }

    }
}

#endif