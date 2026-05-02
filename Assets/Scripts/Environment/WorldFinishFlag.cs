using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldFinishFlag : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip finishClip;
    [SerializeField] private float finishVolume = 1f;

    [Header("Transition")]
    [SerializeField] private GameObject transitionObject;
    [SerializeField] private float delayBeforeTransition = 2f;
    [SerializeField] private float transitionDuration = 1.5f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    private bool isActivated;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (transitionObject != null)
            transitionObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated)
            return;

        if (!collision.CompareTag("Player"))
            return;

        isActivated = true;

        StartCoroutine(FinishWorldSequence());
    }

    private IEnumerator FinishWorldSequence()
    {
        if (animator != null)
            animator.enabled = true;

        PlayFinishSFX();

        yield return new WaitForSeconds(delayBeforeTransition);

        if (transitionObject != null)
            transitionObject.SetActive(true);

        yield return new WaitForSeconds(transitionDuration);

        if (LevelMetrics.Instance != null)
        {
            LevelMetrics.Instance.SaveCurrentLevelCoinSummary();
            LevelMetrics.Instance.LogLevelCoinSummaries();
            LevelMetrics.Instance.LogCurrentMapCoinSummary();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void PlayFinishSFX()
    {
        if (finishClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(finishClip, finishVolume);
    }
}