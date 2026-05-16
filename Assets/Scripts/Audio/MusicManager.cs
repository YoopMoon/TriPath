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

        // Volumen específico para esta escena.
        [Range(0f, 1f)]
        public float volume = 0.15f;
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

        // Volumen específico para este grupo de escenas.
        // Por ejemplo, puedes poner MenuFlow a 0.08.
        [Range(0f, 1f)]
        public float volume = 0.10f;
    }

    [System.Serializable]
    public class WorldMusicEntry
    {
        // Prefijo del mundo, por ejemplo Map1, Map2, Map3.
        public string worldPrefix;

        // Pista común para los niveles de ese mundo.
        public AudioClip musicClip;

        // Volumen específico para este mundo.
        // Por ejemplo, puedes poner los mapas a 0.05 si suenan más fuertes.
        [Range(0f, 1f)]
        public float volume = 0.08f;
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
    [Range(0f, 1f)]
    [SerializeField] private float masterMusicVolume = DefaultMasterMusicVolume;

    private const float DefaultMasterMusicVolume = 0.5f;
    private const string MusicVolumePrefsKey = "MusicVolume";

    // AudioSource que está sonando actualmente.
    private AudioSource activeMusicSource;

    // AudioSource preparado para entrar durante el crossfade.
    private AudioSource inactiveMusicSource;

    // Guardamos la última información reproducida para no reiniciar la pista innecesariamente.
    private string currentSceneName = string.Empty;
    private string currentWorldPrefix = string.Empty;
    private AudioClip currentClip;

    // Volumen objetivo de la pista que está sonando actualmente.
    private float currentTargetVolume;

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

        masterMusicVolume = PlayerPrefs.GetFloat(MusicVolumePrefsKey, DefaultMasterMusicVolume);

        ConfigureAudioSource(firstMusicSource);
        ConfigureAudioSource(secondMusicSource);

        activeMusicSource = firstMusicSource;
        inactiveMusicSource = secondMusicSource;

        currentTargetVolume = defaultVolume;

        if (activeMusicSource != null)
            activeMusicSource.volume = GetFinalMusicVolume(currentTargetVolume);

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

        MusicSelection selection = GetMusicSelectionForScene(sceneName);

        AudioClip newClip = selection.clip;
        float newVolume = selection.volume;

        string newWorldPrefix = GetWorldPrefix(sceneName);

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

            // Si el clip es el mismo pero el volumen configurado ha cambiado,
            // ajustamos suavemente el volumen al nuevo valor.
            if (!Mathf.Approximately(currentTargetVolume, newVolume))
                SetVolume(newVolume);

            return;
        }

        currentSceneName = sceneName;
        currentWorldPrefix = newWorldPrefix;
        currentClip = newClip;
        currentTargetVolume = newVolume;

        CrossfadeToClip(newClip, newVolume);
    }

    private MusicSelection GetMusicSelectionForScene(string sceneName)
    {
        // Orden de prioridad:
        // 1. Música específica de escena
        // 2. Música de grupo de escenas
        // 3. Música por mundo

        MusicSelection sceneSelection = GetSceneMusicSelection(sceneName);

        if (sceneSelection.clip != null)
            return sceneSelection;

        MusicSelection groupSelection = GetSceneGroupMusicSelection(sceneName);

        if (groupSelection.clip != null)
            return groupSelection;

        string worldPrefix = GetWorldPrefix(sceneName);

        MusicSelection worldSelection = GetWorldMusicSelection(worldPrefix);

        if (worldSelection.clip != null)
            return worldSelection;

        return new MusicSelection(null, defaultVolume);
    }

    private MusicSelection GetSceneMusicSelection(string sceneName)
    {
        if (sceneMusicEntries == null)
            return new MusicSelection(null, defaultVolume);

        foreach (SceneMusicEntry entry in sceneMusicEntries)
        {
            if (entry == null)
                continue;

            if (entry.sceneName == sceneName)
                return new MusicSelection(entry.musicClip, entry.volume);
        }

        return new MusicSelection(null, defaultVolume);
    }

    private MusicSelection GetSceneGroupMusicSelection(string sceneName)
    {
        if (sceneGroupMusicEntries == null)
            return new MusicSelection(null, defaultVolume);

        foreach (SceneGroupMusicEntry entry in sceneGroupMusicEntries)
        {
            if (entry == null || entry.sceneNames == null)
                continue;

            foreach (string groupedSceneName in entry.sceneNames)
            {
                if (groupedSceneName == sceneName)
                    return new MusicSelection(entry.musicClip, entry.volume);
            }
        }

        return new MusicSelection(null, defaultVolume);
    }

    private MusicSelection GetWorldMusicSelection(string worldPrefix)
    {
        if (string.IsNullOrEmpty(worldPrefix) || worldMusicEntries == null)
            return new MusicSelection(null, defaultVolume);

        foreach (WorldMusicEntry entry in worldMusicEntries)
        {
            if (entry == null)
                continue;

            if (entry.worldPrefix == worldPrefix)
                return new MusicSelection(entry.musicClip, entry.volume);
        }

        return new MusicSelection(null, defaultVolume);
    }

    private void CrossfadeToClip(AudioClip newClip, float targetVolume)
    {
        if (newClip == null)
            return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, targetVolume));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float targetVolume)
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
            inactiveMusicSource.volume = Mathf.Lerp(0f, GetFinalMusicVolume(targetVolume), t);

            yield return null;
        }

        activeMusicSource.Stop();
        activeMusicSource.clip = null;
        activeMusicSource.volume = 0f;

        inactiveMusicSource.volume = GetFinalMusicVolume(targetVolume);

        SwapAudioSources();

        crossfadeCoroutine = null;
    }

    private void FadeOutMusic()
    {
        currentClip = null;
        currentSceneName = string.Empty;
        currentWorldPrefix = string.Empty;
        currentTargetVolume = defaultVolume;

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
        currentTargetVolume = Mathf.Clamp01(volume);

        if (activeMusicSource != null)
            activeMusicSource.volume = GetFinalMusicVolume(currentTargetVolume);
    }

    public void SetMasterMusicVolume(float volume)
    {
        masterMusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MusicVolumePrefsKey, masterMusicVolume);
        PlayerPrefs.Save();

        if (activeMusicSource != null)
            activeMusicSource.volume = GetFinalMusicVolume(currentTargetVolume);
    }

    public float GetMasterMusicVolume()
    {
        return masterMusicVolume;
    }

    private float GetFinalMusicVolume(float baseVolume)
    {
        return Mathf.Clamp01(baseVolume) * masterMusicVolume;
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
        currentTargetVolume = defaultVolume;
    }

    public void PauseMusic()
    {
        if (activeMusicSource != null)
            activeMusicSource.Pause();

        if (inactiveMusicSource != null)
            inactiveMusicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (activeMusicSource != null && activeMusicSource.clip != null)
            activeMusicSource.UnPause();

        if (inactiveMusicSource != null && inactiveMusicSource.clip != null)
            inactiveMusicSource.UnPause();
    }

    private readonly struct MusicSelection
    {
        public readonly AudioClip clip;
        public readonly float volume;

        public MusicSelection(AudioClip clip, float volume)
        {
            this.clip = clip;
            this.volume = Mathf.Clamp01(volume);
        }
    }
}
