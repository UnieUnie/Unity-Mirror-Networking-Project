using System.Collections;
using UnityEngine;

public class PuzzleResultDank : MonoBehaviour
{
    PuzzleResult puzzleResult;
    bool isComplete;

    void Start()
    {
        puzzleResult = GetComponent<PuzzleResult>();
    }
    void FixedUpdate()
    {
        if (isComplete) return;
        isComplete = puzzleResult.completeState;
        OnComplete();
    }

    void OnComplete()
    {
        
    }
}
