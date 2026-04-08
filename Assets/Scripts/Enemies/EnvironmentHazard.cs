using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        PlayerController playerController = collision.transform.GetComponent<PlayerController>();

        if (playerController != null && playerController.isInmortal)
            return;

        Debug.Log("Player damaged");
        Destroy(collision.gameObject);
    }
}
