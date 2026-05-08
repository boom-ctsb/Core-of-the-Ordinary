using System.Collections;
/// <summary>
/// Base class สำหรับทุก Action
/// </summary>
public abstract class Action
{
    public string ActionName { get; protected set; }
    public string Description { get; protected set; }
    public abstract IEnumerator Execute(BattleManager battleManager, BattleCharacter source, BattleCharacter target);
}