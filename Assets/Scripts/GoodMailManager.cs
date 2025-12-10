using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GoodMailManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AppManager appManager;
    [SerializeField] private GameObject mailInstance;
    [SerializeField] private GameObject mailContentInstance;

    [Header("Local Data")]
    [SerializeField] private GameObject currentState;
    [SerializeField] private GameObject lastState;
    private int lastPageHits;
    private bool isTransitioning;
    [SerializeField] private GameObject currentTargetMail;
    [SerializeField] private GameObject mailListContent;
    [SerializeField] private GameObject mailContent;
    [SerializeField] private GameObject viewHolder;
    public GameObject mailListView;
    [SerializeField] private GameObject mailContentView;
    [SerializeField] private GameObject profileView;
    [SerializeField] private Button mailListButton;
    [SerializeField] private Button profileButton;
    [SerializeField] private Button mailBackButton;
    [SerializeField] private GameObject messageAssessmentScreen;
    [SerializeField] private Button messageAssessButton;
    [SerializeField] private Button messageAssessConfirmButton;
    [SerializeField] private Button messageAssessReturnButton;
    [SerializeField] private List<MailContentTemplate> activeMails = new List<MailContentTemplate>(); // mail:content

    private void Start()
    {
        // setup vars
        currentState = mailListView;
        mailListView.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, 27f, 0f);
        profileView.GetComponent<RectTransform>().transform.localPosition = new Vector3(mailListView.GetComponent<RectTransform>().rect.width, 39.6f, 0f);
        mailContentView.GetComponent<RectTransform>().transform.localPosition = new Vector3(mailListView.GetComponent<RectTransform>().rect.width, 26.909f, 0f);
        messageAssessmentScreen.GetComponent<RectTransform>().transform.localPosition = new Vector3(0f, -96f, 0f);
        mailListView.SetActive(true);
        mailContentView.SetActive(false);
        profileView.SetActive(false);
        messageAssessmentScreen.SetActive(false);
        mailContent.SetActive(false);

        // setup buttons
        mailListButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(mailListView)); lastPageHits = 0; } );
        profileButton.onClick.AddListener(delegate {StartCoroutine(PageTransitionHandler(profileView)); lastPageHits = 0; } );
        mailBackButton.onClick.AddListener(delegate { StartCoroutine(PageTransitionHandler(mailListView)); lastPageHits = 0; } );
        messageAssessButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(true); });
        messageAssessConfirmButton.onClick.AddListener(delegate { AssessmentHandler(); });
        messageAssessReturnButton.onClick.AddListener(delegate { messageAssessmentScreen.SetActive(false); });

        // generate mailview
        PhotoViewHandler();
    }

    private void PhotoViewHandler()
    {
        int noticeCount = 0;
        for (int i = 0; i < 8; i++)
        {
            GameObject newMail = Instantiate(mailInstance, mailInstance.transform.position, Quaternion.identity);
            newMail.name = "newMail";
            newMail.transform.Find("Sender").GetComponent<TMP_Text>().text = "Sender #" + i;
            newMail.transform.Find("Message").GetComponent<TMP_Text>().text = "New mail";
            newMail.transform.SetParent(mailListContent.transform);
            newMail.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            // generate openable mails
            if ((UnityEngine.Random.Range(0,2) == 1 || i > 4) && noticeCount < 3) 
            {
                noticeCount++;
                newMail.transform.Find("Notice").gameObject.SetActive(true);
                newMail.GetComponent<Button>().onClick.AddListener(delegate { MailInteractionHandler(newMail); lastPageHits = 0; } );
                MessageViewHandler(newMail);
            }
            else 
            {
                newMail.GetComponent<Image>().color = new Color32(177,177,177,255); 
                newMail.GetComponent<Button>().interactable = false;
            }
        }
    }

    private void MessageViewHandler(GameObject mailObject)
    {
        GameObject newContent = Instantiate(mailContent, mailContent.transform.position, Quaternion.identity);
        newContent.name = "Content";
        newContent.SetActive(false);
        newContent.transform.SetParent(mailContent.transform.parent);
        newContent.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        newContent.GetComponent<RectTransform>().sizeDelta = new Vector3(0f, 0f, 0f);
        newContent.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        activeMails.Add(new MailContentTemplate {mailObject = mailObject, contentObject = newContent});

        GameObject newMail = Instantiate(mailContentInstance, mailContentInstance.transform.position, Quaternion.identity);
        newMail.name = "newMail";
        newMail.transform.Find("Sender").GetComponent<TMP_Text>().text = mailObject.transform.Find("Sender").GetComponent<TMP_Text>().text;
        newMail.transform.SetParent(newContent.transform);
        newMail.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
    }

    private void MailInteractionHandler(GameObject newMail)
    {
        currentTargetMail = newMail;
        StartCoroutine(PageTransitionHandler(mailContentView));
    }

    public void AssessmentHandler()
    {
        messageAssessButton.gameObject.SetActive(false);
        
        // disable current chat TODO: is this temp?
        var activeMail = activeMails.FirstOrDefault(x => x.contentObject == mailContentView.GetComponent<ScrollRect>().content.gameObject);
        activeMail.mailObject.transform.Find("Notice").gameObject.SetActive(false);
        activeMail.mailObject.GetComponent<Image>().color = new Color32(177,177,177,255);
        activeMail.mailObject.GetComponent<Button>().interactable = false;
        
        // return to chat page
        StartCoroutine(PageTransitionHandler(mailListView));
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
        Vector3 originalPosition = viewHolder.GetComponent<RectTransform>().transform.localPosition;
        float targetPositionX = targetPage == mailListView ? 0f : mailListView.GetComponent<RectTransform>().rect.width * -1f;
        
        if (targetPage == mailContentView)
        {
            // handle gameobjects before transition
            messageAssessmentScreen.SetActive(false);
            mailListButton.gameObject.SetActive(false);
            profileButton.gameObject.SetActive(false);
            mailContentView.SetActive(true);
            appManager.appTitle.SetActive(false);
            messageAssessButton.gameObject.SetActive(true);

            // handle target page before transition
            if (currentTargetMail == null) { Debug.Log("Well shit."); yield return null;}
            activeMails.ForEach(x => x.contentObject.SetActive(false));
            var targetMail = activeMails.FirstOrDefault(x => x.mailObject == currentTargetMail);
            mailContentView.GetComponent<ScrollRect>().content = targetMail.contentObject.GetComponent<RectTransform>();
            targetMail.contentObject.SetActive(true);
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
        if (targetPage == mailListView)
        {
            mailListButton.gameObject.SetActive(true);
            profileButton.gameObject.SetActive(true);
            appManager.appTitle.SetActive(true);
            currentTargetMail = null;
        }

        // reset mailcontentview position
        if (targetPage == mailContentView) mailContentView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
    
        // free up transition state
        isTransitioning = false;

        // hide last state after transition finished
        lastState.SetActive(false);
    }
    
    private void PageVisibilityHandler()
    {
        mailListView.SetActive(false);
        mailContentView.SetActive(false);
        profileView.SetActive(false);
        messageAssessmentScreen.SetActive(false);
        currentState.SetActive(true);
        lastState.SetActive(true);
    }
    public void BackNavigationHandler()
    {
        // bail if we are on chatlist or lastpagehit is reached
        if (lastPageHits > 2 || currentState == mailListView) 
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
public class MailContentTemplate
{
    public GameObject mailObject;
    public GameObject contentObject;
}