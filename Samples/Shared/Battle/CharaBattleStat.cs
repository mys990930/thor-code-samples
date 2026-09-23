/// <summary>
/// CharaBattleStat의 모든 Factor들은 미리 1을 더한 값으로 초기화한다.
/// 추가적인 계산 없이 필요처에서 바로 Factor만 곱해서 사용할 수 있게 한다.
/// </summary>
public class CharaBattleStat
{
    public float DMGFactor = 1;
    public float GoldEarnFactor = 1;
    public float EXPEarnFactor = 1;
    public float RecoveryFactor = 0;
    public float SpeedFactor = 1;
    public float MAXHPFactor = 1;
    public float BossDMGFactor = 1;
}