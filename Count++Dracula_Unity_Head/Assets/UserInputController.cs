using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserInputController : MonoBehaviour
{
    //These can be set from the inspector and controlled from within the script
    public InputField InnerInput;
    public InputField OuterInput;
    public InputField NameInput;


    // It would be better to pass this reference through
    // the inspector instead of searching for it every time
    // an input changes
    public static int innerCount_input = -1;
    public static int outerCount_input = -1;

    public static float inner_average = 0;
    public static float outer_average = 0;

    public static float Guage = 0;
    public static string Area = "";
    public static string Pulled = "";
    
    public static string names;

    public static string dull_first;
    public static string dull_second;
    public static float dull_first_value;
    public static float dull_second_value;

    private void SubmitOuterInput(string input)
    {
        int result;
        if (int.TryParse(input, out result))
        {
            Debug.Log("outer in ");

            outerCount_input = result;
        }
    }

    private void SubmitInnerInput(string input)
    {
        int result1;
        if (int.TryParse(input, out result1))
        {
            Debug.Log("iiner in ");
            innerCount_input = result1;
        }
    }

    private void SubmitNameInput(string input)
    {
        string result2;
        result2 = input;
        names = result2;
    }

    private void Start()
    {
        InnerInput.onEndEdit.AddListener(SubmitInnerInput);
        OuterInput.onEndEdit.AddListener(SubmitOuterInput);
        NameInput.onEndEdit.AddListener(SubmitNameInput);
    }

}