using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VK.BikeLab
{
    public class ToDemo : MonoBehaviour
    {
        public GameObject panelMunu;
        public void loadDemo()
        {
            SceneManager.LoadScene("Demo");
        }
        public void menu()
        {
            panelMunu.SetActive(!panelMunu.activeSelf);
        }
    }
}