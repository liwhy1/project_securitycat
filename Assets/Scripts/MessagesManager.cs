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
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject messageInstance;
    [SerializeField] private GameObject messageContent;

    [Header("Local Data")]
    [SerializeField] private string currentState;
    [SerializeField] private string lastState;
    private int lastPageHits;
    [SerializeField] private GameObject currentTargetChat;
    [SerializeField] private GameObject viewHolder;
    [SerializeField] private GameObject messageView;
    [SerializeField] private Button messagesButton;
    [SerializeField] private Button contactsButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button messageBackButton;
    [SerializeField] private GameObject messageAssessmentScreen;
    [SerializeField] private Button messageAssessButton;
    [SerializeField] private Button messageAssessConfirmButton;
    [SerializeField] private Button messageAssessReturnButton;
    [SerializeField] private List<ChatContentTemplate> activeChats = new List<ChatContentTemplate>(); // chat:content

    private void Awake()
    {
        // setup vars
        currentState = "chatlist";
        messageContent.SetActive(false);
        messageAssessmentScreen.SetActive(false);
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -96f, 0f);
        messageView.SetActive(false);
        messageView.GetComponent<RectTransform>().transform.localPosition = new Vector3(1166f, 26.909f, 0f);
        viewHolder.transform.Find("ChatView").GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 27f, 0f);
        viewHolder.transform.Find("ContactsView").GetComponent<RectTransform>().transform.localPosition = new Vector3(1166f, 39.6f, 0f);
        viewHolder.transform.Find("ProfileView").GetComponent<RectTransform>().transform.localPosition = new Vector3(2330f, 39.6f, 0f);

        // setup button
        messageBackButton.onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler("chatlist")); lastPageHits = 0; } );
        messageAssessButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(true); });
        messageAssessConfirmButton.onClick.AddListener(delegate { AssessmentHandler(); });
        messageAssessReturnButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(false); });
        messagesButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("chatlist")); lastPageHits = 0; } );
        contactsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("contacts")); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("profile")); lastPageHits = 0; } );

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
        StartCoroutine(PageTransitionHandler("chatmessage"));
    }

    private void MessageViewHandler(GameObject chatObject)
    {
        GameObject newContent = Instantiate(messageContent, messageContent.transform.position, Quaternion.identity);
        newContent.name = "Content";
        newContent.SetActive(false);
        newContent.transform.SetParent(messageContent.transform.parent);
        activeChats.Add(new ChatContentTemplate {chatObject = chatObject, contentObject = newContent});
        for (int i = 0; i < 10; i++)
        {
            GameObject newMessage = Instantiate(messageInstance, messageInstance.transform.position, Quaternion.identity);
            newMessage.name = "newChat";
            newMessage.transform.Find("Sender").GetComponent<TMP_Text>().text = chatObject.transform.Find("Sender").GetComponent<TMP_Text>().text;
            newMessage.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newMessage.transform.SetParent(newContent.transform);
        }
    }

    public void AssessmentHandler()
    {
        messageAssessButton.gameObject.SetActive(false);
        
        // disable current chat TODO: is this temp?
        var activeChat = activeChats.FirstOrDefault(x => x.contentObject = messageView.GetComponent<ScrollRect>().content.gameObject);
        activeChat.chatObject.transform.Find("Notice").gameObject.SetActive(false);
        activeChat.chatObject.GetComponent<Image>().color = new Color32(177,177,177,255); 
        activeChat.chatObject.GetComponent<Button>().interactable = false;  
        
        // return to chat page
        StartCoroutine(PageTransitionHandler("chatlist"));
    }

    private IEnumerator PageTransitionHandler(string targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;

        float time = 0f;
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        lastState = currentState;
        currentState = targetPage;
        float targetPositionX = targetPage == "chatlist" ? 0f : currentState == "contacts" || currentState == "chatmessage" ? -1166f : -2330f;
        
        if (targetPage == "chatmessage")
        {
            // handle gameobjects before transition
            messageAssessmentScreen.SetActive(false);
            messagesButton.gameObject.SetActive(false);
            contactsButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);
            messageView.SetActive(true);
            viewHolder.transform.Find("ContactsView").gameObject.SetActive(false);
            appManager.appTitle.SetActive(false);
            messageAssessButton.gameObject.SetActive(true);

            // handle target page before transition
            if (currentTargetChat == null) { Debug.Log("Well shit."); yield return null;}
            activeChats.ForEach(x => x.contentObject.SetActive(false));
            var targetChat = activeChats.FirstOrDefault(x => x.chatObject == currentTargetChat);
            messageView.GetComponent<ScrollRect>().content = targetChat.contentObject.GetComponent<RectTransform>();
            targetChat.contentObject.SetActive(true);
            messageView.transform.Find("Sender").GetComponent<TMP_Text>().text = currentTargetChat.transform.Find("Sender").GetComponent<TMP_Text>().text;
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
        if (targetPage == "chatlist")
        {
            messagesButton.gameObject.SetActive(true);
            contactsButton.gameObject.SetActive(true);
            profileButton.gameObject.SetActive(true);
            appManager.appTitle.SetActive(true);
            messageView.SetActive(false);
            viewHolder.transform.Find("ContactsView").gameObject.SetActive(true);
            currentTargetChat = null;
        }
    }

    public void BackNavigationHandler()
    {
        // bail if we are on chatlist or lastpagehit is reached
        if (lastPageHits > 2 || currentState == "chatlist") 
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
}