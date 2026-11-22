using UnityEngine;
using System.Collections;

public class TNTExplosion : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.6f;

    private void Start()
    {
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(animationDuration);

        Destroy(gameObject);
    }
}