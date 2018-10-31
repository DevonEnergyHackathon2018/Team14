using UnityEngine;
using System.Collections;
using UnityEngine.UI;  

public class ScoreCounter : MonoBehaviour
{

    public Text InnerScoreText;
    int InnerScoreCounter;

    void Start()
    {
        InnerScoreCounter = UserInputController.innerCount_input;
        InnerScoreText = GetComponent<Text>();
        
    }

    void Update()
    {
        // convert to string, output as text in scene
        InnerScoreText.text = InnerScoreCounter.ToString();
    }
}