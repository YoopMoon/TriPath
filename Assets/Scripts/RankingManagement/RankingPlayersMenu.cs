using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingPlayersMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private RankingRowUI rankingRowPrefab;
    [SerializeField] private Text emptyRankingText;
    [SerializeField] private int maxRankingRows = 5;

    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button clearRankingButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private float clickVolume = 1f;

    private bool isLoadingScene;

    private void Start()
    {
        isLoadingScene = false;

        ShowRanking();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (clearRankingButton != null)
            clearRankingButton.onClick.AddListener(ClearRanking);
    }

    private void ShowRanking()
    {
        if (rankingContainer == null)
        {
            Debug.LogWarning("[RankingPlayersMenu] Ranking Container is not assigned.");
            return;
        }

        if (rankingRowPrefab == null)
        {
            Debug.LogWarning("[RankingPlayersMenu] Ranking Row Prefab is not assigned.");
            return;
        }

        foreach (Transform child in rankingContainer)
            Destroy(child.gameObject);

        List<RankingEntry> entries = RankingStorage.GetSortedEntries();

        Debug.Log($"[RankingPlayersMenu] Entries loaded: {entries.Count}");

        bool hasEntries = entries.Count > 0;

        if (emptyRankingText != null)
            emptyRankingText.gameObject.SetActive(!hasEntries);

        int rowsToShow = Mathf.Min(entries.Count, maxRankingRows);

        for (int i = 0; i < rowsToShow; i++)
        {
            Debug.Log("Entrada");
            RankingRowUI row = Instantiate(rankingRowPrefab, rankingContainer);

            row.gameObject.SetActive(true);
            row.transform.localScale = Vector3.one;
            row.transform.localPosition = Vector3.zero;

            row.Setup(i + 1, entries[i]);
        }
    }

    public void GoToMainMenu()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();
        ResetCurrentRun();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        if (isLoadingScene)
            return;

        isLoadingScene = true;

        PlayClickSFX();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ClearRanking()
    {
        PlayClickSFX();

        RankingStorage.ClearRanking();
        ShowRanking();
    }

    private void ResetCurrentRun()
    {
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.ResetProgress();

        if (LevelMetrics.Instance != null)
            LevelMetrics.Instance.ResetProgress();

        if (LevelProgressManager.Instance != null)
            LevelProgressManager.Instance.ResetProgress();

        if (AdaptiveDifficultyManager.Instance != null)
            AdaptiveDifficultyManager.Instance.ResetProgress();

        if (PlayerRunData.Instance != null)
            PlayerRunData.Instance.ResetRunData();
    }

    private void PlayClickSFX()
    {
        if (clickClip == null)
            return;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFX(clickClip, clickVolume);
    }
}