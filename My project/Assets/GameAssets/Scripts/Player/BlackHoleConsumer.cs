using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BlackHoleConsumer : MonoBehaviour
{
    public struct ConsumedObjectData
    {
        public string name;
        public string tag;
        public int layer;
    }

    public static event Action<ConsumedObjectData> ObjectConsumed;

    [Header("References")]
    public Transform blackHoleScaleTarget;
    public SphereCollider consumptionCollider;

    [Header("Suction Targets")]
    public LayerMask consumableLayers;
    [Range(0.1f, 1f)] public float maxConsumableSizeRatio = 0.75f;

    [Header("Suction Range")]
    public float suctionRangeMultiplier = 4f;
    public float minSuctionRange = 4f;

    [Header("Suction Speed")]
    public float baseSuctionForce = 20f;
    public float smallObjectSpeedBonus = 2f;

    [Header("Growth")]
    public float blackHoleGrowthPerSize = 0.02f;
    public float colliderGrowthPerSize = 0.03f;

    //Maximum of 128 objects can be processed per FixedUpdate
    private readonly Collider[] overlapBuffer = new Collider[128];

    void Awake()
    {
        if (!blackHoleScaleTarget)
            blackHoleScaleTarget = transform;

        if (!consumptionCollider)
            consumptionCollider = GetComponent<SphereCollider>();

        consumptionCollider.isTrigger = true;
    }

    void FixedUpdate()
    {
        float blackHoleSize = GetSizeMagnitude(blackHoleScaleTarget, consumptionCollider);
        float maxConsumableSize = blackHoleSize * maxConsumableSizeRatio;

        float suctionRange = Mathf.Max(minSuctionRange, blackHoleSize * suctionRangeMultiplier);
        Vector3 center = consumptionCollider.bounds.center;

        //Does not allocate memory every frame, but can only process a limited number of objects (128)
        int found = Physics.OverlapSphereNonAlloc(center, suctionRange, overlapBuffer, consumableLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < found; i++)
        {
            Collider col = overlapBuffer[i];
            if (!col || col.transform == transform || col.transform.IsChildOf(transform))
                continue;

            float objectSize = GetSizeMagnitude(col.transform, col);
            if (objectSize > maxConsumableSize)
                continue;

            Vector3 toCenter = center - col.bounds.center;
            float distance = Mathf.Max(0.1f, toCenter.magnitude);
            Vector3 direction = toCenter / distance;

            float sizeRatio = objectSize / Mathf.Max(0.0001f, maxConsumableSize);
            float attractionMultiplier = 1f + (1f - Mathf.Clamp01(sizeRatio)) * smallObjectSpeedBonus;
            float pullStrength = baseSuctionForce * attractionMultiplier;

            Rigidbody rb = col.attachedRigidbody;
            if (rb && !rb.isKinematic)
            {
                rb.AddForce(direction * pullStrength, ForceMode.Acceleration);
            }
            else
            {
                col.transform.position = Vector3.MoveTowards(col.transform.position, center, pullStrength * Time.fixedDeltaTime);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer, consumableLayers))
            return;

        if (other.transform == transform || other.transform.IsChildOf(transform))
            return;

        float blackHoleSize = GetSizeMagnitude(blackHoleScaleTarget, consumptionCollider);
        float objectSize = GetSizeMagnitude(other.transform, other);

        if (objectSize > blackHoleSize * maxConsumableSizeRatio)
            return;

        Consume(other.gameObject, objectSize);
    }

    void Consume(GameObject targetObject, float objectSize)
    {
        ConsumedObjectData consumedData = new ConsumedObjectData
        {
            name = targetObject.name,
            tag = targetObject.tag,
            layer = targetObject.layer
        };

        if (targetObject.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = Vector3.zero;

        Destroy(targetObject);

        ObjectConsumed?.Invoke(consumedData);

        float growthAmount = objectSize * blackHoleGrowthPerSize;
        blackHoleScaleTarget.localScale += Vector3.one * growthAmount;

        consumptionCollider.radius += objectSize * colliderGrowthPerSize;
    }

    static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    static float GetSizeMagnitude(Transform targetTransform, Collider sourceCollider = null)
    {
        if (sourceCollider)
            return sourceCollider.bounds.size.magnitude;

        Renderer renderer = targetTransform.GetComponentInChildren<Renderer>();
        if (renderer)
            return renderer.bounds.size.magnitude;

        Collider collider = targetTransform.GetComponentInChildren<Collider>();
        if (collider)
            return collider.bounds.size.magnitude;

        return targetTransform.localScale.magnitude;
    }
}
