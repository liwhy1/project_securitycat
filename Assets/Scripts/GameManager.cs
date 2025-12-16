using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Input Manager")]
    private InputManager inputManager;
    private InputAction resetAction;
    public InputAction pointerAction;

    [Header("Reference Data")]
    public Transform canvasObject;
    public Transform canvasLimiterObject;
    public Transform persistentObject;
    public TextAsset messagesChats;

    [Header("Local Data")]
    public float animationSpeed;
    public GameObject activeApp;
    private float screenWidth;
    [SerializeField] private GameObject gridInstance;
    [SerializeField] private GameObject gridHolder;
    [SerializeField] TMP_Text statusBarTime;
    public GameObject assessmentElements;
    [SerializeField] private Button assessmentReturnButton;
    [SerializeField] private Button assessmentConfirmButton;
    public List<AppListTemplate> appArrayList = new List<AppListTemplate>();
    public List<GameObject> appList;
    public List<string> chatBlocks;

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
        AssessmentVisibilityHandler(false);
        StartCoroutine(StatusBarUpdateHandler());
        StartCoroutine(ScreenWidthUpdateHandler());
        AppContentParseHandler();

        // setup buttons
        assessmentReturnButton.onClick.AddListener(delegate { AssessmentVisibilityHandler(false); });
        assessmentConfirmButton.onClick.AddListener(delegate { activeApp.GetComponent<AppManager>().AssessmentHandler(); });
    }

    private void AppContentParseHandler()
    {
        // chat messages
        string cleanLines = "";
        foreach (var rawLine in messagesChats.text.Split("\n"))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;
            cleanLines += line;

        }
        chatBlocks = cleanLines.Split(new[] { ":chat:" }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
                Debug.Log("Reevaluating screen size! " + screenWidth + "->" + canvasObject.GetComponent<RectTransform>().rect.width);
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
            assessmentElements.transform.SetParent(activeApp.GetComponent<AppManager>().appElements.transform);
        }
        else
        {
            assessmentElements.SetActive(false);
            assessmentElements.transform.SetParent(canvasLimiterObject);
            assessmentElements.transform.SetSiblingIndex(persistentObject.GetSiblingIndex() - 1);
        }
        if (activeApp != null)
        {
            string contentType = activeApp.name.Contains("Messages") ? "chat" : activeApp.name.Contains("GoodMail") ? "mail" : activeApp.name.Contains("Bubbl") ? "post" : "current";
            assessmentElements.transform.Find("Title").GetComponent<TMP_Text>().text = "Assess results from " + contentType +" content!";
        }
        assessmentElements.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -155.5f, 0f);
        assessmentElements.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
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
