using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AppManager appManager;
    [SerializeField] private MessagesManager messagesManager;
    [SerializeField] private GoodMailManager goodMailManager;
    [SerializeField] private BubblManager bubblManager;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private GameObject languageSwitcher;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject infoPage;

    [Header("Local Data")]
    [SerializeField] private GameObject currentState;
    [SerializeField] private GameObject lastState;
    public GameObject settingsView;
    private int lastPageHits;
    private bool isTransitioning;

    void Start()
    {
        // setup vars
        infoPage.SetActive(false);

        // setup buttons
        sfxToggle.onValueChanged.AddListener(delegate { AudioToggleHandler();} );
        languageSwitcher.transform.Find("Right").GetComponent<Button>().onClick.AddListener(delegate { LanguageSwitchHandler();} );
        languageSwitcher.transform.Find("Left").GetComponent<Button>().onClick.AddListener(delegate { LanguageSwitchHandler();} );
        restartButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); } );
        infoButton.onClick.AddListener(delegate { infoPage.SetActive(true); } );
        infoPage.transform.Find("ReturnButton").GetComponent<Button>().onClick.AddListener(delegate { infoPage.SetActive(false); });

        // Apply language preferences
        LanguageHandler();
    }

    private void LanguageHandler()
    {
        if (gameManager.selectedLocalization == "en")
        {
            sfxToggle.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Sound Effects";
            languageSwitcher.transform.Find("Title").GetComponent<TMP_Text>().text = "English";
            restartButton.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Restart Game";
            infoButton.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Information";
            infoPage.transform.Find("ReturnButton").transform.Find("Text").GetComponent<TMP_Text>().text = "Return";
            infoPage.transform.Find("Title").GetComponent<TMP_Text>().text = "Your score:\n" + gameManager.assessmentCorrect + " correct answers\n" + gameManager.assessmentIncorrect + " incorrect answers\n\nMade with <3\nby:\n Nedas\nDeividas\nMika\nLevente";
        }
        else
        {
            sfxToggle.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Geluidseffecten";
            languageSwitcher.transform.Find("Title").GetComponent<TMP_Text>().text = "Nederlands";
            restartButton.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Game opnieuw starten";
            infoButton.gameObject.transform.Find("Title").GetComponent<TMP_Text>().text = "Informatie";
            infoPage.transform.Find("ReturnButton").transform.Find("Text").GetComponent<TMP_Text>().text = "Terugkeer";
            infoPage.transform.Find("Title").GetComponent<TMP_Text>().text = "Je score:\n" + gameManager.assessmentCorrect + " juiste antwoorden\n" + gameManager.assessmentIncorrect + " foute antwoorden\n\nGemaakt met <3\nby:\n Nedas\nDeividas\nMika\nLevente";
        }
    }

    private void LanguageSwitchHandler()
    {
        gameManager.selectedLocalization = gameManager.selectedLocalization == "en" ? "du" : "en";
        LanguageHandler();
        messagesManager.LanguageHandler();
        goodMailManager.LanguageHandler();
        bubblManager.LanguageHandler();
    }

    private void AudioToggleHandler()
    {
        if (gameManager.gameObject.GetComponent<AudioSource>().volume == 1)
        {
            gameManager.gameObject.GetComponent<AudioSource>().volume = 0;
        }
        else
        {
            gameManager.gameObject.GetComponent<AudioSource>().volume = 1;
        }
    }

    public void AppRefreshHandler()
    {
        infoPage.SetActive(false);
        if (gameManager.selectedLocalization == "en")
        {
            infoPage.transform.Find("Title").GetComponent<TMP_Text>().text = "Your score:\n" + gameManager.assessmentCorrect + " correct answers\n" + gameManager.assessmentIncorrect + " incorrect answers\n\nMade with <3\nby:\n Nedas\nDeividas\nMika\nLevente";
        }
        else
        {
            infoPage.transform.Find("Title").GetComponent<TMP_Text>().text = "Je score:\n" + gameManager.assessmentCorrect + " juiste antwoorden\n" + gameManager.assessmentIncorrect + " foute antwoorden\n\nGemaakt met <3\nby:\n Nedas\nDeividas\nMika\nLevente";
        }
    }

    public void BackNavigationHandler()
    {
        if (isTransitioning) return;
        
        infoPage.SetActive(false);
        // bail if we are on chatlist or lastpagehit is reached
        if (lastPageHits > 2 || currentState == settingsView) 
        { 
            lastPageHits = 0; 
            StartCoroutine(appManager.AppInteractionHandler(true)); 
            return;
        }
        
        // transition to last page
        lastPageHits++;
        //StartCoroutine(PageTransitionHandler(lastState));
        StartCoroutine(appManager.AppInteractionHandler(true)); 
    }
}
