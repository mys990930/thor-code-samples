public interface IListener
{
    /// <summary>
    /// RegisterListener를 구현한 후에 반드시 Start()에서 RegisterListener()를 호출할 것
    /// </summary>
    public void RegisterListener();

    /// <summary>
    /// DeregisterListener를 구현한 후에 반드시 OnDestroy()에서 RegisterListener()를 호출할 것
    /// </summary>
    public void DeregisterListener();
}