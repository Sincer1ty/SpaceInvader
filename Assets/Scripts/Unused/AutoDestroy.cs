using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float duration = 2f;
    
    private void Start()
    {
        Destroy(gameObject, duration);
    }
}
