using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    [Header("Input Manager")]
    private InputManager inputManager;
    private InputAction resetAction;
    public InputAction pointerAction;

    [Header("Reference Data")]
    public Transform canvasObject;
    public Transform canvasLimiterObject;
    public Transform startAnimationBlocker;
    public Transform startAnimationIcon;
    public Transform persistentObject;
    public TextAsset appContents;
    public TextAsset appContentsDU;
    public TextAsset currentAppContents;
    [SerializeField] private Sprite mascotType1;
    [SerializeField] private Sprite mascotType2;
    [SerializeField] private Sprite mascotType3;
    [SerializeField] private Sprite mascotType4;

    [Header("Local Data")]
    public float animationSpeed;
    public GameObject activeApp;
    private float screenWidth;
    [SerializeField] private GameObject gridInstance;
    [SerializeField] private GameObject gridHolder;
    [SerializeField] TMP_Text statusBarTime;
    public GameObject assessmentElements;
    [SerializeField] private Button assessmentReturnButton;
    public Button assessmentConfirmButton;
    public Button assessmentDangerous;
    public Button assessmentAttention;
    public Button assessmentNeutral;
    [SerializeField] private TMP_Text assessmentTitle;
    [SerializeField] private TMP_Text assessmentExplanation;
    [SerializeField] private string assessmentCurrentSelection;
    public List<AppListTemplate> appArrayList = new List<AppListTemplate>();
    public List<GameObject> appList;
    public List<string> chatBlocks;
    public List<string> postBlocks;
    public List<string> mailBlocks;
    [SerializeField] private int assessmentCorrect;
    [SerializeField] private int assessmentIncorrect;
    [SerializeField] private bool playStartAnimation;
    public string selectedLocalization;
    [SerializeField] private GameObject backgroundMascot;
    public int openableContent;

    private void OnEnable() => inputManager.Enable();
    private void OnDisable() => inputManager.Disable();
    private void ResetHandler(InputAction.CallbackContext context) => SceneManager.LoadScene("MAIN");

    private void Awake()
    {
        // setup input
        inputManager = new InputManager();
        resetAction = inputManager.Player.Reset;   
        resetAction.performed += ResetHandler;
        pointerAction = inputManager.Player.PointerPosition;   
        
        // setup vars
        animationSpeed = .25f;
        screenWidth = canvasObject.GetComponent<RectTransform>().rect.width;
        canvasLimiterObject.GetComponent<Mask>().enabled = true;
        gridInstance.SetActive(false);
        selectedLocalization = "en";
        AssessmentVisibilityHandler(false);
        StartCoroutine(StatusBarUpdateHandler());
        StartCoroutine(ScreenWidthUpdateHandler());

        // apply localization
        LocalizationHandler();

        // parse content file
        AppContentParseHandler();

        // setup buttons
        assessmentReturnButton.onClick.AddListener(delegate { AssessmentVisibilityHandler(false); });
        assessmentConfirmButton.onClick.AddListener(delegate { activeApp.GetComponent<AppManager>().AssessmentHandler(); });
        assessmentDangerous.onClick.AddListener(delegate { AssessmentButtonHandler("dangerous"); });
        assessmentAttention.onClick.AddListener(delegate { AssessmentButtonHandler("needs attention"); });
        assessmentNeutral.onClick.AddListener(delegate { AssessmentButtonHandler("neutral"); });

        // start animation
        StartCoroutine(StartAnimationHandler());
    }

    public void LocalizationHandler()
    {
        if (selectedLocalization == "en")
        {
            currentAppContents = appContents;
            assessmentConfirmButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "Confirm";
            assessmentReturnButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "Return";
        }
        else
        {
            // TODO: finish localization data
            currentAppContents = appContentsDU;
            assessmentConfirmButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "Confirm";
            assessmentReturnButton.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = "Return";
        }
    }

    private void ScoreHandler(string scoreType)
    {
        if (scoreType == "correct")
        {
            assessmentCorrect++;
        }
        else
        {
            assessmentIncorrect++;
        }
        if (assessmentCorrect-assessmentIncorrect < 0)
        {
            backgroundMascot.GetComponent<Image>().sprite = mascotType2;
        }
        else if (assessmentCorrect-assessmentIncorrect < -2)
        {
            backgroundMascot.GetComponent<Image>().sprite = mascotType3;
        }
        else if (assessmentCorrect-assessmentIncorrect < -4)
        {
            backgroundMascot.GetComponent<Image>().sprite = mascotType4;
        }
        else
        {
            backgroundMascot.GetComponent<Image>().sprite = mascotType1;
        }
    }

    private void AppContentParseHandler()
    {
        string cleanLines = "";
        // sanitize input
        foreach (var rawLine in currentAppContents.text.Split("\n"))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;
            cleanLines += line;
        }
        // divide content
        chatBlocks = Regex.Matches(cleanLines, @":chat:(.*?):chat:").Select(m => m.Groups[1].Value).ToList();
        postBlocks = Regex.Matches(cleanLines, @":post:(.*?):post:").Select(m => m.Groups[1].Value).ToList();
        mailBlocks = Regex.Matches(cleanLines, @":mail:(.*?):mail:").Select(m => m.Groups[1].Value).ToList();
    }

    private IEnumerator StatusBarUpdateHandler()
    {
        while (true)
        {
            statusBarTime.text = DateTime.Now.ToString().Split(" ")[1].Split(":")[0] + ":" + DateTime.Now.ToString().Split(" ")[1].Split(":")[1] + " " + DateTime.Now.ToString().Split(" ")[2];

            // refresh every 10 seconds
            yield return new WaitForSeconds(10f);      
        }
    }

    private IEnumerator ScreenWidthUpdateHandler()
    {
        while (true)
        {
            if (screenWidth != canvasObject.GetComponent<RectTransform>().rect.width)
            {
                Debug.Log("Re-evaluating screen size! " + screenWidth + "->" + canvasObject.GetComponent<RectTransform>().rect.width);
                screenWidth = canvasObject.GetComponent<RectTransform>().rect.width;
                var size = canvasLimiterObject.GetComponent<RectTransform>().sizeDelta;
                size.x = screenWidth > 1500f ? 1500f : canvasObject.GetComponent<RectTransform>().rect.width;
                canvasLimiterObject.GetComponent<RectTransform>().sizeDelta = size;
                HomeGridHandler();
            }
            // refresh every second
            yield return new WaitForSeconds(1f);
        }
    }

    public void AssessmentVisibilityHandler(bool assessmentActivity)
    {
        if (assessmentActivity)
        {
            assessmentElements.SetActive(true);
            assessmentConfirmButton.gameObject.SetActive(true);
            assessmentReturnButton.gameObject.SetActive(true);
            // reset assessment buttons
            assessmentDangerous.gameObject.SetActive(true);
            assessmentDangerous.gameObject.GetComponent<Image>().enabled = false;
            assessmentAttention.gameObject.SetActive(true);
            assessmentAttention.gameObject.GetComponent<Image>().enabled = false;
            assessmentNeutral.gameObject.SetActive(true);
            assessmentNeutral.gameObject.GetComponent<Image>().enabled = true;
            assessmentCurrentSelection = "neutral";
            assessmentElements.transform.SetParent(activeApp.GetComponent<AppManager>().appElements.transform);
        }
        else
        {
            assessmentElements.SetActive(false);
            assessmentExplanation.gameObject.SetActive(false);
            assessmentElements.transform.SetParent(canvasLimiterObject);
            assessmentElements.transform.SetSiblingIndex(persistentObject.GetSiblingIndex() - 1);
        }
        if (activeApp != null)
        {
            string contentType = activeApp.name.Contains("Messages") ? "chat" : activeApp.name.Contains("GoodMail") ? "mail" : activeApp.name.Contains("Bubbl") ? "post" : "current";
            assessmentTitle.text = "Assess results from " + contentType + " content!";
        }
        assessmentElements.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -155.5f, 0f);
        assessmentElements.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
    }

    public void AssessmentButtonHandler(string targetState)
    {
        assessmentDangerous.gameObject.GetComponent<Image>().enabled = false;
        assessmentAttention.gameObject.GetComponent<Image>().enabled = false;
        assessmentNeutral.gameObject.GetComponent<Image>().enabled = false;
        assessmentCurrentSelection = targetState;

        switch (targetState)
        {
            case "dangerous":
                assessmentDangerous.gameObject.GetComponent<Image>().enabled = true;
            break;
            case "needs attention":
                assessmentAttention.gameObject.GetComponent<Image>().enabled = true;
            break;
            case "neutral":
                assessmentNeutral.gameObject.GetComponent<Image>().enabled = true;
            break;
        }
    }

    public void AssessmentResultHandler(string userAnswer, string answerExplanation)
    {
        bool assessmentCorrectResult = false;
        try
        {
            assessmentCorrectResult = assessmentCurrentSelection.ToLower() == userAnswer.ToLower();
        }
        catch {}
        if (!assessmentCorrectResult)
        {
            assessmentTitle.text = "Incorrect answer!";
            assessmentExplanation.gameObject.SetActive(true);
            assessmentExplanation.text = answerExplanation;
            ScoreHandler("incorrect");
        }
        else
        {
            assessmentTitle.text = "Correct answer!";
            ScoreHandler("correct");
        }
    }

    public void AppInitializationHandler()
    {
        foreach (var app in appList)
        {
            // place new app on the next empty grid
            var appArray = appArrayList.FirstOrDefault(x => x.appObject == null);
            appArray.appObject = app;
            appArray.arrayObject.GetComponent<Image>().color = Color.red;
            app.GetComponent<RectTransform>().position = appArray.arrayObject.GetComponent<RectTransform>().position;
            Debug.Log("App initialized: " + app.transform.Find("Label").GetComponent<TMP_Text>().text + " at: " + appArray.arrayObject.name);
        }
        persistentObject.transform.SetAsLastSibling();
    }

    private void HomeGridHandler()
    {
        appArrayList.Clear();
        foreach (Transform child in gridHolder.transform) Destroy(child.gameObject);
        gridInstance.GetComponent<RectTransform>().localPosition = new Vector3(150f - canvasLimiterObject.GetComponent<RectTransform>().rect.width / 2f, canvasLimiterObject.GetComponent<RectTransform>().rect.height/ 2f -200f);

        // generate grid
        for (int i = 0; i < canvasLimiterObject.GetComponent<RectTransform>().rect.height / 280f; i++)
        {
            for (int j = 0; j < canvasLimiterObject.GetComponent<RectTransform>().rect.width / 225f; j++)
            {
                GameObject newGrid = Instantiate(gridInstance, new Vector3(0f, 0f, 1f), Quaternion.identity);
                newGrid.transform.SetParent(gridHolder.transform);
                newGrid.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                newGrid.GetComponent<RectTransform>().localPosition = new Vector2(gridInstance.GetComponent<RectTransform>().localPosition.x + 200f * j, gridInstance.GetComponent<RectTransform>().localPosition.y - 220f * i);
                newGrid.name = "Grid" + i + j;
                appArrayList.Add(new AppListTemplate {appObject = null, arrayObject = newGrid});
                newGrid.SetActive(false);
            }
        }
        Debug.Log("HomeGrid generated!");
        AppInitializationHandler();
    }

    private IEnumerator StartAnimationHandler()
    {
        if (!playStartAnimation)
        {
            Destroy(startAnimationBlocker.gameObject);
            Destroy(startAnimationIcon.gameObject);
            yield break;
        }
        startAnimationBlocker.gameObject.SetActive(true);
        startAnimationIcon.gameObject.SetActive(true);
        canvasLimiterObject.SetParent(startAnimationBlocker);
        startAnimationIcon.SetParent(canvasObject);
        startAnimationIcon.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        yield return new WaitForSeconds(.7f);

        // animate icon
        float currentAlpha;
        float animSpeed = .95f;
        float time = 0f;
        while (time < animSpeed)
        {
            time += Time.deltaTime;
            currentAlpha = Mathf.Lerp(0f, 255f, time / animSpeed);
            startAnimationIcon.GetComponent<Image>().color = new Color32(255, 255, 255, Convert.ToByte(currentAlpha));
            yield return null;
        }
        startAnimationIcon.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        
        yield return new WaitForSeconds(.35f);
        var savedSize = canvasLimiterObject.GetComponent<RectTransform>().sizeDelta;
        startAnimationBlocker.GetComponent<RectTransform>().sizeDelta = new Vector3(0f, 0f, 0f);
        
        // animate mask
        var savedSizeParented = startAnimationBlocker.GetComponent<RectTransform>().sizeDelta;
        Vector2 targetSize = new Vector2(3000f, 3000f);
        time = 0f;
        animSpeed = .65f;
        while (time < animSpeed)
        {
            time += Time.deltaTime;
            startAnimationBlocker.GetComponent<RectTransform>().sizeDelta = Vector3.Lerp(savedSizeParented, targetSize, time / animSpeed);
            yield return null;
        }
        startAnimationBlocker.GetComponent<RectTransform>().localPosition = targetSize;
        canvasLimiterObject.SetParent(canvasObject);
        canvasLimiterObject.GetComponent<RectTransform>().sizeDelta = savedSize;
        canvasLimiterObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 1f);
        Destroy(startAnimationBlocker.gameObject);
        Destroy(startAnimationIcon.gameObject);
    }

    public void NavigationInteractionHandler(GameObject navigatorObject)
    {
        // handle navigation actions
        if (activeApp != null && navigatorObject.GetComponent<Image>().color.a != 0)
        {
            if (navigatorObject.name.Contains("Back"))
            {
                // call app specific back navigation
                activeApp.GetComponent<AppManager>().BackNavigationHandler();
            }
            else if (navigatorObject.name.Contains("Home"))
            {
                assessmentElements.SetActive(false);
                StartCoroutine(activeApp.GetComponent<AppManager>().TransitionAnimationHandler("out"));
            }
        }

        // handle button color
        Color newColor = navigatorObject.GetComponent<Image>().color;
        newColor.a = newColor.a == 0 ? .5f : 0; 
        navigatorObject.GetComponent<Image>().color = newColor;
    }
}

[Serializable]
public class AppListTemplate
{
    public GameObject appObject;
    public GameObject arrayObject;
}
