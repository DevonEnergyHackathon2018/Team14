using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NameCatcher : MonoBehaviour
{
    public Text NameText;
    string Name;

    void Start()
    {
        Name = UserInputController.names;
        NameText = GetComponent<Text>();
    }

    void Update()
    {
        // convert to string, output as text in scene
        NameText.text = NameText.ToString();
    }
}