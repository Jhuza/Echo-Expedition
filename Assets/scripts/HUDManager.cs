using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("HP")]
    public Slider hpBar;

    [Header("Materiales")]
    public TextMeshProUGUI materialsText;
    private int materialsCount = 0;
    private int materialsMax = 2;

    [Header("Bombas")]
    public TextMeshProUGUI bombText;

    void Start()
    {
        UpdateMaterials(0);
        UpdateBombs(3);
    }

    // HP
    public void UpdateHP(int current, int max)
    {
        hpBar.maxValue = max;
        hpBar.value = current;
    }

    // Materiales
    public void UpdateMaterials(int count)
    {
        materialsCount = count;
        materialsText.text = "⬡ " + materialsCount + " / " + materialsMax;
    }

    // Bombas
    public void UpdateBombs(int count)
    {
        bombText.text = "◉ x" + count;
    }
}