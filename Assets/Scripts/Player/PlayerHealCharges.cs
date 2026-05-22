using System;
using UnityEngine;

public class PlayerHealCharges : MonoBehaviour
{
    public int maxCharges = 3;
    public int startingCharges = 2;

    public int Current { get; private set; }
    public bool HasAny => Current > 0;
    public event Action Changed;

    void Awake() { Current = Mathf.Clamp(startingCharges, 0, maxCharges); }

    public bool TrySpend()
    {
        if (Current <= 0) return false;
        Current--;
        Changed?.Invoke();
        return true;
    }

    public void Grant(int n = 1)
    {
        Current = Mathf.Min(maxCharges, Current + n);
        Changed?.Invoke();
    }
}
