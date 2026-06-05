using UnityEngine;

[CreateAssetMenu(fileName = "SymbolSystem", menuName = "Haunted Reels/Symbol System")]
public class SymbolSystem : ScriptableObject
{
    public enum SymbolType
    {
        H1,     // Bruxa - Alto valor
        H2,     // Abóbora - Alto valor
        H3,     // Caveira - Alto valor
        L1,     // Morcego - Baixo valor
        L2,     // Aranha - Baixo valor
        L3,     // Poção - Baixo valor
        Wild    // Fantasma - Coringa
    }

    [Header("Symbol Type")]
    public SymbolType Type;

    [Header("Probability")]
    [Range(0, 100)]
    public int Weight;          // Peso para sorteio (maior = mais frequente)

    [Header("Sprite")]
    public Sprite Sprite;       // Sprite para símbolos de baixo valor

    [Header("Spine Skin")]
    public string SpineSkin;    // Nome da skin Spine para símbolos de alto valor

    [Header("Payout Values")]
    public float Multiplier3;   // Multiplicador para 3 símbolos
    public float Multiplier4;   // Multiplicador para 4 símbolos
    public float Multiplier5;   // Multiplicador para 5 símbolos
}