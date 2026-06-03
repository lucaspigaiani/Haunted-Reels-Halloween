using UnityEngine;

[CreateAssetMenu(fileName = "SymbolSystem", menuName = "Haunted Reels/Symbol System")]
public class SymbolSystem : ScriptableObject
{
    public enum SymbolType
    {
        H1,
        H2,
        H3,
        L1,
        L2,
        L3,
        Wild
    }

    [Header("Symbol Type")]
    public SymbolType Type;

    [Header("Probability")]
    [Range(0, 100)]
    public int Weight;

    [Header("Sprite")]
    public Sprite Sprite;

    [Header("Spine Skin")]
    public string SpineSkin;

    [Header("Payout Values")]
    public float Multiplier3;
    public float Multiplier4;
    public float Multiplier5;
}