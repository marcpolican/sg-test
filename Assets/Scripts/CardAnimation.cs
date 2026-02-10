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
        
        Vector3 destPos = Vector3.zero;
        int childCount = destTransform.childCount;

        if (childCount > 0)
        {
            destPos = destTransform.GetChild(childCount-1).position;
        }
        else
        {
            destPos = destTransform.position;
        }

        moveSequence = DOTween.Sequence();
        moveSequence.Append(transform.DOMove(Vector3.zero, 0.5f));
        moveSequence.Append(transform.DOMove(destPos, 0.5f));
        moveSequence.Play().OnComplete(() => 
        {
            onComplete?.Invoke();
        });
    }
}
