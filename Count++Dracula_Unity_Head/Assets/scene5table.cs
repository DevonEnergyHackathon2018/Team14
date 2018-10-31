using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class scene5table : MonoBehaviour {

    public Text name, inneravg, outeravg, guage, area, reasonpull, pdull, sdull;

	// Use this for initialization
	void Start () {

        name.text = UserInputController.names;
        inneravg.text = UserInputController.inner_average.ToString("F2");
        outeravg.text = UserInputController.outer_average.ToString("F2");
        guage.text = UserInputController.Guage.ToString("F2");
        area.text = UserInputController.Area;
        reasonpull.text = UserInputController.Pulled;
        pdull.text = UserInputController.dull_first;
        sdull.text = UserInputController.dull_second;

    }

    // Update is called once per frame
    void Update () {
		
	}

    public void FinishAction() {

        SceneManager.LoadScene("Scene1");

    }
}
