using TMPro;
using UnityEngine;

public class QuestObjectiveItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Color inProgressColor = Color.white;
    [SerializeField] private Color completedColor = new Color(0.62f, 1f, 0.62f);

    void Awake()
    {
        if (!objectiveText)
            objectiveText = GetComponentInChildren<TMP_Text>();
    }

    public void Refresh(string label, int currentCount, int requiredCount, bool completed)
    {
        if (!objectiveText)
            return;

        string baseText = $"{label} {currentCount}/{requiredCount}";
        objectiveText.text = completed ? $"<s>{baseText}</s>" : baseText;
        objectiveText.color = completed ? completedColor : inProgressColor;
    }
}
