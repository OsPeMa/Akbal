using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    public enum State { Chasing, WindUp, Strike, Recover, Vulnerable }

    [Header("Target")]
    public Transform target;
    public string targetTag = "Player";

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float attackRange = 2.2f;

    [Header("Attack")]
    public float windUpTime = 0.7f;
    public float strikeTime = 0.12f;
    public float recoverTime = 0.4f;
    public float strikeReach = 2.2f;
    public float strikeRadius = 1.0f;
    public float strikeDamage = 1.0f;

    [Header("Stamina (Vulnerability path A)")]
    public int attacksUntilTired = 3;
    public float tiredDuration = 3f;

    [Header("Dodge window (Vulnerability path B)")]
    public float dodgeVulnerableDuration = 2f;

    [Header("Colors")]
    public Color baseColor = new Color(0.7f, 0.25f, 0.25f);
    public Color telegraphColor = new Color(1f, 0.85f, 0.2f);
    public Color vulnerableColor = new Color(0.45f, 0.75f, 1f);

    public State CurrentState { get; private set; } = State.Chasing;
    public bool IsVulnerable => CurrentState == State.Vulnerable;

    Health health;
    Rigidbody rb;
    MeshRenderer mr;
    int attacksThisCycle;
    bool playerHitThisStrike;
    float pendingVulnerableDuration;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        mr = GetComponentInChildren<MeshRenderer>();
        ApplyColor(baseColor);
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
        StartCoroutine(Brain());
    }

    IEnumerator Brain()
    {
        while (true)
        {
            switch (CurrentState)
            {
                case State.Chasing:    yield return ChaseStep();    break;
                case State.WindUp:     yield return WindUpStep();   break;
                case State.Strike:     yield return StrikeStep();   break;
                case State.Recover:    yield return RecoverStep();  break;
                case State.Vulnerable: yield return VulnerableStep(); break;
            }
        }
    }

    IEnumerator ChaseStep()
    {
        ApplyColor(baseColor);
        while (CurrentState == State.Chasing)
        {
            if (target == null) { rb.linearVelocity = Vector3.zero; yield return null; continue; }
            Vector3 to = target.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist <= attackRange) { CurrentState = State.WindUp; yield break; }
            Vector3 dir = to.normalized;
            rb.linearVelocity = dir * moveSpeed;
            if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
            yield return null;
        }
    }

    IEnumerator WindUpStep()
    {
        rb.linearVelocity = Vector3.zero;
        ApplyColor(telegraphColor);
        float t = 0f;
        while (t < windUpTime)
        {
            if (target != null)
            {
                Vector3 to = target.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(to.normalized);
            }
            t += Time.deltaTime;
            yield return null;
        }
        CurrentState = State.Strike;
    }

    IEnumerator StrikeStep()
    {
        playerHitThisStrike = false;
        Vector3 dir = transform.forward;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * strikeReach;
        float t = 0f;
        while (t < strikeTime)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / strikeTime);
            rb.MovePosition(Vector3.Lerp(startPos, endPos, f));
            Vector3 checkPos = transform.position + dir * (strikeRadius * 0.5f);
            var hits = Physics.OverlapSphere(checkPos, strikeRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                if (!hits[i].CompareTag(targetTag)) continue;
                var hp = hits[i].GetComponentInParent<Health>();
                if (hp != null && !playerHitThisStrike)
                {
                    hp.TakeDamage(strikeDamage);
                    playerHitThisStrike = true;
                }
            }
            yield return null;
        }
        CurrentState = State.Recover;
    }

    IEnumerator RecoverStep()
    {
        ApplyColor(baseColor);
        rb.linearVelocity = Vector3.zero;
        attacksThisCycle++;
        bool tired = attacksThisCycle >= attacksUntilTired;
        bool missed = !playerHitThisStrike;
        yield return new WaitForSeconds(recoverTime);
        if (tired)
        {
            attacksThisCycle = 0;
            EnterVulnerable(tiredDuration);
        }
        else if (missed)
        {
            EnterVulnerable(dodgeVulnerableDuration);
        }
        else
        {
            CurrentState = State.Chasing;
        }
    }

    void EnterVulnerable(float duration)
    {
        pendingVulnerableDuration = duration;
        CurrentState = State.Vulnerable;
    }

    IEnumerator VulnerableStep()
    {
        ApplyColor(vulnerableColor);
        rb.linearVelocity = Vector3.zero;
        float duration = pendingVulnerableDuration;
        float t = 0f;
        while (t < duration && CurrentState == State.Vulnerable && health.IsAlive)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (CurrentState == State.Vulnerable)
        {
            attacksThisCycle = 0;
            CurrentState = State.Chasing;
        }
    }

    public void Purify(float amount)
    {
        if (CurrentState != State.Vulnerable) return;
        health.TakeDamage(amount);
    }

    void OnDeath() { Destroy(gameObject); }

    void ApplyColor(Color c)
    {
        if (mr != null) mr.material.color = c;
    }
}
