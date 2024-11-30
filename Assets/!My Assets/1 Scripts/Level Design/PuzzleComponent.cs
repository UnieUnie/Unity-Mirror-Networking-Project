using UnityEngine;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// Base class for all puzzle components.
/// This is used to connect all components.
/// </summary>
public abstract class PuzzleComponent : NetworkBehaviour
{
    [SerializeField] protected List<PuzzleComponent> connectedOutputs;

    //[SyncVar]
    protected bool outputValue;

    public bool GetOutputValue() => outputValue;

    protected virtual void ProcessOutput()
    {
        foreach (var component in connectedOutputs)
        {
            component?.ReceiveSignal();
        }
    }

    public virtual void ReceiveSignal()
    {

    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}