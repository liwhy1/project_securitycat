using UnityEngine;
using UnityEngine.UI;

public class PhotosManager : MonoBehaviour
{
    [Header("Reference Data")]
    [SerializeField] private GameObject photoInstance;
    [SerializeField] private GameObject photoContent;

    private void Awake()
    {
        PhotoViewHandler();
    }

    private void PhotoViewHandler()
    {
        for (int i = 0; i < 30; i++)
        {
            GameObject newPhoto = Instantiate(photoInstance, photoInstance.transform.position, Quaternion.identity);
            int r = Random.Range(0,3);
            newPhoto.GetComponent<Image>().color = r == 0 ? Color.lightBlue : r == 1 ? Color.lightGreen : Color.lightCoral;
            newPhoto.name = "newPhoto";
            newPhoto.transform.SetParent(photoContent.transform);
            newPhoto.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
