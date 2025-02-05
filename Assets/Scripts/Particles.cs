using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

public class Particles : MonoBehaviour
{
    [SerializeField] GameObject particlePrefab;
    // [SerializeField] GameObject background;
    [SerializeField] float cylinderDiameter; // 円柱の直径
    [SerializeField] float cylinderHeight; // 円柱の高さ
    [SerializeField] int particleNum; // 生成する球体の数
    [SerializeField] float startAngle;
    [SerializeField] float endAngle;
    List<GameObject> particles = new List<GameObject>();
    private string rotateAxis = "z";
    private float rotate = 0f;
    float angle;
    // [Title("事前生成")]
    // [Button("生成")] void OnClickedClearParticlesButton() {UpdateParticles();}
    // [Button("削除")] void OnClickedUpdateParticlesButton() {ClearParticles();}
    // [Title("パーティクル制御")]
    // [Button("表示")] void OnClickedShowParticlesButton() {SwitchVisibility(true);}
    // [Button("非表示")] void OnClickedHideParticlesButton() {SwitchVisibility(false);}

    void Start()
    {
        GenerateParticles();
    }

    public void SwitchVisibility(bool state) {
        gameObject.SetActive(state);
        // background.SetActive(state);
    }

    public void Turn(float rotateSpeed)
    {
        if (rotateAxis == "z")
        {
            rotate = rotateSpeed * Time.deltaTime;
            transform.Rotate(0,0,rotate);
            // transform.Rotate(transform.forward * rotateSpeed * Time.deltaTime, Space.World);
            // transform.rotation = transform.parent.rotation;
        }
    }

    public void SetOpacity(float opacity)
    {
        // Color c = background.GetComponent<Renderer>().material.color;
        // c.a = opacity;
        // background.GetComponent<Renderer>().material.color = c;
    }

    public void ClearParticles() {
        for (int i = gameObject.transform.childCount; i > 0; --i) {
            DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
        }
    }

    public void UpdateParticles() {
        ClearParticles();
        GenerateParticles();
    }

    void GenerateParticles()
    {
        for (int i = 0; i < particleNum; i++)
        {
            // 円柱の表面上のランダムな位置を求める
            Vector3 randomPoint = RandomPointOnCylinderSurface();
            

            // 球体を生成してランダムな位置に配置する
            GameObject p = Instantiate(particlePrefab, transform.position + randomPoint, Quaternion.identity);
            // GameObject p = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity);
            particles.Add(p);
            // p.transform.SetParent(gameObject.transform, false);
            p.transform.SetParent(gameObject.transform);
            // p.transform.localPosition = randomPoint;


        }
    }

    Vector3 RandomPointOnCylinderSurface()
    {
        if (rotateAxis == "z")
        {
            cylinderDiameter = 5;
            particleNum = 800;
        }
        // else
        // {
        //     cylinderDiameter = 10;
        //     particleNum = 1600;
        // }
        // 円柱の周囲をランダムに選択
        float randomAngle = Random.Range(startAngle * Mathf.Deg2Rad, endAngle * Mathf.Deg2Rad);

        // 円柱の表面上のランダムな位置を計算
        float x = Mathf.Cos(randomAngle) * cylinderDiameter / 2;
        float y = Mathf.Sin(randomAngle) * cylinderDiameter / 2;
        float z = Random.Range(0f, cylinderHeight);

        return new Vector3(x, y, z);
        // return new Vector3(x, z, y);
    }

    public void SetRotateAxis(string axis)
    {
        rotateAxis = axis;
    }

    void Update() {
        // angle = transform.rotation.x;
        // transform.localRotation = Quaternion.Euler(angle,0,0);
    }
}

// #if UNITY_EDITOR
// [CustomEditor(typeof(Particles))]
// public class ParticlesInspector : Editor
// {
//     private Particles particles;

//     private void OnEnable()
//     {
//         particles = target as Particles;
//     }
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//         if(GUILayout.Button("Update Particles"))
//         {
//             particles.UpdateParticles();
//         }
//         if(GUILayout.Button("Clear Particles"))
//         {
//             particles.ClearParticles();
//         }
//     }
// }

// #endif
