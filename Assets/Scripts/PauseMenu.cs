using UnityEngine;

public class CollectibleOrb : MonoBehaviour
{
    public int points = 10;
    public AudioClip collectSFX;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.Instance.AddScore(points);

            AudioSource.PlayClipAtPoint(
                collectSFX,
                transform.position
            );

            Destroy(gameObject);
        }
    }
}