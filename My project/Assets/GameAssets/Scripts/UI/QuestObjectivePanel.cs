using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestObjectivePanel : MonoBehaviour
{
    public enum ObjectiveMatchType
    {
        Tag,
        NameEquals,
        NameContains,
        Layer
    }

    [Serializable]
    public class ObjectiveDefinition
    {
        public string label = "Consume objects";
        public ObjectiveMatchType matchType = ObjectiveMatchType.Tag;
        public string matchValue = "Untagged";
        [Min(1)] public int requiredCount = 1;
        public bool caseSensitive;

        [NonSerialized] public int currentCount;

        public bool IsComplete => currentCount >= requiredCount;

        public bool Matches(BlackHoleConsumer.ConsumedObjectData data)
        {
            StringComparison comparison = caseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            switch (matchType)
            {
                case ObjectiveMatchType.Tag:
                    return string.Equals(data.tag, matchValue, comparison);
                case ObjectiveMatchType.NameEquals:
                    return string.Equals(NormalizeName(data.name), matchValue, comparison);
                case ObjectiveMatchType.NameContains:
                    return NormalizeName(data.name).IndexOf(matchValue, comparison) >= 0;
                case ObjectiveMatchType.Layer:
                    if (int.TryParse(matchValue, out int targetLayer))
                        return data.layer == targetLayer;
                    return string.Equals(LayerMask.LayerToName(data.layer), matchValue, comparison);
                default:
                    return false;
            }
        }

        static string NormalizeName(string source)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;

            return source.Replace("(Clone)", string.Empty).Trim();
        }
    }

    [Header("UI")]
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private QuestObjectiveItemUI objectiveItemPrefab;
    [SerializeField] private bool clearContainerChildrenOnStart = true;

    [Header("Objectives")]
    [SerializeField] private List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();

    private readonly List<QuestObjectiveItemUI> objectiveViews = new List<QuestObjectiveItemUI>();

    void Start()
    {
        BuildUI();
    }

    void OnEnable()
    {
        BlackHoleConsumer.ObjectConsumed += HandleObjectConsumed;
    }

    void OnDisable()
    {
        BlackHoleConsumer.ObjectConsumed -= HandleObjectConsumed;
    }

    void BuildUI()
    {
        if (!objectiveContainer || !objectiveItemPrefab)
            return;

        if (clearContainerChildrenOnStart)
        {
            for (int i = objectiveContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(objectiveContainer.GetChild(i).gameObject);
            }
        }

        objectiveViews.Clear();

        for (int i = 0; i < objectives.Count; i++)
        {
            ObjectiveDefinition objective = objectives[i];
            objective.currentCount = 0;

            QuestObjectiveItemUI itemView = Instantiate(objectiveItemPrefab, objectiveContainer);
            itemView.Refresh(objective.label, objective.currentCount, objective.requiredCount, objective.IsComplete);
            objectiveViews.Add(itemView);
        }
    }

    void HandleObjectConsumed(BlackHoleConsumer.ConsumedObjectData consumedData)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            ObjectiveDefinition objective = objectives[i];
            if (objective.IsComplete)
                continue;

            if (!objective.Matches(consumedData))
                continue;

            objective.currentCount = Mathf.Min(objective.currentCount + 1, objective.requiredCount);

            if (i < objectiveViews.Count && objectiveViews[i])
            {
                objectiveViews[i].Refresh(objective.label, objective.currentCount, objective.requiredCount, objective.IsComplete);
            }
        }
    }
}
