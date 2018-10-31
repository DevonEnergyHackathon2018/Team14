using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour
    {
     public void OnMouseClick() {
        Debug.Log("Hit");
        SceneManager.LoadScene("Classify");}
    }
