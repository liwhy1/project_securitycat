using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MessagesManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject chatInstance;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject messageInstance;
    [SerializeField] private GameObject messageContent;
    private GameObject messageAssessmentScreen;
    [SerializeField] private GameObject viewHolder;
    [SerializeField] private string currentState;
    [SerializeField] private string lastState;
    private List<GameObject[]> activeChats = new List<GameObject[]>(); // chat:content

    private void Awake()
    {
        currentState = "chatlist";
        messageContent.SetActive(false);
        
        // start of tech depth
        messageAssessmentScreen = viewHolder.transform.Find("MessageView").transform.Find("AssessmentScreen").gameObject;
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f);
        messageAssessmentScreen.SetActive(true);
        viewHolder.transform.Find("MessageView").transform.Find("Back").GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(null)); });
        viewHolder.transform.Find("MessageView").transform.Find("AssessButton").GetComponent<Button>().onClick.AddListener(delegate { messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -96f, 0f); });
        messageAssessmentScreen.transform.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(delegate { AssessmentHandler(); });
        messageAssessmentScreen.transform.Find("ReturnButton").GetComponent<Button>().onClick.AddListener(delegate { messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f); });
        // end of tech depth

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
            if ((Random.Range(0,2) == 1 || i > 4) && noticeCount < 3) 
            {
                noticeCount++;
                newChat.transform.Find("Notice").gameObject.SetActive(true);
                newChat.GetComponent<Button>().onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(newChat)); });
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
        activeChats.Add(new GameObject[] {chatObject, newContent});
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
        // disable chat TODO: this is temp
        foreach (var chat in activeChats)
        {
            // fetch current chat from active content window
            if (chat[1] == viewHolder.transform.Find("MessageView").GetComponent<ScrollRect>().content.gameObject)
            {
                chat[0].transform.Find("Notice").gameObject.SetActive(false);
                chat[0].GetComponent<Image>().color = new Color32(177,177,177,255); 
                chat[0].GetComponent<Button>().interactable = false;                
            }
        }
        // return to chat page
        StartCoroutine(PageTransitionHandler(null));
    }

    public IEnumerator PageTransitionHandler(GameObject chatObject)
    {
        float time = 0f;
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = currentState == "chatlist" ? -1165f : 0f;
        lastState = currentState;
        currentState = currentState == "chatlist" ? "chatmessage" : "chatlist";
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -1800f, 0f);
        // handle messagepage
        foreach (var chat in activeChats)
        {
            if (chatObject != null)
            {
                // disable all content windows
                chat[1].SetActive(false);
                if (chat[0] == chatObject)
                {
                    // enable window for target page, if exists
                    viewHolder.transform.Find("MessageView").GetComponent<ScrollRect>().content = chat[1].GetComponent<RectTransform>();
                    chat[1].SetActive(true);
                    // set chat title
                    viewHolder.transform.Find("MessageView").transform.Find("Sender").GetComponent<TMP_Text>().text = chatObject.transform.Find("Sender").GetComponent<TMP_Text>().text;
                    // make sure assess button is active
                    viewHolder.transform.Find("MessageView").transform.Find("AssessButton").gameObject.SetActive(true);
                }
            }
        }

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
            StartCoroutine(PageTransitionHandler(null));
            break;
            case "chatlist":
            StartCoroutine(gameManager.activeApp.GetComponent<AppManager>().ButtonInteractionHandler(true));
            break;
            // TODO: handle the rest of the in app pages
        }
    }

}
