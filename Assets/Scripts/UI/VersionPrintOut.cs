using TMPro;
using UnityEngine;

public class VersionPrintOut : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI displayText =  GetComponent<TextMeshProUGUI>();
        displayText.text = "Application Version : " + Application.version;
    }
}
