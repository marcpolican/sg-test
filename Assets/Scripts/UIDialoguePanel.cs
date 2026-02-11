using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDialoguePanel : MonoBehaviour
{
    [SerializeField] private Image imageAvatar;
    [SerializeField] private TextMeshProUGUI textDialogue;

    public void SetData(DialogueInfo dialogue, AvatarInfo avatar)
    {
        textDialogue.text = dialogue.text;
        imageAvatar.sprite = avatar.sprite;
    }
}
