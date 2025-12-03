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
    [SerializeField] private GameObject chatInstance;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject messageInstance;
    [SerializeField] private GameObject messageContent;

    [Header("Local Data")]
    private GameObject messageAssessmentScreen;
    [SerializeField] private GameObject viewHolder;
    [SerializeField] private string currentState;
    [SerializeField] private string lastState;
    [SerializeField] private Button messagesButton;
    [SerializeField] private Button contactsButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private List<ChatContentTemplate> activeChats = new List<ChatContentTemplate>(); // chat:content

    private void Awake()
    {
        currentState = "chatlist";
        messageContent.SetActive(false);
        
        // start of tech depth
        messageAssessmentScreen = viewHolder.transform.Find("MessageView").transform.Find("AssessmentScreen").gameObject;
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f);
        messageAssessmentScreen.SetActive(true);
        viewHolder.transform.Find("MessageView").transform.Find("Back").GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(ChatTransitionHandler(null)); });
        viewHolder.transform.Find("MessageView").transform.Find("AssessButton").GetComponent<Button>().onClick.AddListener(delegate { messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -96f, 0f); });
        messageAssessmentScreen.transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate { AssessmentHandler(); });
        messageAssessmentScreen.transform.Find("ReturnButton").GetComponent<Button>().onClick.AddListener(delegate { messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f); });
        // end of tech depth

        messagesButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("chatlist"));} );
        contactsButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("contacts"));} );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler("profile"));} );

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
                newChat.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(ChatTransitionHandler(newChat)); });
                MessageViewHandler(newChat);
            }
            else 
            {
                newChat.GetComponent<Image>().color = new Color32(177,177,177,255); 
                newChat.GetComponent<Button>().interactable = false;
            }
        }
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
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f);
        viewHolder.transform.Find("MessageView").transform.Find("AssessButton").gameObject.SetActive(false);
        
        // disable current chat TODO: this is temp?
        var activeChat = activeChats.FirstOrDefault(x => x.contentObject = viewHolder.transform.Find("MessageView").GetComponent<ScrollRect>().content.gameObject);
        activeChat.chatObject.transform.Find("Notice").gameObject.SetActive(false);
        activeChat.chatObject.GetComponent<Image>().color = new Color32(177,177,177,255); 
        activeChat.chatObject.GetComponent<Button>().interactable = false;  
        
        // return to chat page
        StartCoroutine(ChatTransitionHandler(null));
    }

    // TODO: Merge with page transition handler :)
    private IEnumerator ChatTransitionHandler(GameObject chatObject)
    {
        float time = 0f;
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = currentState == "chatlist" ? -1165f : 0f;
        lastState = currentState;
        currentState = currentState == "chatlist" ? "chatmessage" : "chatlist";
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f);
        
        // disable in app navbar before transitioning to message page
        if (currentState == "chatmessage")
        {
            messagesButton.gameObject.SetActive(false);
            contactsButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);
        }

        // disable all content pages, before animating in
        if (chatObject != null) activeChats.ForEach(x => x.contentObject.SetActive(false));

        // enable target content page, if exists
        if (chatObject != null)
        {
            var targetChat = activeChats.FirstOrDefault(x => x.chatObject == chatObject);
            viewHolder.transform.Find("MessageView").GetComponent<ScrollRect>().content = targetChat.contentObject.GetComponent<RectTransform>();
            targetChat.contentObject.SetActive(true);
            // set chat title
            viewHolder.transform.Find("MessageView").transform.Find("Sender").GetComponent<TMP_Text>().text = chatObject.transform.Find("Sender").GetComponent<TMP_Text>().text;
            // make sure assess button is active
            viewHolder.transform.Find("MessageView").transform.Find("AssessButton").gameObject.SetActive(true);
        }

        // animate chat page
        while (time < gameManager.animationSpeed)
        {
            time += Time.deltaTime;
            viewHolder.GetComponent<RectTransform>().transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(targetPositionX, originalPosition.y, originalPosition.z), time / gameManager.animationSpeed);
            yield return null;
        }
        viewHolder.GetComponent<RectTransform>().transform.localPosition = new Vector3(targetPositionX, originalPosition.y, originalPosition.z);
        
        // disable content pages, after animating out
        if (chatObject == null) activeChats.ForEach(x => x.contentObject.SetActive(false));
        
        // enable in app navbar after transitioning to chat page
        if (currentState == "chatlist")
        {
            messagesButton.gameObject.SetActive(true);
            contactsButton.gameObject.SetActive(true);
            profileButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator PageTransitionHandler(string targetPage)
    {
        // don't try to transition to the same page
        if (currentState == targetPage) yield break;

        float time = 0f;
        lastState = currentState;
        currentState = targetPage;
        float targetPositionX = currentState == "chatlist" ? 0f : currentState == "contacts" ? -2330f : -3518f;
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
        switch (currentState)
        {
            case "chatmessage":
            StartCoroutine(ChatTransitionHandler(null));
            break;
            case "contacts":
            StartCoroutine(PageTransitionHandler(lastState));
            break;
            case "profile":
            StartCoroutine(PageTransitionHandler(lastState));
            break;
            case "chatlist":
            StartCoroutine(gameManager.activeApp.GetComponent<AppManager>().ButtonInteractionHandler(true));
            break;
            // TODO: handle the rest of the in app pages
        }
    }

}

[Serializable]
public class ChatContentTemplate
{
    public GameObject chatObject;
    public GameObject contentObject;
}