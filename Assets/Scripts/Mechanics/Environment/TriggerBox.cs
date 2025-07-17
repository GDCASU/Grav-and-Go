using UnityEngine;
using UnityEngine.Events;

public class TriggerBox : MonoBehaviour
{
    [Header("Target Identifiers")]
    public LayerMask targetLayers;
    public string[] targetTags;

    [Header("Callbacks")]
    public UnityEvent<Collider2D> OnEnter;
    public UnityEvent<Collider2D> OnStay;
    public UnityEvent<Collider2D> OnExit;

    [Header("Debug")]
    public bool DoDebug = false;

    private void Awake()
    {
        if (!TryGetComponent(out Collider2D _) && DoDebug) Debug.LogWarning("TriggerBox Missing Collider");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(IsTarget(collision))
            OnEnter?.Invoke(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsTarget(collision))
            OnStay?.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsTarget(collision))
            OnExit?.Invoke(collision);
    }

    private bool IsTarget(Collider2D collision)
    {
        bool foundTag = targetTags.Length == 0;

        if (!foundTag)
            foreach (string tag in targetTags)
                if (collision.CompareTag(tag))
                {
                    foundTag = true;
                    break;
                }

        if (!foundTag)
        {
            if (DoDebug) Debug.Log($"{collision.name}: was not on target tags");
            return false;
        }

        if (targetLayers != 0)
            if ((targetLayers & (1 << collision.gameObject.layer)) == 0)
            {
                if (DoDebug) Debug.Log($"{collision.name}: was not on target layers");
                return false;
            }

        return true;
    }
}
