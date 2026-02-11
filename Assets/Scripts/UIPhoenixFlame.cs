using UnityEngine;

public class UIPhoenixFlame : UIBase
{
    [SerializeField] private Animator flameAnimator;

    public void OnButtonTriggerAnimation(string triggerName)
    {
        flameAnimator.SetTrigger(triggerName);
    }

    private void Start()
    {
        flameAnimator.gameObject.SetActive(true);
    }

}
