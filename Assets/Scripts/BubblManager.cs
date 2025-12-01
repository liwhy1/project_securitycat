using UnityEngine;

public class BubblManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameObject postInstance;
    [SerializeField] private GameObject postContent;

    private void Awake()
    {
        PostViewHandler();
    }

    private void PostViewHandler()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject newPost = Instantiate(postInstance, postInstance.transform.position, Quaternion.identity);
            newPost.name = "newPost";
            newPost.transform.SetParent(postContent.transform);
        }
    }
}