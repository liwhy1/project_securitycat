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
    [SerializeField] private GameObject messageInstance;
    [SerializeField] private GameObject postInstance;

    [Header("Local Data")]
    [SerializeField] private GameObject currentState;
    [SerializeField] private GameObject lastState;
    private int lastPageHits;
    private bool isTransitioning;
    [SerializeField] private GameObject currentTargetPost;
    [SerializeField] private GameObject postContent;
    [SerializeField] private GameObject commentContent;
    [SerializeField] private GameObject viewHolder;
    public GameObject postView;
    [SerializeField] private GameObject profileView;
    [SerializeField] private GameObject commentsView;
    [SerializeField] private Button commentsBackButton;
    [SerializeField] private Button postsButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private GameObject messageAssessmentScreen;
    [SerializeField] private Button messageAssessButton;
    [SerializeField] private Button messageAssessConfirmButton;
    [SerializeField] private Button messageAssessReturnButton;
    [SerializeField] private List<PostContentTemplate> activePosts = new List<PostContentTemplate>(); // post:content

    private void Start()
    {
        // setup vars
        currentState = postView;
        postView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 40f, 0f);
        commentsView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1904f, 0f);
        profileView.GetComponent<RectTransform>().transform.localPosition = new Vector3(postView.GetComponent<RectTransform>().rect.width, 40f, 0f);
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -96f, 0f);
        postView.SetActive(true);
        commentsView.SetActive(false);
        profileView.SetActive(false);
        messageAssessmentScreen.SetActive(false);
        commentContent.SetActive(false);

        // setup buttons
        postsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(postView)); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(profileView)); lastPageHits = 0; } );
        commentsBackButton.onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(postView)); lastPageHits = 0; } );
        messageAssessButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(true); });
        messageAssessConfirmButton.onClick.AddListener(delegate { AssessmentHandler(); });
        messageAssessReturnButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(false); });

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
            newPost.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
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
        newContent.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        newContent.GetComponent<RectTransform>().sizeDelta = new Vector3(0f, 0f, 0f);
        newContent.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        activePosts.Add(new PostContentTemplate {postObject = postObject, contentObject = newContent});
        for (int i = 0; i < 10; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newChat";
            newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = postObject.transform.Find("User").GetComponent<TMP_Text>().text;
            newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newMessage.transform.SetParent(newContent.transform);
            newMessage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void PostInteractionHandler(GameObject newPost)
    {
        currentTargetPost = newPost;
        StartCoroutine(PageTransitionHandler(commentsView));
    }

    public void AssessmentHandler()
    {
        messageAssessButton.gameObject.SetActive(false);
        
        // disable current post TODO: is this temp?
        var activePost = activePosts.FirstOrDefault(x => x.contentObject == commentsView.GetComponent<ScrollRect>().content.gameObject);
        activePost.postObject.transform.Find("Notice").gameObject.SetActive(false);
        activePost.postObject.GetComponent<Image>().color = new Color32(177,177,177,255);
        activePost.postObject.GetComponent<Button>().interactable = false;
        
        // return to post page
        StartCoroutine(PageTransitionHandler(postView));
    }

    private IEnumerator PageTransitionHandler(GameObject targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;
        
        // prevent interaction with posts while comments are up
        if (commentsView.activeSelf && targetPage != postView) yield break;

        // another transition already in progress
        if (isTransitioning) yield break;
        isTransitioning = true;

        float time = 0f;
        lastState = currentState;
        currentState = targetPage;
        
        // ensure only current and targetpage is visible
        PageVisibilityHandler();
        
        // setup transition vars
        GameObject targetObject = targetPage == commentsView || commentsView.activeSelf ? commentsView : viewHolder;
        Vector3 originalPosition = targetObject.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = targetPage == postView || targetPage == commentsView ? 0f : postView.GetComponent<RectTransform>().rect.width * -1;
        float targetPositionY = targetPage == commentsView ? 38f : targetPage == postView && commentsView.activeSelf ? -1904f : 0f;

        if (targetPage == commentsView)
        {
            // handle gameobjects before transition
            messageAssessButton.gameObject.SetActive(true);
            appManager.appTitle.SetActive(false);
            postsButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);

            // disable all comment sections
            activePosts.ForEach(x => x.contentObject.SetActive(false));
            var targetComments = activePosts.FirstOrDefault(x => x.postObject == currentTargetPost);
            // set target comment sections
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
        
        // enable navbar & title when moving to postview
        if (targetPage == postView)
        {
            postsButton.gameObject.SetActive(true);
            profileButton.gameObject.SetActive(true);
            appManager.appTitle.SetActive(true);
        }
        
        // reset commentsview position
        if (targetPage == commentsView) commentsView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

        // free up transition state
        isTransitioning = false;

        // hide last state after transition finished, expect when switching between comments a posts
        if (lastState == postView && currentState == commentsView) yield break;
        lastState.SetActive(false);
    }

    private void PageVisibilityHandler()
    {
        postView.SetActive(false);
        commentsView.SetActive(false);
        profileView.SetActive(false);
        messageAssessmentScreen.SetActive(false);
        currentState.SetActive(true);
        lastState.SetActive(true);
    }

    public void BackNavigationHandler()
    {
        // bail if we are on postview or lastpagehit is reached
        if (lastPageHits > 2 || currentState == postView) 
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