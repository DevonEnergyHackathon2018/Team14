using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TFClassify;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using TensorFlow;
using UnityEngine.SceneManagement;


public enum Mode
{
    Detect,
    Classify,
}

public class PhoneCamera : MonoBehaviour
{
    private const int detectImageSize = 300;
    private const int classifyImageSize = 224;

    private static Texture2D boxOutlineTexture;
    private static GUIStyle labelStyle;
    
    private bool camAvailable;
    private WebCamTexture backCamera;
    private Texture defaultBackground;
    
    private Classifier classifier;
    private Detector detector;

    private List<BoxOutline> boxOutlines;
    private Vector2 backgroundSize;
    private Vector2 backgroundOrigin;


    public Mode mode;
    public RawImage background;
    public AspectRatioFitter fitter;
    public TextAsset modelFile;
    public TextAsset labelsFile;
    public Text uiText;


    public int innerCount = 10;
    public int outerCount = 10;

    public int innerCaptureCount = 0;
    public int outerCaptureCount = 0;

    private float[] innerValues = { };
    private float[] outerValues = { };

    public bool captureInnerFlag = true;
    // UI
    public Text innerCountText;
    public Text outerCountText;

    public Text code;
    public Text conf;

    public Button InnerButton;
    public Button OuterButton;

    public Image wait;

    private void Start()
    {
        LoadWorker();

        wait.enabled = false;

        innerCount = UserInputController.innerCount_input;
        outerCount = UserInputController.outerCount_input;

        innerCountText.text = "Inner  : 0/" + innerCount.ToString();
        outerCountText.text = "Outer  : 0/" + outerCount.ToString();

        innerValues = new float[innerCount];
        outerValues = new float[outerCount];

        Button b = captureInnerFlag ? InnerButton : OuterButton;
        b.interactable = false;


        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;



        if(devices.Length == 0)
        {
            this.uiText.text = "No camera detected";
            camAvailable = false;

            return;
        }

        for(int i = 0; i < devices.Length; i++)
        {
            if(!devices[i].isFrontFacing)
            {
                this.backCamera = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }

        if(backCamera == null)
        {
            this.uiText.text = "Unable to find back camera";
            
            return;
        }


        this.backCamera.Play();
        this.background.texture = this.backCamera;
        this.backgroundSize = new Vector2(this.backCamera.width, this.backCamera.height);
        camAvailable = true;

        string func = mode == Mode.Classify ? nameof(TFClassify) : nameof(TFDetect);
      //  InvokeRepeating(func, 1f, 1f);
    }


    public void CaptureAction(Text label) {

        if (!wait.enabled)
        {
            wait.enabled = true;
            TFClassify();

        }


    }

    public void DoneAction() {
        // Report score to next scene here
        float ic = 0;
        for (int i = 0; i < innerCaptureCount; i++) {
            ic += innerValues[i];
        }
        ic = ic / innerCaptureCount;

        float oc = 0;
        for (int i = 0; i < outerCaptureCount; i++)
        {
            oc += outerValues[i];
        }
        oc = oc / outerCaptureCount;

        UserInputController.inner_average = ic;
        UserInputController.outer_average = oc;

        // 
        SceneManager.LoadScene("HelloAR");
    }



public void InnerOuterButtonToggle(bool inner) {
        if (inner) {
            InnerButton.interactable = false;
            OuterButton.interactable = true;
        }
        else {
            InnerButton.interactable = true;
            OuterButton.interactable = false;
        }
        captureInnerFlag = inner;
    }

    public void UndoAction() {
        if (captureInnerFlag && innerCaptureCount>0)
        {
            innerCaptureCount--;
            innerValues[innerCaptureCount] = 0; // assign value
            innerCountText.text = "Inner  : " + innerCaptureCount + "/" + innerCount.ToString();
        }
        else if(!captureInnerFlag && outerCaptureCount > 0) {
            outerCaptureCount--;
            outerValues[outerCaptureCount] = 0; // assign value
            outerCountText.text = "Outer  : " + outerCaptureCount + "/" + outerCount.ToString();
        }
    }

    private void Update()
    {
        if(!this.camAvailable)
        {
            return;
        }

        float ratio = (float)backCamera.width / (float)backCamera.height;
        fitter.aspectRatio = ratio;

        float scaleY = backCamera.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -backCamera.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);


    }
    

    public void OnGUI()
    {
        if (this.boxOutlines != null && this.boxOutlines.Any())
        {
            foreach (var outline in this.boxOutlines)
            {
                DrawBoxOutline(outline);
            }
        }
    }


    private void LoadWorker()
    {
        try
        {
            if (mode == Mode.Classify)
            {
                LoadClassifier();
            }
            else
            {
                LoadDetector();
            }
        }
        catch (TFException ex)
        {
            if (ex.Message.EndsWith("is up to date with your GraphDef-generating binary.)."))
            {
                this.uiText.text = "Error: TFException. Make sure you model trained with same version of TensorFlow as in Unity plugin.";
            }
            
            throw;
        }
    }


    private void LoadClassifier()
    {
        this.classifier = new Classifier(
            this.modelFile.bytes,
            Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
                .Where(s => !String.IsNullOrEmpty(s)).ToArray(),
            classifyImageSize);
    }


    private void LoadDetector()
    {
        this.detector = new Detector(
            this.modelFile.bytes,
            Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
                .Where(s => !String.IsNullOrEmpty(s)).ToArray(),
            detectImageSize);
    }


    private async void TFClassify()
    {
        var snap = TakeTextureSnap();
        var scaled = Scale(snap, classifyImageSize);
        var rotated = await RotateAsync(scaled.GetPixels32(), scaled.width, scaled.height);
        List<KeyValuePair<string,float>> probabilities = new List<KeyValuePair<string, float>>();
    string bigKey = "0"; float value = -1; int key = -1;
        try
        {
             probabilities = await this.classifier.ClassifyAsync(rotated);
            this.uiText.text = String.Empty;


            for (int i = 0; i < 3; i++)
            {
                this.uiText.text += probabilities[i].Key + ": " + String.Format("{0:0.000}%", probabilities[i].Value) + "\n";
            if (probabilities[i].Value > value) {
                key = int.Parse(probabilities[i].Key);
                    value = probabilities[i].Value;
            }
            }
        }
        catch (NullReferenceException)
        {
            this.uiText.text = "Error2: NullReferenceException. Make sure you set correct INPUT_NAME and OUTPUT_NAME";
        }
        finally
        {
        if (captureInnerFlag && innerCaptureCount < innerCount)
        {
            innerValues[innerCaptureCount] = key; // assign value
            innerCaptureCount++; // increment count
            innerCountText.text = "Inner  : " + innerCaptureCount + "/" + innerCount.ToString();

        }
        else if (!captureInnerFlag && outerCaptureCount < outerCount)
        {
            outerValues[outerCaptureCount] = key; // assign value
            outerCaptureCount++; // increment count
            outerCountText.text = "Outer  : " + outerCaptureCount + "/" + outerCount.ToString();
        }
            code.text = "Code  :  " + ( key == 0 ? 1 : key == 3 ?  4 : 8);
            conf.text = "Conf  :  " + value;
        //SaveToFile(scaled);
        Destroy(snap);
        Destroy(scaled);
        wait.enabled = false;
    }
    }


    private async void TFDetect()
    {
        UpdateBackgroundOrigin();

        var snap = TakeTextureSnap();
        var scaled = Scale(snap, detectImageSize);
        var rotated = await RotateAsync(scaled.GetPixels32(), scaled.width, scaled.height);
        this.boxOutlines = await this.detector.DetectAsync(rotated);

        Destroy(snap);
        Destroy(scaled);
    }

    
    private void UpdateBackgroundOrigin()
    {
        var backgroundPosition = this.background.transform.position;
        this.backgroundOrigin = new Vector2(backgroundPosition.x - this.backgroundSize.x / 2, 
                                            backgroundPosition.y - this.backgroundSize.y / 2);
    }


    private void DrawBoxOutline(BoxOutline outline)
    {
        var xMin = outline.XMin * this.backgroundSize.x + this.backgroundOrigin.x;
        var xMax = outline.XMax * this.backgroundSize.x + this.backgroundOrigin.x;
        var yMin = outline.YMin * this.backgroundSize.y + this.backgroundOrigin.y;
        var yMax = outline.YMax * this.backgroundSize.y + this.backgroundOrigin.y;

        DrawRectangle(new Rect(xMin, yMin, xMax - xMin, yMax - yMin), 4, Color.red);
        DrawLabel(new Rect(xMin + 10, yMin + 10, 200, 20), $"{outline.Label}: {(int)(outline.Score * 100)}%");
    }


    public static void DrawRectangle(Rect area, int frameWidth, Color color)
    {
        // Create a one pixel texture with the right color
        if (boxOutlineTexture == null)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            boxOutlineTexture = texture;
        }
        
        Rect lineArea = area;
        lineArea.height = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Top line

        lineArea.y = area.yMax - frameWidth; 
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Bottom line

        lineArea = area;
        lineArea.width = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Left line

        lineArea.x = area.xMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Right line
    }


    private static void DrawLabel(Rect position, string text)
    {
        if (labelStyle == null)
        {
            var style = new GUIStyle();
            style.fontSize = 50;
            style.normal.textColor = Color.red;
            labelStyle = style;
        }

        GUI.Label(position, text, labelStyle);
    }


    private Texture2D TakeTextureSnap()
    {
        var smallest = backCamera.width < backCamera.height ?
            backCamera.width : backCamera.height;
        var snap = TextureTools.CropWithRect(backCamera,
             new Rect(0, 0, 400, 400),
            TextureTools.RectOptions.Center, 0, 0);

        return snap;
    }


    private Texture2D Scale(Texture2D texture, int imageSize)
    {
        var scaled = TextureTools.scaled(texture, imageSize, imageSize, FilterMode.Bilinear);
        
        return scaled;
    }


    private Task<Color32[]> RotateAsync(Color32[] pixels, int width, int height)
    {
        return Task.Run(() =>
        {
            return TextureTools.RotateImageMatrix(
                    pixels, width, height, -90);
        });
    }

    private Task<Texture2D> RotateAsync(Texture2D texture)
    {
        return Task.Run(() =>
        {
            return TextureTools.RotateTexture(texture, -90);
        });
    }


    private void SaveToFile(Texture2D texture)
    {
        File.WriteAllBytes(
            Application.persistentDataPath + "/" +
            "snap.png", texture.EncodeToPNG());
        Debug.Log(Application.dataPath + "/" +
            "snap.png");
    }
}
