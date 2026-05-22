using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public enum Phase { Attacking, Stunned, Ritualling, Resolving }
    enum AttackSubState { Chasing, Telegraphing, Striking }

    [Header("Identity")]
    public EnemyArchetype archetype;
    public float beatOffset = 0f;

    [Header("Target")]
    public Transform target;
    public string targetTag = "Player";

    public Phase CurrentPhase { get; private set; } = Phase.Attacking;
    public bool IsVulnerable => CurrentPhase == Phase.Stunned || CurrentPhase == Phase.Ritualling;
    public bool CanReceiveRitual => CurrentPhase == Phase.Stunned;
    public float Pressure { get; private set; }

    Health health;
    Rigidbody rb;
    MeshRenderer mr;

    int lastTriggerCycle = -1;

    int activeAttackIndex = -1;
    Vector3 strikeStartPos;
    Vector3 strikeEndPos;
    double strikeStartBeat;
    double strikeEndBeat;
    bool playerHitThisStrike;
    AttackSubState prevSubState = AttackSubState.Chasing;

    double phaseEnteredBeat;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        mr = GetComponentInChildren<MeshRenderer>();
        EnemyRegistry.Register(this);
        health.Died += OnDeath;
    }

    void OnDestroy()
    {
        EnemyRegistry.Unregister(this);
        if (health != null) health.Died -= OnDeath;
    }

    void Start()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null) target = go.transform;
        }
        if (archetype != null) ApplyColor(archetype.baseColor);
    }

    void Update()
    {
        if (!health.IsAlive) return;
        var c = Conductor.I;
        if (c == null || !c.IsPlaying || archetype == null)
        {
            if (CurrentPhase == Phase.Attacking) DoChase();
            return;
        }

        double songBeats = c.SongBeats;
        UpdatePhase(songBeats);

        switch (CurrentPhase)
        {
            case Phase.Attacking: RunAttacking(songBeats); break;
            case Phase.Stunned:
            case Phase.Ritualling:
            case Phase.Resolving:
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }

    void UpdatePhase(double songBeats)
    {
        switch (CurrentPhase)
        {
            case Phase.Attacking:
                CheckAttackingTransitions(songBeats);
                break;
            case Phase.Stunned:
                if (songBeats - phaseEnteredBeat > archetype.stunDurationBeats)
                    EnterPhase(Phase.Attacking, songBeats);
                break;
            case Phase.Ritualling:
                break;
            case Phase.Resolving:
                if (songBeats - phaseEnteredBeat > archetype.resolveDurationBeats)
                    EnterPhase(Phase.Attacking, songBeats);
                break;
        }
    }

    void CheckAttackingTransitions(double songBeats)
    {
        var pattern = archetype.attackPattern;
        if (pattern == null || pattern.totalBeats <= 0f) return;

        double localBeats = songBeats - beatOffset;
        int cycle = (int)System.Math.Floor(localBeats / pattern.totalBeats);
        if (cycle <= lastTriggerCycle) return;

        if (Pressure >= archetype.pressureThreshold)
        {
            lastTriggerCycle = cycle;
            EnterPhase(Phase.Stunned, songBeats);
            return;
        }

        float patternBeat = (float)(localBeats - cycle * pattern.totalBeats);
        if (patternBeat >= pattern.vulnerableStartBeat && patternBeat < pattern.vulnerableEndBeat)
        {
            lastTriggerCycle = cycle;
            EnterPhase(Phase.Stunned, songBeats);
        }
    }

    void RunAttacking(double songBeats)
    {
        var pattern = archetype.attackPattern;
        if (pattern == null || pattern.totalBeats <= 0f) { DoChase(); return; }

        double localBeats = songBeats - beatOffset;
        float patternBeat = (float)(localBeats - System.Math.Floor(localBeats / pattern.totalBeats) * pattern.totalBeats);

        AttackSubState newSub = DetermineSubState(patternBeat, pattern, out int newAttackIdx);

        if (newSub != prevSubState || newAttackIdx != activeAttackIndex)
        {
            EnterSubState(newSub, newAttackIdx);
            prevSubState = newSub;
            activeAttackIndex = newAttackIdx;
        }

        switch (newSub)
        {
            case AttackSubState.Chasing: DoChase(); break;
            case AttackSubState.Telegraphing: DoTelegraph(); break;
            case AttackSubState.Striking: DoStrike(songBeats); break;
        }
    }

    AttackSubState DetermineSubState(float patternBeat, EnemyAttackPattern pattern, out int attackIdx)
    {
        attackIdx = -1;
        if (!InAttackRange()) return AttackSubState.Chasing;

        for (int i = 0; i < pattern.attacks.Count; i++)
        {
            var atk = pattern.attacks[i];
            float telegraphStart = atk.beat - atk.telegraphBeats;
            float strikeEnd = atk.beat + archetype.strikeDurationBeats;
            if (patternBeat >= telegraphStart && patternBeat < strikeEnd)
            {
                attackIdx = i;
                return patternBeat < atk.beat ? AttackSubState.Telegraphing : AttackSubState.Striking;
            }
        }

        return AttackSubState.Chasing;
    }

    void EnterSubState(AttackSubState s, int attackIdx)
    {
        switch (s)
        {
            case AttackSubState.Chasing:
                ApplyColor(archetype.baseColor);
                break;
            case AttackSubState.Telegraphing:
                rb.linearVelocity = Vector3.zero;
                ApplyColor(archetype.telegraphColor);
                break;
            case AttackSubState.Striking:
                ApplyColor(archetype.telegraphColor);
                SetupStrike(attackIdx);
                break;
        }
    }

    void SetupStrike(int attackIdx)
    {
        playerHitThisStrike = false;
        Vector3 dir = transform.forward;
        strikeStartPos = transform.position;
        strikeEndPos = strikeStartPos + dir * archetype.strikeReach;
        if (attackIdx >= 0 && attackIdx < archetype.attackPattern.attacks.Count)
        {
            var atk = archetype.attackPattern.attacks[attackIdx];
            strikeStartBeat = atk.beat;
            strikeEndBeat = atk.beat + archetype.strikeDurationBeats;
        }
    }

    void EnterPhase(Phase next, double songBeats)
    {
        CurrentPhase = next;
        phaseEnteredBeat = songBeats;
        prevSubState = AttackSubState.Chasing;
        activeAttackIndex = -1;
        playerHitThisStrike = false;

        switch (next)
        {
            case Phase.Attacking:
                Pressure = 0f;
                ApplyColor(archetype.baseColor);
                break;
            case Phase.Stunned:
                Pressure = 0f;
                rb.linearVelocity = Vector3.zero;
                ApplyColor(archetype.vulnerableColor);
                break;
            case Phase.Ritualling:
                rb.linearVelocity = Vector3.zero;
                ApplyColor(archetype.vulnerableColor);
                break;
            case Phase.Resolving:
                rb.linearVelocity = Vector3.zero;
                ApplyColor(archetype.vulnerableColor);
                break;
        }
    }

    void DoChase()
    {
        if (target == null) { rb.linearVelocity = Vector3.zero; return; }
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        if (dist <= archetype.attackRange * 0.9f) { rb.linearVelocity = Vector3.zero; return; }
        Vector3 dir = to.normalized;
        rb.linearVelocity = dir * archetype.moveSpeed;
        if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
    }

    void DoTelegraph()
    {
        if (target == null) return;
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(to.normalized);
    }

    void DoStrike(double songBeats)
    {
        double total = strikeEndBeat - strikeStartBeat;
        if (total > 0.0)
        {
            float progress = Mathf.Clamp01((float)((songBeats - strikeStartBeat) / total));
            rb.MovePosition(Vector3.Lerp(strikeStartPos, strikeEndPos, progress));
        }

        if (playerHitThisStrike) return;

        if (archetype.attackPattern != null
            && activeAttackIndex >= 0
            && activeAttackIndex < archetype.attackPattern.attacks.Count
            && archetype.attackPattern.attacks[activeAttackIndex].kind != AttackKind.Melee)
        {
            return;
        }

        Vector3 checkPos = transform.position + transform.forward * (archetype.strikeRadius * 0.5f);
        var hits = Physics.OverlapSphere(checkPos, archetype.strikeRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag(targetTag)) continue;
            if (playerHitThisStrike) continue;
            var parry = hits[i].GetComponentInParent<PlayerParry>();
            if (parry != null && parry.IsActive)
            {
                playerHitThisStrike = true;
                AddPressure(parry.LastParryWasOnBeat);
                continue;
            }
            var hp = hits[i].GetComponentInParent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(archetype.strikeDamage);
                playerHitThisStrike = true;
            }
        }
    }

    void AddPressure(bool onBeat)
    {
        if (CurrentPhase != Phase.Attacking || archetype == null) return;
        float p = onBeat ? archetype.pressurePerOnBeatParry : archetype.pressurePerOffBeatParry;
        Pressure = Mathf.Clamp(Pressure + p, 0f, archetype.pressureThreshold);
    }

    public void NotifyRitualOpened()
    {
        if (CurrentPhase != Phase.Stunned) return;
        double sb = Conductor.I != null ? Conductor.I.SongBeats : 0.0;
        EnterPhase(Phase.Ritualling, sb);
    }

    public void NotifyRitualClosed()
    {
        if (CurrentPhase != Phase.Ritualling) return;
        double sb = Conductor.I != null ? Conductor.I.SongBeats : 0.0;
        EnterPhase(Phase.Resolving, sb);
    }

    public void Purify(float amount)
    {
        if (!IsVulnerable) return;
        health.TakeDamage(amount);
    }

    void OnDeath() { Destroy(gameObject); }

    void ApplyColor(Color c)
    {
        if (mr != null) mr.material.color = c;
    }

    bool InAttackRange()
    {
        if (target == null) return false;
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        return to.sqrMagnitude <= archetype.attackRange * archetype.attackRange;
    }
}
