using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardTable : MonoBehaviour
{
    [SerializeField] private Transform containerLeft;
    [SerializeField] private Transform containerRight;
    [SerializeField] private Transform animMidPoint;
    [SerializeField] private AnimationCurve cardStackOffsetCurve;
    [SerializeField] private float moveCardDuration = 1.0f;
    [SerializeField] private Vector3 scaleMidPoint = new Vector3(1.2f, 1.2f, 1.0f);

    [SerializeField] private TextMeshProUGUI textButtonSpeed;

    private CardConfig cardConfig;
    private Stack<SpriteRenderer> cardsLeft = new();
    private Stack<SpriteRenderer> cardsRight = new();

    private Sequence moveSequence;
    private int currentSpeed = 1;

    private bool isPlayingAnimation = false;

    public void Initialize()
    {
        StartCoroutine(InitializeCoroutine());
    }

    public void PlayAnimation()
    {
        isPlayingAnimation = true;
        MoveNextCardAnimation();
    }

    public void PauseAnimation()
    {
        isPlayingAnimation = false;
    }

    public void ToggleSpeed()
    {
        currentSpeed++;
        if (currentSpeed > 3)
            currentSpeed = 1;

        Time.timeScale = currentSpeed;

        textButtonSpeed.text = $"Speed x{currentSpeed}";
    }

    private void MoveNextCardAnimation()
    {
        if (cardsLeft.Count <= 0) return;
        if (moveSequence != null && moveSequence.IsPlaying()) return;

        var card = cardsLeft.Pop();
        card.sortingOrder = cardConfig.MaxCards;

        // Figure out destination
        Vector3 destPos = containerRight.position;
        int childCount = containerRight.childCount;
        if (childCount > 0)
        {
            destPos += GetCardLocalPos(childCount);
        }

        moveSequence = DOTween.Sequence();

        moveSequence.Append(
                card.transform.DOMove(animMidPoint.position, moveCardDuration * 0.5f)
                .SetEase(Ease.InOutQuart));

        moveSequence.Insert(
                0.0f,
                card.transform.DOScale(scaleMidPoint, moveCardDuration * 0.5f)
                .SetEase(Ease.InOutQuart));

        moveSequence.Append(
                card.transform.DOMove(destPos, moveCardDuration * 0.5f) 
                .SetEase(Ease.InOutQuart));

        moveSequence.Insert(
                moveCardDuration * 0.5f,
                card.transform.DOScale(Vector3.one, moveCardDuration * 0.5f)
                .SetEase(Ease.InOutQuart));

        moveSequence.Play().OnComplete(() => 
        {
            card.transform.SetParent(containerRight);
            card.transform.SetAsLastSibling();
            cardsRight.Push(card);
            card.sortingOrder = cardsRight.Count;
            AdjustPositionOfCards(containerRight);
            moveSequence = null;

            if (isPlayingAnimation)
                MoveNextCardAnimation();
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
        ClearContainers();

        // need to wait for next frame to actually destroy the children
        yield return null; 

        PopulateLeft();

        AdjustPositionOfCards(containerLeft);
    }

    private void ClearContainers()
    {
        if (containerLeft != null)
            containerLeft.DestroyChildren();
        
        if (containerRight != null)
            containerRight.DestroyChildren();

        cardsLeft.Clear();
        cardsRight.Clear();
    }

    private void PopulateLeft()
    {
        if (containerLeft == null) 
            return;

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

