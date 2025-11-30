using UnityEngine;
using TMPro;

public class MessagesManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameObject chatInstance;
    [SerializeField] private GameObject chatContent;

    private void Awake()
    {
        ChatViewHandler();
    }

    private void ChatViewHandler()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject newChat = Instantiate(chatInstance, chatInstance.transform.position, Quaternion.identity);
            newChat.name = "newChat";
            newChat.transform.Find("Sender").GetComponent<TMP_Text>().text = "New sender";
            newChat.transform.Find("Message").GetComponent<TMP_Text>().text = "New message";
            newChat.transform.SetParent(chatContent.transform);            
        }
        Debug.Log("ChatView generated!");
    }
}
