using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardTable : MonoBehaviour
{
    [SerializeField] private Transform animMidPoint;
    [SerializeField] private AnimationCurve scaleEaseCurve;
    [SerializeField] private float moveCardDuration = 1.0f;
    [SerializeField] private Vector3 scaleMidPoint = new Vector3(1.2f, 1.2f, 1.0f);

    [SerializeField] private CardStack stackLeft;
    [SerializeField] private CardStack stackRight;

    private CardConfig cardConfig;
    private Sequence moveSequence;

    public bool CanPlay => stackLeft.Count > 0;

    private bool isPlaying = false;
    public bool IsPlaying
    {
        get => isPlaying;
        private set 
        {
            isPlaying = value;
            IsPlayingChanged?.Invoke(isPlaying);
        }
    }

    private int currentSpeed = 1;
    public int CurrentSpeed 
    { 
        get => currentSpeed;
        private set
        {
            currentSpeed = value;
            Time.timeScale = currentSpeed;
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }

    public event Action<bool> IsPlayingChanged;
    public event Action<int> OnSpeedChanged;
    public event Action OnAnimationComplete;

    public void Initialize()
    {
        StartCoroutine(InitializeCoroutine());
    }

    public void TogglePlay()
    {
        if (!CanPlay)
            return;

        IsPlaying = !IsPlaying;
    }

    public void ToggleSpeed()
    {
        const int maxSpeed = 4;

        CurrentSpeed++;
        if (CurrentSpeed > maxSpeed)
            CurrentSpeed = 1;
    }

    public void CleanUpAndExit()
    {
        CurrentSpeed = 1;
        IsPlaying = false;
        if (moveSequence != null)
        {
            moveSequence.Kill();
        }
    }

    private void Start()
    {
        cardConfig = Resources.Load<CardConfig>("CardConfig");
        if (cardConfig == null)
        {
            Debug.LogError("CardConfig cannot be loaded");
            return;
        }

        Initialize();
    }

    private void Update()
    {
        if (IsPlaying)
        {
            MoveNextCardAnimation();
        }
    }

    private IEnumerator InitializeCoroutine()
    {
        CurrentSpeed = 1;
        IsPlaying = false;

        // wait for the coroutine to finish
        while (moveSequence != null && moveSequence.IsPlaying())
        {
            yield return null;
        }

        // need to wait for next frame to actually destroy the children
        stackLeft.Clear();
        stackRight.Clear();
        yield return null; 

        stackLeft.Populate();
    }

    private void MoveNextCardAnimation()
    {
        if (!CanPlay) return;
        if (moveSequence != null && moveSequence.IsPlaying()) return;

        float halfDuration = moveCardDuration * 0.5f;

        // Pop the top card from the left stack, make sure to set sorting order to the top most
        var card = stackLeft.PopTopCard();
        if (card == null) return;

        card.sortingOrder = cardConfig.MaxCards;

        // Create DOTween sequence to move the card from left to right stacks
        moveSequence = DOTween.Sequence();

        // Move to midpoint
        moveSequence.Append(
                card.transform.DOMove(animMidPoint.position, halfDuration)
                .SetEase(Ease.InOutQuart));

        // Add scale when moving to midpoint
        moveSequence.Insert(
                0.0f,
                card.transform.DOScale(scaleMidPoint, halfDuration).SetEase(scaleEaseCurve));

        // Move from midpoint to destination
        Vector3 destPos = stackRight.GetPositionOnTopOfStack();
        moveSequence.Append(
                card.transform.DOMove(destPos, halfDuration)
                .SetEase(Ease.InOutQuart));

        // Add scale back to 1 when moving to destination
        moveSequence.Insert(
                halfDuration,
                card.transform.DOScale(Vector3.one, halfDuration));

        moveSequence.Play().OnComplete(() => 
        {
            stackRight.PushCard(card);
            moveSequence = null;

            if (IsPlaying && !CanPlay)
            {
                // We're done with all the cards
                CurrentSpeed = 1;
                IsPlaying = false;
                OnAnimationComplete?.Invoke();
            }
        });
    }
}

