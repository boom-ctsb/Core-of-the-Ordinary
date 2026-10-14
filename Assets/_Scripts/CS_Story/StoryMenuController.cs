using UnityEngine;

public class StoryMenuController : MonoBehaviour
{
    public static StoryMenuController Instance { get; private set; }

    [Header("Pages")]
    [SerializeField] private StageListPageUI stageListPage;
    [SerializeField] private EpisodeListPageUI episodeListPage;
    [SerializeField] private EpisodeDetailPageUI episodeDetailPage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void OnEnable()
    {
        OpenStagePage();
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    public void OpenStagePage()
    {
        stageListPage.Open();
        episodeListPage.Close();
        episodeDetailPage.Close();
    }

    public void OpenEpisodeList(StageData stage)
    {
        stageListPage.Close();
        episodeListPage.Open(stage);
        episodeDetailPage.Close();
    }

    public void OpenEpisodeDetail(EpisodeData ep, StageData stage)
    {
        stageListPage.Close();
        episodeListPage.Close();
        episodeDetailPage.Open(ep, stage);
    }
}