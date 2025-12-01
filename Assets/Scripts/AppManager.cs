using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    [Header("Local App Data")]
    private Vector2 originalRectSize;
    private Vector3 originalRectPosition;
    public GameObject appContent;
    public bool isPointerDown;
    public bool isPointerHovered;

    [Header("External App Data")]
    public GameObject appIcon;
    public GameObject appLabel;
    public GameObject appTitle;
    public GameObject appElements;

    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        // setup vars
        appContent.SetActive(false);
        appContent.GetComponent<Mask>().enabled = true;
        originalRectSize = appContent.GetComponent<RectTransform>().sizeDelta;
    }

    public void PointerCancelHandler(string cancelReason) 
    {
        // handle pointer cancel events
        if (cancelReason == "pointerexit")
        {
            isPointerHovered = false;
            appIcon.GetComponent<Image>().color = Color.white;
        }
        else if (cancelReason == "pointerup")
        {
            isPointerDown = false;
            appIcon.GetComponent<Image>().color = Color.white;
        }
    }
    public void PointerBeginHandler(string beginReason)
    {
        // handle pointer begin events
        if (beginReason == "pointerenter")
        { 
            isPointerHovered = true;
        }
        else if (beginReason == "pointerdown")
        {
            isPointerDown = true;
            StartCoroutine(ButtonInteractionHandler(false));            
        }
    }

    public IEnumerator ButtonInteractionHandler(bool isExternal)
    {
        float time = 0f;
        while (isPointerDown)
        {
            // long press limit reached, follow mouse
            if (time > .55f)
            {
                // make sure app appears over other apps
                gameObject.transform.SetSiblingIndex(gameManager.persistentObject.transform.GetSiblingIndex() - 1);
                // follow mouse position and highlight
                gameObject.GetComponent<RectTransform>().position = new Vector3(gameManager.pointerAction.ReadValue<Vector2>().x, gameManager.pointerAction.ReadValue<Vector2>().y, 0f);
                appIcon.GetComponent<Image>().color = Color.lightGray;
            }
            if (isPointerHovered) time += Time.deltaTime;
            yield return null;
        }
        // pointerup triggered earlier than long press limit reach or called from navbar
        if ((time < .55f && isPointerHovered) || isExternal)
        {
            // Handle transition
            if (gameManager.activeApp == gameObject)
            {
                StartCoroutine(TransitionAnimationHandler("out"));
            }
            else if (gameManager.activeApp == null)
            {
                appContent.transform.SetParent(gameManager.canvasObject);
                originalRectPosition = appContent.GetComponent<RectTransform>().localPosition;
                appContent.transform.SetParent(gameObject.transform);
                StartCoroutine(TransitionAnimationHandler("in"));
            }
            yield break;
        }
        // pointerup triggered after long press limit was reached, place icon on grid
        AppPlacementHandler();
    }

    private void AppPlacementHandler()
    {
        RectTransform closestGrid = gameManager.appList[0][1].GetComponent<RectTransform>();
        float closestDistance = 99999;
        foreach (var app in gameManager.appList)
        {
            GameObject currentGrid = app[1];
            // check if current grid has values, if it does, only allow placing if it is occupied by the same grid position to allow placing in the same spot
            if (currentGrid == null || (app[0] != null && app[0] != gameObject)) continue;
            float currentDistance = Vector3.Distance(currentGrid.GetComponent<RectTransform>().localPosition, gameObject.GetComponent<RectTransform>().localPosition);
            if (currentDistance <= closestDistance)
            {
                closestGrid = currentGrid.GetComponent<RectTransform>();
                closestDistance = currentDistance;
            }
        }

        // redundant if position stays the same
        foreach (var app in gameManager.appList)
        {
            // unassign app from old grid
            if (app[0] == gameObject)
            {
                app[1].GetComponent<Image>().color = Color.white;
                app[0] = null;
            }
            // assign app to grid
            if (app[1] == closestGrid.gameObject)
            {
                app[0] = gameObject;
                app[1].GetComponent<Image>().color = Color.red;
            }
        }
        // animate app position to new grid
        StartCoroutine(AppIconAnimationHandler(gameObject, closestGrid));
    }

    private IEnumerator AppIconAnimationHandler(GameObject iconObject, RectTransform targetObject)
    {
        float time = 0f;
        Vector3 originalPosition = iconObject.GetComponent<RectTransform>().localPosition;

        // animate to closest grid
        while (time < gameManager.animationSpeed * 1.5f)
        {
            time += Time.deltaTime;
            iconObject.GetComponent<RectTransform>().localPosition = Vector3.Lerp(originalPosition, targetObject.localPosition, time / gameManager.animationSpeed * 1.5f);
            yield return null;
        }
        iconObject.GetComponent<RectTransform>().localPosition = targetObject.localPosition;
    }
    
    public IEnumerator TransitionAnimationHandler(string animationType)
    {
        appContent.transform.SetParent(gameManager.canvasObject);
        gameManager.persistentObject.transform.SetAsLastSibling();

        appContent.SetActive(true);
        if (animationType == "in")
        {
            appContent.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
            appContent.GetComponent<RectTransform>().localPosition = transform.localPosition;
            gameManager.activeApp = gameObject;
        }

        Vector2 currentRectSize = appContent.GetComponent<RectTransform>().sizeDelta;
        Vector3 currentRectPosition = appContent.GetComponent<RectTransform>().localPosition;
        // set target values based on animation type
        Vector2 targetSize = animationType == "in" ? new Vector2(1080f, 1920f) : originalRectSize;
        Vector3 targetPosition = animationType == "in" ? new Vector3(0,0,0) : originalRectPosition;

        // animate transition: app icon -> fullscreen app
        float time = 0f;
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            appContent.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(currentRectSize, targetSize, time / gameManager.animationSpeed);
            appContent.GetComponent<RectTransform>().localPosition = Vector3.Lerp(currentRectPosition, targetPosition, time / gameManager.animationSpeed);
            yield return null;
        }
        appContent.GetComponent<RectTransform>().sizeDelta = targetSize;
        appContent.GetComponent<RectTransform>().localPosition = targetPosition;
        if (animationType == "out")
        {
            appContent.transform.SetParent(gameObject.transform);
            appContent.SetActive(false);
            gameManager.activeApp = null;
        }
    }

    public void BackNavigationHandler()
    {
        // handle back navigation for Messages
        if (appTitle.GetComponent<TMP_Text>().text == "Messages")
        {
            appElements.GetComponent<MessagesManager>().BackNavigationHandler();
        }
        else
        {
            StartCoroutine(TransitionAnimationHandler("out"));
        }
    }
}
