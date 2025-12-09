using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    [Header("Local App Data")]
    [SerializeField] private GameObject appIcon;
    public GameObject appTitle;
    [SerializeField] private GameObject appElements;
    [SerializeField] private GameObject appMask;
    public bool isPointerDown;
    public bool isPointerHovered;

    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        // setup vars
        appElements.SetActive(false);
        appElements.GetComponent<RectTransform>().localScale = new Vector3(0,0,1);
        appElements.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        appMask.GetComponent<Mask>().enabled = true;
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
            StartCoroutine(AppInteractionHandler(false));            
        }
    }

    public IEnumerator AppInteractionHandler(bool isExternal)
    {
        float time = 0f;
        while (isPointerDown)
        {
            // long press limit reached, follow mouse
            if (time > .55f)
            {
                // make sure app appears over other apps, but below persistentui
                gameObject.transform.SetSiblingIndex(gameManager.persistentObject.transform.GetSiblingIndex() - 1);
                // follow mouse position and highlight
                gameObject.GetComponent<RectTransform>().position = new Vector3(gameManager.pointerAction.ReadValue<Vector2>().x, gameManager.pointerAction.ReadValue<Vector2>().y, 0f);
                appIcon.GetComponent<Image>().color = Color.lightGray;
            }
            if (isPointerHovered) time += Time.deltaTime;
            yield return null;
        }
        // pointerup triggered earlier than long press limit reached or called from navbar
        if ((time < .55f && isPointerHovered) || isExternal)
        {
            // Handle transition
            if (gameManager.activeApp == gameObject)
            {
                StartCoroutine(TransitionAnimationHandler("out"));
            }
            else if (gameManager.activeApp == null)
            {
                appElements.transform.SetParent(gameManager.canvasObject);
                appElements.transform.SetParent(gameObject.transform);
                StartCoroutine(TransitionAnimationHandler("in"));
            }
            yield break;
        }
        // pointerup triggered after long press limit was reached, place icon on grid
        AppPlacementHandler();
    }

    private void AppPlacementHandler()
    {
        RectTransform closestGrid = gameManager.appArrayList[0].arrayObject.GetComponent<RectTransform>();
        float closestDistance = 99999;
        foreach (var app in gameManager.appArrayList)
        {
            GameObject currentGrid = app.arrayObject;
            // check if current grid has values, if it does, only allow placing if it is occupied by the same grid position to allow placing in the same spot
            if (currentGrid == null || (app.appObject != null && app.appObject != gameObject)) continue;
            float currentDistance = Vector3.Distance(currentGrid.GetComponent<RectTransform>().localPosition, gameObject.GetComponent<RectTransform>().localPosition);
            if (currentDistance <= closestDistance)
            {
                closestGrid = currentGrid.GetComponent<RectTransform>();
                closestDistance = currentDistance;
            }
        }

        // unassign app from old grid
        var appArrayOld = gameManager.appArrayList.FirstOrDefault(x => x.appObject == gameObject);
        appArrayOld.arrayObject.GetComponent<Image>().color = Color.white;
        appArrayOld.appObject = null;

        // assign app to grid
        var appArrayNew = gameManager.appArrayList.FirstOrDefault(x => x.arrayObject == closestGrid.gameObject);
        appArrayNew.arrayObject.GetComponent<Image>().color = Color.red;
        appArrayNew.appObject = gameObject;

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
        // move mask outside of app
        appElements.SetActive(true);
        appElements.name = gameObject.name + "Content";
        appElements.transform.SetParent(gameManager.canvasObject);

        // make sure persistent stays on top
        gameManager.persistentObject.transform.SetAsLastSibling();

        // reset mask to expected values
        Vector2 currentRectSize = animationType == "in" ? new Vector2(0, 0) : new Vector2(1, 1);
        Vector3 currentRectPosition = animationType == "in" ? transform.localPosition : new Vector3(0,0,0);
        appElements.GetComponent<RectTransform>().localScale = currentRectSize;
        appElements.GetComponent<RectTransform>().localPosition = currentRectPosition;

        // set target values based on animation type
        Vector2 targetSize = animationType == "in" ? new Vector2(1, 1) : new Vector2(0, 0);
        Vector3 targetPosition = animationType == "in" ? new Vector3(0,0,0) : transform.localPosition;
        gameManager.activeApp = animationType == "in" ? gameObject : null;

        // animate transition: app icon -> fullscreen app
        float time = 0f;
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            appElements.GetComponent<RectTransform>().localScale = Vector2.Lerp(currentRectSize, targetSize, time / gameManager.animationSpeed);
            appElements.GetComponent<RectTransform>().localPosition = Vector3.Lerp(currentRectPosition, targetPosition, time / gameManager.animationSpeed);
            yield return null;
        }

        // reset vars
        appElements.GetComponent<RectTransform>().localScale = targetSize;
        appElements.GetComponent<RectTransform>().localPosition = targetPosition;
        if (animationType == "out")
        {
            appElements.SetActive(false);
            appElements.name = "Elements";
            appElements.transform.SetParent(gameObject.transform);
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
        else if (appTitle.GetComponent<TMP_Text>().text == "Bubbl")
        {
            appElements.GetComponent<BubblManager>().BackNavigationHandler();
        }
        else
        {
            StartCoroutine(TransitionAnimationHandler("out"));
        }
    }
}