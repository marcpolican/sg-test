using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardTable : MonoBehaviour
{
    [SerializeField] private UIMessageBox uiMessageBox;

    [SerializeField] private Transform containerLeft;
    [SerializeField] private Transform containerRight;
    [SerializeField] private Transform animMidPoint;
    [SerializeField] private AnimationCurve cardStackOffsetCurve;
    [SerializeField] private AnimationCurve scaleEaseCurve;
    [SerializeField] private float moveCardDuration = 1.0f;
    [SerializeField] private Vector3 scaleMidPoint = new Vector3(1.2f, 1.2f, 1.0f);

    private CardConfig cardConfig;
    private Stack<SpriteRenderer> cardsLeft = new();
    private Stack<SpriteRenderer> cardsRight = new();
    private Sequence moveSequence;

    public bool CanPlay => cardsLeft.Count > 0;

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

    private void TriggerCountChanged()
    {
        OnCountChanged?.Invoke(cardsLeft.Count, cardsRight.Count);
    }

    private void MoveNextCardAnimation()
    {
        if (!CanPlay) return;
        if (moveSequence != null && moveSequence.IsPlaying()) return;

        var card = cardsLeft.Pop();
        TriggerCountChanged();
        card.sortingOrder = cardConfig.MaxCards;

        // Figure out destination
        Vector3 destPos = containerRight.position;
        int childCount = containerRight.childCount;
        if (childCount > 0)
        {
            destPos += GetCardLocalPos(childCount);
        }

        moveSequence = DOTween.Sequence();

        float halfDuration = moveCardDuration * 0.5f;

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
            card.transform.SetParent(containerRight);
            card.transform.SetAsLastSibling();
            cardsRight.Push(card);
            TriggerCountChanged();
            card.sortingOrder = cardsRight.Count;
            AdjustPositionOfCards(containerRight);
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
        ClearContainers();
        yield return null; 

        PopulateLeft();
        AdjustPositionOfCards(containerLeft);
    }

    private void ClearContainers()
    {
        cardsLeft.Clear();
        cardsRight.Clear();
        containerLeft.DestroyChildren();
        containerRight.DestroyChildren();
    }

    private void PopulateLeft()
    {
        for (int i = cardConfig.Cards.Length - 1; i >= 0; i--)
        {
            var cardSprite = cardConfig.Cards[i];
            if (cardSprite == null) continue;

            var go = new GameObject(cardSprite.name);
            if (go == null) continue;

            go.transform.SetParent(containerLeft);
            go.transform.ResetTransformation();
            go.transform.SetAsLastSibling();
            go.SetActive(false);

            var sr = go.AddComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.sprite = cardSprite;
            cardsLeft.Push(sr);
            sr.sortingOrder = cardsLeft.Count;
        }

        TriggerCountChanged();
    }

    private void AdjustPositionOfCards(Transform container)
    {
        Vector3 startPos = container.position;

        int childCount = container.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var transform = container.GetChild(i);
            transform.localPosition = GetCardLocalPos(i);
            transform.gameObject.SetActive(true);
        }
    }

    private Vector3 GetCardLocalPos(int index)
    {
        Vector3 localPos = Vector3.zero;
        localPos.y = cardStackOffsetCurve.Evaluate((float) index / (float) cardConfig.MaxCards);
        return localPos;
    }
}

