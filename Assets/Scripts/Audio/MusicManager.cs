using System.Collections;
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
    [SerializeField] public AudioSource firstMusicSource;
    [SerializeField] public AudioSource secondMusicSource;

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
    [SerializeField] private float crossfadeDuration = 1.5f;

    // AudioSource que está sonando actualmente.
    private AudioSource activeMusicSource;

    // AudioSource preparado para entrar durante el crossfade.
    private AudioSource inactiveMusicSource;

    // Guardamos la última información reproducida para no reiniciar la pista innecesariamente.
    private string currentSceneName = string.Empty;
    private string currentWorldPrefix = string.Empty;
    private AudioClip currentClip;

    private Coroutine crossfadeCoroutine;

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

        ConfigureAudioSource(firstMusicSource);
        ConfigureAudioSource(secondMusicSource);

        activeMusicSource = firstMusicSource;
        inactiveMusicSource = secondMusicSource;

        if (activeMusicSource != null)
            activeMusicSource.volume = defaultVolume;

        if (inactiveMusicSource != null)
            inactiveMusicSource.volume = 0f;
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

    private void ConfigureAudioSource(AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f; // Música en 2D
        source.volume = 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que cambia la escena, revisamos qué música debe sonar.
        UpdateMusicForScene(scene.name);
    }

    private void UpdateMusicForScene(string sceneName)
    {
        if (activeMusicSource == null || inactiveMusicSource == null)
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

        // Si no hay música definida para esta escena, paramos con fade out.
        if (newClip == null)
        {
            FadeOutMusic();
            return;
        }

        // Si ya está sonando este mismo clip, no lo reiniciamos.
        // Esto permite continuidad entre MainMenu,
        // CharacterSelectScene y HowToPlayScene si comparten pista.
        if (currentClip == newClip && activeMusicSource.isPlaying)
        {
            currentSceneName = sceneName;
            currentWorldPrefix = newWorldPrefix;
            return;
        }

        currentSceneName = sceneName;
        currentWorldPrefix = newWorldPrefix;
        currentClip = newClip;

        CrossfadeToClip(newClip);
    }

    private void CrossfadeToClip(AudioClip newClip)
    {
        if (newClip == null)
            return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.loop = true;
        inactiveMusicSource.Play();

        float timer = 0f;
        float startActiveVolume = activeMusicSource.isPlaying ? activeMusicSource.volume : 0f;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / crossfadeDuration;

            activeMusicSource.volume = Mathf.Lerp(startActiveVolume, 0f, t);
            inactiveMusicSource.volume = Mathf.Lerp(0f, defaultVolume, t);

            yield return null;
        }

        activeMusicSource.Stop();
        activeMusicSource.clip = null;
        activeMusicSource.volume = 0f;

        inactiveMusicSource.volume = defaultVolume;

        SwapAudioSources();

        crossfadeCoroutine = null;
    }

    private void FadeOutMusic()
    {
        currentClip = null;
        currentSceneName = string.Empty;
        currentWorldPrefix = string.Empty;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        float timer = 0f;
        float startVolume = activeMusicSource.volume;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / crossfadeDuration;

            activeMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        activeMusicSource.Stop();
        activeMusicSource.clip = null;
        activeMusicSource.volume = 0f;

        crossfadeCoroutine = null;
    }

    private void SwapAudioSources()
    {
        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
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
        defaultVolume = Mathf.Clamp01(volume);

        if (activeMusicSource != null)
            activeMusicSource.volume = defaultVolume;
    }

    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        if (activeMusicSource != null)
        {
            activeMusicSource.Stop();
            activeMusicSource.clip = null;
            activeMusicSource.volume = 0f;
        }

        if (inactiveMusicSource != null)
        {
            inactiveMusicSource.Stop();
            inactiveMusicSource.clip = null;
            inactiveMusicSource.volume = 0f;
        }

        currentClip = null;
        currentSceneName = string.Empty;
        currentWorldPrefix = string.Empty;
    }

    public void PauseMusic()
    {
        if (activeMusicSource == null)
            return;

        activeMusicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (activeMusicSource == null)
            return;

        activeMusicSource.Play();
    }


}