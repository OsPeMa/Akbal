public interface IRhythmAction
{
    bool IsValid();
    void OnHit(HitResult result);
    void Apply(RhythmSession session);
}

public class PurifyAction : IRhythmAction
{
    public Enemy Target { get; }
    public Health PlayerHealth { get; }
    public float PerfectPurify { get; set; } = 2.0f;
    public float GoodPurify { get; set; } = 1.0f;
    public float PerfectHeal { get; set; } = 1.0f;
    public float GoodHeal { get; set; } = 0.5f;

    public PurifyAction(Enemy target, Health playerHealth)
    {
        Target = target;
        PlayerHealth = playerHealth;
    }

    public bool IsValid() => Target != null && Target.IsVulnerable;

    public void OnHit(HitResult result)
    {
        float purifyAmt = result == HitResult.Perfect ? PerfectPurify
                        : result == HitResult.Good    ? GoodPurify : 0f;
        float healAmt   = result == HitResult.Perfect ? PerfectHeal
                        : result == HitResult.Good    ? GoodHeal   : 0f;
        if (purifyAmt > 0f && Target != null) Target.Purify(purifyAmt);
        if (healAmt > 0f && PlayerHealth != null) PlayerHealth.Heal(healAmt);
    }

    public void Apply(RhythmSession session) { }
}
