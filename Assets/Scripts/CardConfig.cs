using UnityEngine;

[CreateAssetMenu(fileName = "CardConfig", menuName = "Game/CardConfig", order = 1)]
public class CardConfig : ScriptableObject
{
    public Sprite CardBack;
    public Sprite[] Cards;
}
