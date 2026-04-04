using UnityEngine;

public class ToggleQuestPanel : MonoBehaviour
{
    public GameObject panel;
    public PlayerStateManager stateManager;

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            panel.SetActive(!panel.activeSelf);

            if (panel.activeSelf)
            {
                stateManager.SetState(PlayerState.Menu);
            }   
            else
            {
                stateManager.SetState(PlayerState.Default);
            }
        }
    }
}