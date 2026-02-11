using UnityEngine;

[CreateAssetMenu(fileName = "CardConfig", menuName = "Game/CardConfig", order = 1)]
public class CardConfig : ScriptableObject
{
    public Sprite CardBack;
    public Sprite[] Cards;
    
    [Tooltip("Controls the spacing between the cards on the stack")]
    public AnimationCurve OffsetCurve;

    public int MaxCards => Cards.Length;
}
