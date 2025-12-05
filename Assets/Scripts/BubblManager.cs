using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BubblManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AppManager appManager;

    [Header("Local Data")]
    [SerializeField] private string currentState;
    [SerializeField] private string lastState;
    private int lastPageHits;
    [SerializeField] private GameObject currentTargetPost;
    [SerializeField] private GameObject messageInstance;
    [SerializeField] private GameObject postInstance;
    [SerializeField] private GameObject postContent;
    [SerializeField] private GameObject commentContent;
    [SerializeField] private GameObject viewHolder;
    [SerializeField] private GameObject commentsView;
    [SerializeField] private Button postsButton;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private List<PostContentTemplate> activePosts = new List<PostContentTemplate>(); // chat:content

    private void Awake()
    {
        // setup vars
        currentState = "posts";
        commentsView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1904f, 0f);
        commentsView.SetActive(true);
        viewHolder.transform.Find("PostView").GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 40f, 0f);
        viewHolder.transform.Find("SearchView").GetComponent<RectTransform>().transform.localPosition = new Vector3(1166f, 40f, 0f);
        viewHolder.transform.Find("ProfileView").GetComponent<RectTransform>().transform.localPosition = new Vector3(2330f, 40f, 0f);

        // setup buttons
        postsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("posts")); lastPageHits = 0; } );
        searchButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("search")); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("profile")); lastPageHits = 0; } );

        // generate postsview
        PostViewHandler();
    }

    private void PostViewHandler()
    {
        int noticeCount = 0;
        for (int i = 0; i < 10; i++)
        {
            GameObject newPost = Instantiate(postInstance, postInstance.transform.position, Quaternion.identity);
            newPost.name = "newPost";
            newPost.transform.SetParent(postContent.transform);
            newPost.transform.Find("User").gameObject.GetComponent<TMP_Text>().text = "User #" + i;
            int r = UnityEngine.Random.Range(1,5);
            newPost.transform.Find("Info").gameObject.GetComponent<TMP_Text>().text = "Posted: " + r + (r > 1 ? " days ago." : " day ago.");
            
            // generate openable posts
            if ((UnityEngine.Random.Range(0,2) == 1 || i > 4) && noticeCount < 3) 
            {
                noticeCount++;
                newPost.transform.Find("Notice").gameObject.SetActive(true);
                newPost.GetComponent<Button>().onClick.AddListener(delegate { PostInteractionHandler(newPost); lastPageHits = 0; });
                MessageViewHandler(newPost);
            }
            else 
            {
                newPost.GetComponent<Image>().color = new Color32(177,177,177,255); 
                newPost.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void MessageViewHandler(GameObject postObject)
    {
        GameObject newContent = Instantiate(commentContent, commentContent.transform.position, Quaternion.identity);
        newContent.name = "Content";
        newContent.SetActive(false);
        newContent.transform.SetParent(commentContent.transform.parent);
        activePosts.Add(new PostContentTemplate {postObject = postObject, contentObject = newContent});
        for (int i = 0; i < 10; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newChat";
            newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = postObject.transform.Find("User").GetComponent<TMP_Text>().text;
            newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newMessage.transform.SetParent(newContent.transform);
        }
    }

    private void PostInteractionHandler(GameObject newPost)
    {
        currentTargetPost = newPost;
        StartCoroutine(PageTransitionHandler("commentsView"));
    }

    // TODO: REFRACT THIS SHIT
    private IEnumerator PageTransitionHandler(string targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;
        
        bool isCommentsActive = commentsView.GetComponent<RectTransform>().transform.localPosition == new Vector3(0f, -256f, 0f);
        // prevent interaction with posts while comments are up
        if (isCommentsActive && targetPage != "posts") yield break;

        float time = 0f;
        lastState = currentState;
        currentState = targetPage;
        GameObject targetObject = targetPage == "commentsView" || isCommentsActive ? commentsView : viewHolder;
        if (targetPage == "commentsView") commentsView.transform.SetParent(gameObject.transform);
        else if (!isCommentsActive) commentsView.transform.SetParent(viewHolder.transform);
        Vector3 originalPosition = targetObject.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = targetPage == "posts" || targetPage == "commentsView" ? 0f : targetPage == "search" ? -1166f : -2330f;
        float targetPositionY = targetPage == "commentsView" ? -256f : targetPage == "posts" && isCommentsActive ? -1904f : 0f;

        viewHolder.transform.Find("PostView").gameObject.GetComponent<ScrollRect>().enabled = true;
        if (targetPage == "commentsView")
        {
            viewHolder.transform.Find("PostView").gameObject.GetComponent<ScrollRect>().enabled = false;
            activePosts.ForEach(x => x.contentObject.SetActive(false));
            var targetComments = activePosts.FirstOrDefault(x => x.postObject == currentTargetPost);
            commentsView.GetComponent<ScrollRect>().content = targetComments.contentObject.GetComponent<RectTransform>();
            targetComments.contentObject.SetActive(true);
        }

        // animate page
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            targetObject.GetComponent<RectTransform>().transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(targetPositionX, targetPositionY, originalPosition.z), time / gameManager.animationSpeed);
            yield return null;
        }
        targetObject.GetComponent<RectTransform>().transform.localPosition = new Vector3(targetPositionX, targetPositionY, originalPosition.z);

    }

    public void BackNavigationHandler()
    {
        // bail if we are on chatlist or lastpagehit is reached
        if (lastPageHits > 2 || currentState == "posts") 
        { 
            lastPageHits = 0; 
            StartCoroutine(appManager.AppInteractionHandler(true)); 
            return;
        }
        
        // transition to last page
        lastPageHits++;
        StartCoroutine(PageTransitionHandler(lastState));
    }
}

[Serializable]
public class PostContentTemplate
{
    public GameObject postObject;
    public GameObject contentObject;
}