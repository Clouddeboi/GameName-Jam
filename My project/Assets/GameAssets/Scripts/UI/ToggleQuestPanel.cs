using UnityEngine;

public class ToggleQuestPanel : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            panel.SetActive(!panel.activeSelf);
        }
    }
}