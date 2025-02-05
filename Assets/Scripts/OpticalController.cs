using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class OpticalController : MonoBehaviour
{
    public enum Stimulus { None, Stripe, Particle }
    public enum RotateMode { Fixed, Assist, Distract }
    public enum RotateDir { None, Clockwise, CounterClockwise }
    public enum RotateAxis { Pitch, Roll, Yaw }
    
    [SerializeField] private float rotateSpeed = 40f;
    [SerializeField] private Stimulus stim = Stimulus.Particle;
    [SerializeField] private RotateMode rotateMode = RotateMode.Fixed;
    [SerializeField] private RotateDir rotateDir = RotateDir.None;
    [SerializeField] private RotateAxis rotateAxis = RotateAxis.Roll;
    [SerializeField,Range(0f, 1.0f)] private float opacity = 0.5f;
    private Stripe stripe;
    private Particles particles;
    private Player player;
    private float rotateAngle;
    private int stimIndex;

    // Start is called before the first frame update
    void Start()
    {
        stripe = FindObjectOfType<Stripe>();
        particles = FindObjectOfType<Particles>();
        player = FindObjectOfType<Player>();
    }

    public Stimulus GetStimulus { get {return stim;} }
    public RotateMode GetRotateMode { get {return rotateMode;} }
    public RotateDir GetRotateDir { get {return rotateDir;} }
    public RotateAxis GetRotateAxis { get {return rotateAxis;} }

    //particles
    public void UpdateParts()
    {
        particles.UpdateParticles();
    }
    public void ClearParts()
    {
        particles.ClearParticles();
    }

    // Update is called once per frame
    void Update()
    {
        
        //toggle stimulus
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            stimIndex = (int)stim + 1;
            stimIndex = (stimIndex == Stimulus.GetValues(typeof(Stimulus)).Length) ? 0 : stimIndex;
            print(stimIndex);
            stim = (Stimulus)stimIndex;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            stimIndex = (int)stim - 1;
            stimIndex = (stimIndex == -1) ? Stimulus.GetValues(typeof(Stimulus)).Length - 1 : stimIndex;
            print(stimIndex);
            stim = (Stimulus)stimIndex;
        } 

        //rotate mode
        switch (rotateMode)
        {
            case RotateMode.Fixed:
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    rotateDir = RotateDir.CounterClockwise;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    rotateDir = RotateDir.Clockwise;
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    rotateDir = RotateDir.None;
                }
                break;
            case RotateMode.Assist:
                if (player.GetDir == "left")
                {
                    rotateDir = RotateDir.CounterClockwise;
                }
                else if (player.GetDir == "right")
                {
                    rotateDir = RotateDir.Clockwise;
                }
                break;
            case RotateMode.Distract:
                if (player.GetDir == "left")
                {
                    rotateDir = RotateDir.Clockwise;
                }
                else if (player.GetDir == "right")
                {
                    rotateDir = RotateDir.CounterClockwise;
                }
                break;
        }

        //rotate axis
        switch (rotateAxis)
        {
            // case RotateAxis.Pitch:
            //     particles.SetRotateAxis("x");
            //     rotateSpeed = 40f;
            //     break;
            case RotateAxis.Roll:
                particles.SetRotateAxis("z");
                rotateSpeed = 60f;
                break;
            // case RotateAxis.Yaw:
            //     particles.SetRotateAxis("y");
            //     rotateSpeed = 40f;
            //     break;
        }

        //rotate direction
        switch (rotateDir)
        {
            case RotateDir.Clockwise:  
                rotateAngle = -rotateSpeed;
                break;
            case RotateDir.CounterClockwise:
                rotateAngle = rotateSpeed;
                break;
            case RotateDir.None:
                rotateAngle = 0;
                break;

        }

        //stimulus
        switch(stim)
        {
            case Stimulus.None:
                stripe.SwitchVisibility(false);
                particles.SwitchVisibility(false);
                break;
            case Stimulus.Stripe:
                stripe.SwitchVisibility(true);
                particles.SwitchVisibility(false);

                stripe.Turn(rotateAngle);
                stripe.SetOpacity(opacity);
                break;
            case Stimulus.Particle:
                stripe.SwitchVisibility(false);
                particles.SwitchVisibility(true);
                particles.Turn(rotateAngle);
                particles.SetOpacity(opacity);
                break;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OpticalController))]
public class OpticalControllerInspector : Editor
{
    private OpticalController opticalController;

    private void OnEnable()
    {
        opticalController = target as OpticalController;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Update Particles"))
        {
            opticalController.UpdateParts();
        }
        if(GUILayout.Button("Clear Particles"))
        {
            opticalController.ClearParts();
        }
    }
}

#endif
