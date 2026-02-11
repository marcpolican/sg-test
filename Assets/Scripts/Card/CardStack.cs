using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CardStack : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshPro textCount;
    [SerializeField] private AnimationCurve cardStackOffsetCurve;

    private CardConfig cardConfig;
    private Stack<SpriteRenderer> stackSprites = new();

    public int Count => stackSprites.Count;

    public SpriteRenderer PopTopCard()
    {
        var sr = stackSprites.Pop();
        sr.transform.SetParent(null);
        UpdateTextCount();
        return sr;
    }

    public void PushCard(SpriteRenderer sr)
    {
        sr.transform.SetParent(container);
        sr.transform.SetAsLastSibling();
        sr.transform.localPosition = GetCardLocalPos(stackSprites.Count);

        sr.sortingOrder = stackSprites.Count;
        stackSprites.Push(sr);
        AdjustPositionOfCards();
        UpdateTextCount();
    }

    public Vector3 GetPositionOnTopOfStack()
    {
        return transform.position + GetCardLocalPos(stackSprites.Count);
    }

    public void Clear()
    {
        container.DestroyChildren();
        stackSprites.Clear();
        UpdateTextCount();
    }

    public void Populate()
    {
        for (int i = cardConfig.Cards.Length - 1; i >= 0; i--)
        {
            var cardSprite = cardConfig.Cards[i];
            if (cardSprite == null) continue;

            var go = new GameObject(cardSprite.name);
            if (go == null) continue;

            go.transform.SetParent(container);
            go.transform.ResetTransformation();
            go.transform.SetAsLastSibling();
            go.transform.localPosition = GetCardLocalPos(stackSprites.Count);

            var sr = go.AddComponent<SpriteRenderer>();
            if (sr == null) continue;

            sr.sprite = cardSprite;
            sr.sortingOrder = stackSprites.Count;
            stackSprites.Push(sr);
        }

        UpdateTextCount();
    }

    private void Awake()
    {
        cardConfig = Resources.Load<CardConfig>("CardConfig");
        if (cardConfig == null)
        {
            Debug.LogError("CardConfig cannot be loaded");
        }
    }

    private void AdjustPositionOfCards()
    {
        Vector3 startPos = container.position;
        int childCount = container.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var childTransform = container.GetChild(i);
            childTransform.localPosition = GetCardLocalPos(i);
            childTransform.gameObject.SetActive(true);
        }
    }

    private Vector3 GetCardLocalPos(int index)
    {
        Vector3 localPos = Vector3.zero;
        localPos.y = cardStackOffsetCurve.Evaluate((float) index / (float) cardConfig.MaxCards);
        return localPos;
    }

    private void UpdateTextCount()
    {
        textCount.text = stackSprites.Count.ToString();
    }
}
