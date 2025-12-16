using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AppManager appManager;

    [Header("Local Data")]
    [SerializeField] private GameObject currentState;
    [SerializeField] private GameObject lastState;
    public GameObject settingsView;
    private int lastPageHits;
    private bool isTransitioning;

    void Start()
    {
        
    }

    public void BackNavigationHandler()
    {
        if (isTransitioning) return;

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
