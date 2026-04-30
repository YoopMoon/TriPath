using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    // Singleton para acceder fácilmente al manager desde cualquier parte.
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    public class SceneMusicEntry
    {
        // Música específica para una escena concreta.
        // Útil si una escena debe tener una pista única.
        public string sceneName;
        public AudioClip musicClip;
    }

    [System.Serializable]
    public class SceneGroupMusicEntry
    {
        // Nombre interno del grupo. Solo sirve para organizarlo mejor en el inspector.
        public string groupName;

        // Escenas que compartirán esta misma música.
        public string[] sceneNames;

        // Pista común para todas esas escenas.
        public AudioClip musicClip;
    }

    [System.Serializable]
    public class WorldMusicEntry
    {
        // Prefijo del mundo, por ejemplo Map1, Map2, Map3.
        public string worldPrefix;
        public AudioClip musicClip;
    }

    [Header("References")]
    [SerializeField] private AudioSource musicSource;

    [Header("Scene Music")]
    // Música específica de escena.
    // Tiene prioridad sobre grupos y mundos.
    [SerializeField] private SceneMusicEntry[] sceneMusicEntries;

    [Header("Scene Group Music")]
    // Música compartida entre varias escenas.
    // Por ejemplo MainMenu, CharacterSelect y HowToPlay.
    [SerializeField] private SceneGroupMusicEntry[] sceneGroupMusicEntries;

    [Header("World Music")]
    // Música por mundo.
    // Por ejemplo Map1_Level1, Map1_Level2... comparten la misma.
    [SerializeField] private WorldMusicEntry[] worldMusicEntries;

    [Header("Settings")]
    [SerializeField] private float defaultVolume = 0.15f;

    // Guardamos la última información reproducida para no reiniciar la pista innecesariamente.
    private string currentSceneName = string.Empty;
    private string currentWorldPrefix = string.Empty;
    private AudioClip currentClip;

    private void Awake()
    {
        // Evita duplicados del manager al cambiar de escena.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configuración base del AudioSource de música.
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f; // Música en 2D
            musicSource.volume = defaultVolume;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Ajusta música al iniciar el juego.
        UpdateMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que cambia la escena, revisamos qué música debe sonar.
        UpdateMusicForScene(scene.name);
    }

    private void UpdateMusicForScene(string sceneName)
    {
        if (musicSource == null)
            return;

        // Orden de prioridad:
        // 1. Música específica de escena
        // 2. Música de grupo de escenas
        // 3. Música por mundo
        AudioClip newClip = GetClipForScene(sceneName);

        if (newClip == null)
            newClip = GetClipForSceneGroup(sceneName);

        string newWorldPrefix = GetWorldPrefix(sceneName);

        if (newClip == null)
            newClip = GetClipForWorld(newWorldPrefix);

        // Si no hay música definida para esta escena, paramos.
        if (newClip == null)
        {
            StopMusic();
            return;
        }

        // Si ya está sonando este mismo clip, no lo reiniciamos.
        // Esto es justo lo que permite continuidad entre MainMenu,
        // CharacterSelectScene y HowToPlayScene si comparten pista.
        if (currentClip == newClip && musicSource.isPlaying)
        {
            currentSceneName = sceneName;
            currentWorldPrefix = newWorldPrefix;
            return;
        }

        // Si el clip cambia, actualizamos y reproducimos el nuevo.
        currentSceneName = sceneName;
        currentWorldPrefix = newWorldPrefix;
        currentClip = newClip;

        musicSource.clip = newClip;
        musicSource.Play();
    }

    private AudioClip GetClipForScene(string sceneName)
    {
        if (sceneMusicEntries == null)
            return null;

        foreach (SceneMusicEntry entry in sceneMusicEntries)
        {
            if (entry == null)
                continue;

            if (entry.sceneName == sceneName)
                return entry.musicClip;
        }

        return null;
    }

    private AudioClip GetClipForSceneGroup(string sceneName)
    {
        if (sceneGroupMusicEntries == null)
            return null;

        foreach (SceneGroupMusicEntry entry in sceneGroupMusicEntries)
        {
            if (entry == null || entry.sceneNames == null)
                continue;

            foreach (string groupedSceneName in entry.sceneNames)
            {
                if (groupedSceneName == sceneName)
                    return entry.musicClip;
            }
        }

        return null;
    }

    private AudioClip GetClipForWorld(string worldPrefix)
    {
        if (string.IsNullOrEmpty(worldPrefix) || worldMusicEntries == null)
            return null;

        foreach (WorldMusicEntry entry in worldMusicEntries)
        {
            if (entry == null)
                continue;

            if (entry.worldPrefix == worldPrefix)
                return entry.musicClip;
        }

        return null;
    }

    private string GetWorldPrefix(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return string.Empty;

        // Map1_Level2 -> Map1
        int separatorIndex = sceneName.IndexOf('_');

        // Si no hay "_", no tratamos la escena como nivel de mundo.
        if (separatorIndex < 0)
            return string.Empty;

        return sceneName.Substring(0, separatorIndex);
    }

    public void SetVolume(float volume)
    {
        if (musicSource == null)
            return;

        musicSource.volume = Mathf.Clamp01(volume);
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;

        currentClip = null;
        currentSceneName = string.Empty;
        currentWorldPrefix = string.Empty;
    }

    public void PauseMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Play();
    }

}