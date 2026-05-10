using UnityEngine;

public class PlayerRunData : MonoBehaviour
{
    public static PlayerRunData Instance { get; private set; }

    public string PlayerName { get; private set; } = "Player";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPlayerName(string playerName)
    {
        playerName = playerName.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";

        PlayerName = playerName;
    }

    public void ResetRunData()
    {
        PlayerName = "Player";
    }
}