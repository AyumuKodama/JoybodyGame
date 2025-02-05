using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VK.BikeLab
{
    public class LoadScene : MonoBehaviour
    {
        public int sceneIndex;

        private void Start()
        {
            
        }
        public void load()
        {
            Text text = GetComponentInChildren<Text>();
            Debug.Log(text.text);
            SceneManager.LoadScene(text.text);
        }
    }
}