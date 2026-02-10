using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTable : MonoBehaviour
{
    [SerializeField] private Transform containerLeft;
    [SerializeField] private Transform containerRight;
    [SerializeField] private AnimationCurve cardStackOffsetCurve;

    private CardConfig cardConfig;
    private Stack<SpriteRenderer> cardsLeft = new();
    private Stack<SpriteRenderer> cardsRight = new();

    public void Initialize()
    {
        StartCoroutine(InitializeCoroutine());
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

        StartCoroutine(AdjustPositionOfCards(containerLeft));
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

    private IEnumerator AdjustPositionOfCards(Transform container, bool animate = true)
    {
        Vector3 startPos = container.position;

        int childCount = container.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var transform = container.GetChild(i);
            startPos.y += cardStackOffsetCurve.Evaluate((float) i / (float) childCount);
            transform.position = startPos;
            transform.gameObject.SetActive(true);

            if (animate)
                yield return null;
        }

        yield break;
    }

}

