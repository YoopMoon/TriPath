using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;

    public string previousScene { get; private set; }
    public string targetSpawnId { get; private set; }

    // Guardamos aquí la vida del jugador entre escenas.
    // Usamos -1 como valor "no inicializado" para saber si ya existe una vida previa guardada.
    private int savedPlayerHealth = -1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetTransitionData(string fromScene, string spawnId)
    {
        previousScene = fromScene;
        targetSpawnId = spawnId;
    }

    public void ClearTransitionData()
    {
        previousScene = "";
        targetSpawnId = "";
    }

    // Guarda la vida actual del jugador para recuperarla en la siguiente escena.
    public void SetPlayerHealth(int health)
    {
        savedPlayerHealth = health;
    }

    // Devuelve la vida guardada.
    public int GetSavedHealth()
    {
        return savedPlayerHealth;
    }

    // Indica si ya hay una vida guardada válida.
    public bool HasSavedHealth()
    {
        return savedPlayerHealth >= 0;
    }

    // Limpia la vida guardada, útil por ejemplo al morir o al reiniciar partida.
    public void ClearPlayerHealth()
    {
        savedPlayerHealth = -1;
    }
}