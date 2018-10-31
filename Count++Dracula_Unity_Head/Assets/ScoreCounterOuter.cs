using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreCounterOuter : MonoBehaviour 
{
    public Text OuterScoreText;
    int OuterScoreCounter;

    void Start()
    {
        OuterScoreCounter = UserInputController.outerCount_input;
        OuterScoreText = GetComponent<Text>();

    }

    void Update()
    {
        // convert to string, output as text in scene
        OuterScoreText.text = OuterScoreCounter.ToString();
    }


}