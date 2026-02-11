using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardTable : MonoBehaviour
{
    [SerializeField] private UIMessageBox uiMessageBox;

    [SerializeField] private Transform animMidPoint;
    [SerializeField] private AnimationCurve cardStackOffsetCurve;
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
    public event Action<int, int> OnCountChanged;

    public void Initialize()
    {
        StartCoroutine(InitializeCoroutine());
    }

    public void TogglePlay()
    {
        if (!CanPlay)
            return;

        IsPlaying = !IsPlaying;

        if (IsPlaying)
            MoveNextCardAnimation();
    }

    public void ToggleSpeed()
    {
        CurrentSpeed++;
        if (CurrentSpeed > 4)
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

    private void MoveNextCardAnimation()
    {
        if (!CanPlay) return;
        if (moveSequence != null && moveSequence.IsPlaying()) return;

        var card = stackLeft.PopTopCard();
        if (card == null) return;

        card.sortingOrder = cardConfig.MaxCards;

        Vector3 destPos = stackRight.GetPositionOnTopOfStack();
        float halfDuration = moveCardDuration * 0.5f;

        // Create DOTween sequence to move the card from left to right stacks
        moveSequence = DOTween.Sequence();

        moveSequence.Append(
                card.transform.DOMove(animMidPoint.position, halfDuration)
                .SetEase(Ease.InOutQuart));

        moveSequence.Insert(
                0.0f,
                card.transform.DOScale(scaleMidPoint, halfDuration).SetEase(scaleEaseCurve));

        moveSequence.Append(
                card.transform.DOMove(destPos, halfDuration)
                .SetEase(Ease.InOutQuart));

        moveSequence.Insert(
                halfDuration,
                card.transform.DOScale(Vector3.one, halfDuration));

        moveSequence.Play().OnComplete(() => 
        {
            stackRight.PushCard(card);
            moveSequence = null;

            if (IsPlaying)
            {
                if (CanPlay) 
                {
                    // TODO: improve on this if I have time
                    // I don't really like that we're calling it again from here
                    MoveNextCardAnimation();
                }
                else
                {
                    // We're done with all the cards
                    CurrentSpeed = 1;
                    IsPlaying = false;
                    uiMessageBox.gameObject.SetActive(true);
                }
            }
        });
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

    private IEnumerator InitializeCoroutine()
    {
        CurrentSpeed = 1;
        IsPlaying = false;
        uiMessageBox.gameObject.SetActive(false);

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
}

