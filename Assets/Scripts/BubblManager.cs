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
    [SerializeField] private Sprite artPost;
    [SerializeField] private Sprite chessPost;
    [SerializeField] private Sprite hairPost;
    [SerializeField] private Sprite footballPost;
    [SerializeField] private Sprite gymPost;
    [SerializeField] private Sprite theaterPost;
    [SerializeField] private Sprite messageIcon;

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
    [SerializeField] private Button postAssessButton;
    [SerializeField] private List<PostContentTemplate> activePosts = new List<PostContentTemplate>(); // post:content

    private void Start()
    {
        // setup vars
        currentState = postView;
        postView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 30f, 0f);
        commentsView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1904f, 0f);
        profileView.GetComponent<RectTransform>().transform.localPosition = new Vector3(postView.GetComponent<RectTransform>().rect.width, 40f, 0f);
        postView.SetActive(true);
        commentsView.SetActive(false);
        profileView.SetActive(false);
        commentContent.SetActive(false);
        postAssessButton.gameObject.SetActive(true);

        // setup buttons
        postsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(postView)); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(profileView)); lastPageHits = 0; } );
        commentsBackButton.onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(postView)); lastPageHits = 0; } );
        postAssessButton.onClick.AddListener(delegate { gameManager.AssessmentVisibilityHandler(true); });

        // generate postsview
        PostViewHandler();

        // apply language preference
        LanguageHandler();
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
            int r = UnityEngine.Random.Range(1,5);
            if (gameManager.selectedLocalization == "en")
            {
                newPost.transform.Find("User").gameObject.GetComponent<TMP_Text>().text = "New post #" + i;
                newPost.transform.Find("Info").gameObject.GetComponent<TMP_Text>().text = "Posted: " + r + (r > 1 ? " days ago." : " day ago.");
            }
            else
            {
                newPost.transform.Find("User").gameObject.GetComponent<TMP_Text>().text = "Nieuw bericht #" + i;
                newPost.transform.Find("Info").gameObject.GetComponent<TMP_Text>().text = r + (r > 1 ? " dagen" : " dag") + " geleden geplaatst." ;
            }

            // generate openable posts
            if ((UnityEngine.Random.Range(0,2) == 1 || i > 4) && noticeCount < gameManager.openableContent && gameManager.postBlocks.Count > 0) 
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
        // Prepare new post
        int randomPost = UnityEngine.Random.Range(0, gameManager.postBlocks.Count);
        string[] currentPostContent = gameManager.postBlocks[randomPost].Split(";");
        string currentTitle = currentPostContent[0].Split(":")[1];
        string currentResult = currentPostContent[1].Split(":")[1];
        string currentExplanation = currentPostContent[2].Split(":")[1];
        // Set title
        postObject.transform.Find("User").GetComponent<TMP_Text>().text = currentTitle;
        ImageLoadHandler(currentTitle, postObject.transform.Find("Image").GetComponent<Image>());
        // Remove used chat
        gameManager.postBlocks.Remove(gameManager.postBlocks[randomPost]);
        // Generate messages from new chat
        for (int i = 3; i < currentPostContent.Length-1; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newPost";
            newMessage.transform.Find("TextHolder").Find("Sender").GetComponent<TMP_Text>().text = currentPostContent[i].Split(":")[0];
            newMessage.transform.Find("TextHolder").Find("Message").GetComponent<TMP_Text>().text = currentPostContent[i].Split(":")[1];
            newMessage.transform.SetParent(newContent.transform);
            newMessage.transform.Find("Icon").GetComponent<Image>().sprite = messageIcon;
            newMessage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
        /*for (int i = 0; i < 10; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newChat";
            newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = postObject.transform.Find("User").GetComponent<TMP_Text>().text;
            newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newMessage.transform.SetParent(newContent.transform);
            newMessage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }*/
        activePosts.Add(new PostContentTemplate {postObject = postObject, contentObject = newContent, isAssessed = false, result = currentResult, explanation = currentExplanation});
    }

    // TODO: Hacky shit as per usual
    private void ImageLoadHandler(string postTitle, Image targetImage)
    {
        if (postTitle.Contains("Proberen voor de theaterclub") || postTitle.Contains("Trying out for the theater club"))
        {
            targetImage.sprite = theaterPost;
        }
        else if (postTitle.Contains("Begonnen met naar de sportschool gaan") || postTitle.Contains("Started going to the gym"))
        {
            targetImage.sprite = gymPost;
        }
        else if (postTitle.Contains("Gisteren zelf mijn haar geverfd") || postTitle.Contains("Dyed my hair myself last night"))
        {
            targetImage.sprite = hairPost;
        }
        else if (postTitle.Contains("Mijn kunst voor het eerst gepost") || postTitle.Contains("Tried posting my art for the first time"))
        {
            targetImage.sprite = artPost;
        }
        else if (postTitle.Contains("Posten dat ik een doelpunt heb gescoord") || postTitle.Contains("Posting that I scored a goal in"))
        {
            targetImage.sprite = footballPost;
        }
        else if (postTitle.Contains("Posten dat ik lid ben geworden van de school") || postTitle.Contains("Posting that I joined the school chess"))
        {
            targetImage.sprite = chessPost;
        }
    }

    private void PostInteractionHandler(GameObject newPost)
    {
        currentTargetPost = newPost;
        StartCoroutine(PageTransitionHandler(commentsView));
    }

    public void AssessmentHandler()
    {
        postAssessButton.gameObject.SetActive(false);

        var activePost = activePosts.FirstOrDefault(x => x.contentObject == commentsView.GetComponent<ScrollRect>().content.gameObject);
        activePost.postObject.transform.Find("Notice").gameObject.SetActive(false);
        activePost.isAssessed = true;
        gameManager.AssessmentResultHandler(activePost.result, activePost.explanation);

        //activePost.postObject.GetComponent<Image>().color = new Color32(177,177,177,255);
        //activePost.postObject.GetComponent<Button>().interactable = false;
        
        // return to post page
        //StartCoroutine(PageTransitionHandler(postView));
    }

    public void LanguageHandler()
    {
        profileView.transform.Find("User").gameObject.GetComponent<TMP_Text>().text = gameManager.generatedUsername;
        if (gameManager.selectedLocalization == "en")
        {
            profileView.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = "Profile";
            profileView.transform.Find("PostsText").gameObject.GetComponent<TMP_Text>().text = "Posts";
            profileView.transform.Find("FollowerText").gameObject.GetComponent<TMP_Text>().text = "Followers";
            profileView.transform.Find("FollowingText").gameObject.GetComponent<TMP_Text>().text = "Following";
        }
        else
        {
            profileView.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = "Profiel";
            profileView.transform.Find("PostsText").gameObject.GetComponent<TMP_Text>().text = "Berichten";
            profileView.transform.Find("FollowerText").gameObject.GetComponent<TMP_Text>().text = "Volgers";
            profileView.transform.Find("FollowingText").gameObject.GetComponent<TMP_Text>().text = "Volgend";
        }
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
        postView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 30f, 0f);
        targetPage.GetComponent<RectTransform>().transform.localPosition = targetPage == profileView ? new Vector3(postView.GetComponent<RectTransform>().rect.width, 30f, 0f) : targetPage.GetComponent<RectTransform>().transform.localPosition;
        GameObject targetObject = targetPage == commentsView || commentsView.activeSelf ? commentsView : viewHolder;
        Vector3 originalPosition = targetObject.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = targetPage == postView || targetPage == commentsView ? 0f : postView.GetComponent<RectTransform>().rect.width * -1;
        float targetPositionY = targetPage == commentsView ? 38f : targetPage == postView && commentsView.activeSelf ? -1904f : 0f;

        if (targetPage == commentsView)
        {
            // handle gameobjects before transition
            postsButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);

            // handle target page before transition
            activePosts.ForEach(x => x.contentObject.SetActive(false));
            var targetComments = activePosts.FirstOrDefault(x => x.postObject == currentTargetPost);
            commentsView.GetComponent<ScrollRect>().content = targetComments.contentObject.GetComponent<RectTransform>();
            targetComments.contentObject.SetActive(true);
            commentsView.transform.Find("Sender").GetComponent<TMP_Text>().text = currentTargetPost.transform.Find("User").GetComponent<TMP_Text>().text;
            if (targetComments.isAssessed) postAssessButton.gameObject.SetActive(false);
            else postAssessButton.gameObject.SetActive(true);
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
        }

        // reset commentsview position
        if (targetPage == commentsView) StartCoroutine(appManager.ScrollViewResetHandler(commentsView));
    
        // hide last state after transition finished
        lastState.SetActive(false);

        // free up transition state
        isTransitioning = false;
    }

    private void PageVisibilityHandler()
    {
        postView.SetActive(false);
        commentsView.SetActive(false);
        profileView.SetActive(false);
        gameManager.AssessmentVisibilityHandler(false);
        currentState.SetActive(true);
        lastState.SetActive(true);
    }

    public void BackNavigationHandler()
    {
        if (isTransitioning) return;

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
    public bool isAssessed;
    public string title;
    public string result;
    public string explanation;
}