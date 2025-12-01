using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Input Manager")]
    private InputManager inputManager;
    private InputAction resetAction;
    public InputAction pointerAction;

    [Header("Reference Data")]
    public Transform canvasObject;
    public Transform persistentObject;

    [Header("Manager Data")]
    public string gameState;
    public float animationSpeed;
    public GameObject activeApp;
    [SerializeField] private GameObject gridInstance;
    [SerializeField] private GameObject gridHolder;
    public List<GameObject[]> appList = new List<GameObject[]>();
    [SerializeField] TMP_Text statusBarTime;
    [SerializeField] private GameObject appInstance;
    public List<NewAppTemplate> appData;

    private void OnEnable() => inputManager.Enable();
    private void OnDisable() => inputManager.Disable();

    private void Awake()
    {
        // setup input
        inputManager = new InputManager();
        resetAction = inputManager.Player.Reset;   
        resetAction.performed += ResetHandler;
        pointerAction = inputManager.Player.PointerPosition;   
        
        // setup vars
        gameState = "home";
        animationSpeed = .25f;
        appInstance.SetActive(false);
        gridInstance.SetActive(false);

        HomeGridHandler();
        StartCoroutine(StatusBarUpdateHandler());
        AppInitializationHandler();
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
        // initialize new apps
        foreach (var app in appData)
        {
            // apply variables to appmanager
            GameObject newApp = Instantiate(appInstance, new Vector3(0, 0, 0), Quaternion.identity);
            var currentManager = newApp.GetComponent<AppManager>();
            currentManager.appTitle.GetComponent<TMP_Text>().text = app.appName;
            currentManager.appLabel.GetComponent<TMP_Text>().text = app.appName;
            currentManager.appIcon.GetComponent<Image>().sprite = app.appIcon;
            GameObject newElements = Instantiate(app.appElements, new Vector3(0, 0, 0), Quaternion.identity);
            newElements.name = "Elements";
            app.appElements.SetActive(false);
            newElements.SetActive(true);
            newElements.transform.SetParent(currentManager.appContent.transform);
            newElements.transform.SetSiblingIndex(0);
            currentManager.appElements = newElements;
            newApp.SetActive(true);
            newApp.name = app.appName;
            newApp.transform.SetParent(canvasObject);

            // place new apps on the grid
            for (int i = 0; i < appList.Count; i++)
            {
                if (appList[i][0] == null)
                {
                    appList[i][0] = newApp;
                    appList[i][1].GetComponent<Image>().color = Color.red;
                    newApp.GetComponent<RectTransform>().position = appList[i][1].GetComponent<RectTransform>().position;
                    Debug.Log("App initialized: " + newApp.transform.Find("Label").GetComponent<TMP_Text>().text + " at: " + appList[i][1].name);
                    break;
                }
            }
        }
        persistentObject.transform.SetAsLastSibling();
    }

    private void ResetHandler(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("MAIN");
    }

    private void HomeGridHandler()
    {
        // generate a 4x7 grid
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameObject newGrid = Instantiate(gridInstance, new Vector3(gridInstance.transform.position.x + 230 * j, gridInstance.transform.position.y - 220 * i, gridInstance.transform.position.z), Quaternion.identity);
                newGrid.name = "Grid" + j + i;
                newGrid.transform.SetParent(gridHolder.transform);
                appList.Add(new GameObject[] {null, newGrid});
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
public class NewAppTemplate
{
    public string appName;
    public Sprite appIcon;
    public GameObject appElements;
}
