using System;
using UnityEngine;
using DG.Tweening;

public class CardAnimation : MonoBehaviour
{
    private Sequence moveSequence;

    public bool IsPlaying { get; private set; }

    public void MoveToOtherDeck(Transform destTransform, Action onComplete)
    {
        if (destTransform == null) return;
        
    }
}
