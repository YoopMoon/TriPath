using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) {
            Debug.Log("Player damaged");
            Destroy(collision.gameObject);
        }
    }
}
