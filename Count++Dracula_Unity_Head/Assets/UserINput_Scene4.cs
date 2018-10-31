using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserINput_Scene4 : MonoBehaviour {


    public Dropdown Area;
    public Dropdown Pulled;
    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Scene4Done()
    {
        UserInputController.Area = Area.options[Area.value].text;
        UserInputController.Pulled = Pulled.options[Pulled.value].text;
        SceneManager.LoadScene("Scene5");
    }
}
