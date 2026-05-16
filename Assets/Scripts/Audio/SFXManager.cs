using System;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    public static float MasterSfxVolume { get; private set; } = DefaultMasterSfxVolume;
    public static event Action<float> OnMasterSfxVolumeChanged;

    [Header("References")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = DefaultMasterSfxVolume;

    private const float DefaultMasterSfxVolume = 0.5f;
    private const string SfxVolumePrefsKey = "SfxVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        masterSfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefsKey, DefaultMasterSfxVolume);
        MasterSfxVolume = masterSfxVolume;

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
        MasterSfxVolume = masterSfxVolume;
        PlayerPrefs.SetFloat(SfxVolumePrefsKey, masterSfxVolume);
        PlayerPrefs.Save();
        OnMasterSfxVolumeChanged?.Invoke(masterSfxVolume);
    }

    public float GetMasterSfxVolume()
    {
        return masterSfxVolume;
    }
}
