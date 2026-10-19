using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Coto/Story/StageData")]
public class StageData : ScriptableObject
{
    public Sprite stageThumbnail;
    public Sprite stageNameImage;
    [TextArea] public string storyDescription;
    public EpisodeData[] episodes;
}