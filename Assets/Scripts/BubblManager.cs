using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BubblManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;

    [Header("Local Data")]
    [SerializeField] private GameObject postInstance;
    [SerializeField] private GameObject postContent;
    [SerializeField] private GameObject viewHolder;
    [SerializeField] private Button postsButton;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button profileButton;
    private string currentState;
    private string lastState;

    private void Awake()
    {
        // setup vars
        currentState = "posts";
        PostViewHandler();

        postsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("posts"));} );
        searchButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("search"));} );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("profile"));} );
    }

    private void PostViewHandler()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject newPost = Instantiate(postInstance, postInstance.transform.position, Quaternion.identity);
            newPost.name = "newPost";
            newPost.transform.SetParent(postContent.transform);
            newPost.transform.Find("User").gameObject.GetComponent<TMP_Text>().text = "User #" + i;
            int r = Random.Range(1,5);
            newPost.transform.Find("Info").gameObject.GetComponent<TMP_Text>().text = "Posted: " + r + (r > 1 ? " days ago." : " day ago.");
        }
    }

    private IEnumerator PageTransitionHandler(string targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;

        float time = 0f;
        lastState = currentState;
        currentState = targetPage;
        float targetPositionX = currentState == "posts" ? 0f : currentState == "search" ? -1165f : -2330f;
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        
        // animate chat page
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            viewHolder.GetComponent<RectTransform>().transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(targetPositionX, originalPosition.y, originalPosition.z), time / gameManager.animationSpeed);
            yield return null;
        }
        viewHolder.GetComponent<RectTransform>().transform.localPosition = new Vector3(targetPositionX, originalPosition.y, originalPosition.z);
    }

    public void BackNavigationHandler()
    {
        // TODO: don't return to last state if we arrived from there aka end up exiting the app eventually
        switch (currentState)
        {
            case "posts":
            StartCoroutine(gameManager.activeApp.GetComponent<AppManager>().ButtonInteractionHandler(true));
            break;
            case "search":
            StartCoroutine(PageTransitionHandler(lastState));
            break;
            case "profile":
            StartCoroutine(PageTransitionHandler(lastState));
            break;
        }
    }
}