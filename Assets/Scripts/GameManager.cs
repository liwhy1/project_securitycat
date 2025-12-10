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
    public Transform persistentObject;
    public TextAsset messagesChats;

    [Header("Local Data")]
    public float animationSpeed;
    public GameObject activeApp;
    [SerializeField] private GameObject gridInstance;
    [SerializeField] private GameObject gridHolder;
    [SerializeField] TMP_Text statusBarTime;
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
        gridInstance.SetActive(false);

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 144;

        HomeGridHandler();
        StartCoroutine(StatusBarUpdateHandler());
        AppInitializationHandler();
        AppContentParseHandler();
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
            yield return new WaitForSeconds(10);            
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
        // generate a 4x7 grid
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameObject newGrid = Instantiate(gridInstance, new Vector3(gridInstance.transform.position.x + 215 * j, gridInstance.transform.position.y - 220 * i, gridInstance.transform.position.z), Quaternion.identity);
                newGrid.name = "Grid" + i + j;
                newGrid.transform.SetParent(gridHolder.transform);
                appArrayList.Add(new AppListTemplate {appObject = null, arrayObject = newGrid});
                newGrid.SetActive(false);
            }
        }
        Destroy(gridInstance);
        Debug.Log("HomeGrid generated!");
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
