using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIMessageBox : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelContent;
    [SerializeField] private float animDuration = 0.33f;

    public void OnButtonDismiss()
    {
        canvasGroup.DOFade(0.0f, animDuration * 0.5f);
        panelContent.DOScale(0.0f, animDuration * 0.5f)
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, animDuration);

        panelContent.localScale = Vector3.zero;
        panelContent.DOScale(1.0f, animDuration).SetEase(Ease.OutBack);
    }

}
