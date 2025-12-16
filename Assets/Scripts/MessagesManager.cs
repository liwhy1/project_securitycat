using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MessagesManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AppManager appManager;
    [SerializeField] private GameObject chatInstance;
    [SerializeField] private GameObject messageInstance;

    [Header("Local Data")]
    [SerializeField] private GameObject currentState;
    [SerializeField] private GameObject lastState;
    private int lastPageHits;
    private bool isTransitioning;
    [SerializeField] private GameObject currentTargetChat;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject messageContent;
    [SerializeField] private GameObject viewHolder;
    public GameObject chatView;
    [SerializeField] private GameObject messageView;
    [SerializeField] private GameObject profileView;
    [SerializeField] private Button messagesButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button messageBackButton;
    [SerializeField] private Button messageAssessButton;
    [SerializeField] private List<ChatContentTemplate> activeChats = new List<ChatContentTemplate>(); // chat:content

    private void Start()
    {
        // setup vars
        currentState = chatView;
        chatView.SetActive(true);
        messageView.SetActive(false);
        profileView.SetActive(false);
        messageContent.SetActive(false);
        messageAssessButton.gameObject.SetActive(true);

        // setup buttons
        messagesButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(chatView)); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(profileView)); lastPageHits = 0; } );
        messageBackButton.onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(chatView)); lastPageHits = 0; } );
        messageAssessButton.onClick.AddListener(delegate { gameManager.AssessmentVisibilityHandler(true); });

        // generate chatview
        ChatViewHandler();
    }

    private void ChatViewHandler()
    {
        int noticeCount = 0;
        for (int i = 0; i < 8; i++)
        {
            GameObject newChat = Instantiate(chatInstance, chatInstance.transform.position, Quaternion.identity);
            newChat.name = "newChat";
            newChat.transform.Find("Sender").GetComponent<TMP_Text>().text = "Sender #" + i;
            newChat.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newChat.transform.SetParent(chatContent.transform);
            newChat.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            // generate openable chats
            if ((UnityEngine.Random.Range(0,2) == 1 || i > 4) && noticeCount < 3) 
            {
                noticeCount++;
                newChat.transform.Find("Notice").gameObject.SetActive(true);
                newChat.GetComponent<Button>().onClick.AddListener(delegate { ChatInteractionHandler(newChat); lastPageHits = 0; } );
                MessageViewHandler(newChat);
            }
            else 
            {
                newChat.GetComponent<Image>().color = new Color32(177,177,177,255); 
                newChat.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void ChatInteractionHandler(GameObject newChat)
    {
        currentTargetChat = newChat;
        StartCoroutine(PageTransitionHandler(messageView));
    }

    private void MessageViewHandler(GameObject chatObject)
    {
        GameObject newContent = Instantiate(messageContent, messageContent.transform.position, Quaternion.identity);
        newContent.name = "Content";
        newContent.SetActive(false);
        newContent.transform.SetParent(messageContent.transform.parent);
        newContent.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        newContent.GetComponent<RectTransform>().sizeDelta = new Vector3(0f, 0f, 0f);
        newContent.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        activeChats.Add(new ChatContentTemplate {chatObject = chatObject, contentObject = newContent, isAssessed = false});
        //string[] currentChatContent = gameManager.chatBlocks[2].Split(";");
        //for (int i = 0; i < currentChatContent.Length-1; i++)
        for (int i = 0; i < 14; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newChat";
            newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = chatObject.transform.Find("Sender").GetComponent<TMP_Text>().text;
            newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            //newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = currentChatContent[i].Split(":")[0];
            //newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = currentChatContent[i].Split(":")[1];
            newMessage.transform.SetParent(newContent.transform);
            newMessage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
    }

    public void AssessmentHandler()
    {
        messageAssessButton.gameObject.SetActive(false);

        // disable current chat TODO: is this temp?
        var activeChat = activeChats.FirstOrDefault(x => x.contentObject == messageView.GetComponent<ScrollRect>().content.gameObject);
        activeChat.chatObject.transform.Find("Notice").gameObject.SetActive(false);
        activeChat.isAssessed = true;
        //activeChat.chatObject.GetComponent<Image>().color = new Color32(177,177,177,255);
        //activeChat.chatObject.GetComponent<Button>().interactable = false;
        
        // return to chat page
        //StartCoroutine(PageTransitionHandler(chatView));
    }

    private IEnumerator PageTransitionHandler(GameObject targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;

        // another transition already in progress
        if (isTransitioning) yield break;
        isTransitioning = true;
    
        float time = 0f;
        lastState = currentState;
        currentState = targetPage;

        // ensure only current and targetpage is visible
        PageVisibilityHandler();

        // setup transition vars
        chatView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 30f, 0f);
        targetPage.GetComponent<RectTransform>().transform.localPosition = targetPage == profileView || targetPage == messageView ? new Vector3(chatView.GetComponent<RectTransform>().rect.width, 30f, 0f) : targetPage.GetComponent<RectTransform>().transform.localPosition;
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = targetPage == chatView ? 0f : chatView.GetComponent<RectTransform>().rect.width * -1f;
        
        if (targetPage == messageView)
        {
            // handle gameobjects before transition
            messagesButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);
            messageView.SetActive(true);

            // handle target page before transition
            if (currentTargetChat == null) { Debug.Log("Well shit."); yield return null;}
            activeChats.ForEach(x => x.contentObject.SetActive(false));
            var targetChat = activeChats.FirstOrDefault(x => x.chatObject == currentTargetChat);
            messageView.GetComponent<ScrollRect>().content = targetChat.contentObject.GetComponent<RectTransform>();
            targetChat.contentObject.SetActive(true);
            messageView.transform.Find("Sender").GetComponent<TMP_Text>().text = currentTargetChat.transform.Find("Sender").GetComponent<TMP_Text>().text;
            if (targetChat.isAssessed) messageAssessButton.gameObject.SetActive(false);
            else messageAssessButton.gameObject.SetActive(true);
        }

        // animate chat page
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            viewHolder.GetComponent<RectTransform>().transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(targetPositionX, originalPosition.y, originalPosition.z), time / gameManager.animationSpeed);
            yield return null;
        }
        viewHolder.GetComponent<RectTransform>().transform.localPosition = new Vector3(targetPositionX, originalPosition.y, originalPosition.z);
        
        // handle gameobjects after transition
        if (targetPage == chatView)
        {
            messagesButton.gameObject.SetActive(true);
            profileButton.gameObject.SetActive(true);
            currentTargetChat = null;
        }

        // reset messageview position
        if (targetPage == messageView) StartCoroutine(appManager.ScrollViewResetHandler(messageView));

        // hide last state after transition finished
        lastState.SetActive(false);

        // free up transition state
        isTransitioning = false;
    }

    private void PageVisibilityHandler()
    {
        chatView.SetActive(false);
        messageView.SetActive(false);
        profileView.SetActive(false);
        gameManager.AssessmentVisibilityHandler(false);
        currentState.SetActive(true);
        lastState.SetActive(true);
    }

    public void BackNavigationHandler()
    {
        if (isTransitioning) return;

        // bail if we are on chatlist or lastpagehit is reached
        if (lastPageHits > 2 || currentState == chatView) 
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
public class ChatContentTemplate
{
    public GameObject chatObject;
    public GameObject contentObject;
    public bool isAssessed;
}