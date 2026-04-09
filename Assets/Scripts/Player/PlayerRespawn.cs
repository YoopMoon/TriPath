using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{

    private float checkpointPositionX, checkpointPositionY;
    

    void Start()
    {
        if (PlayerPrefs.GetFloat("checkpointPositionX") != 0 && PlayerPrefs.GetFloat("checkpointPositionY") != 0)
        {
            transform.position = new Vector2(PlayerPrefs.GetFloat("checkpointPositionX"), PlayerPrefs.GetFloat("checkpointPositionY"));
        }
    }

    public void ReachedCheckpoint(float x, float y)
    {
        PlayerPrefs.SetFloat("checkpointPositionX", x);
        PlayerPrefs.SetFloat("checkpointPositionY", y);
    }
}
