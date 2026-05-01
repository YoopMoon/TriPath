using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 1f;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        float finalVolume = Mathf.Clamp01(volume) * masterSfxVolume;

        sfxSource.PlayOneShot(clip, finalVolume);
    }

    public void SetMasterSfxVolume(float volume)
    {
        masterSfxVolume = Mathf.Clamp01(volume);
    }
}