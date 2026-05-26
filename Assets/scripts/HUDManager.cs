using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("HP")]
    public Slider hpBar;
    public TextMeshProUGUI hpText;

    [Header("Materiales")]
    public TextMeshProUGUI materialsText;
    private int materialsMax = 2;

    [Header("Bombas")]
    public TextMeshProUGUI bombText;

    [Header("Inventario Panel")]
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryContent;

    [Header("Dialogo")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueNPCName;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueContinueHint;

    [Header("Interaccion")]
    public TextMeshProUGUI interactPrompt;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateMaterials(0);
        UpdateBombs(0);
        HideDialogue();
        HideInteractPrompt();
        if (inventoryPanel) inventoryPanel.SetActive(false);
    }

    // ── HP ──────────────────────────────────────
    public void UpdateHP(float current, float max)
    {
        if (hpBar)
        {
            hpBar.maxValue = max;
            hpBar.value = current;
        }
        if (hpText)
            hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    // ── Materiales ───────────────────────────────
    public void UpdateMaterials(int count)
    {
        if (materialsText)
            materialsText.text = $"⬡  {count} / {materialsMax}";
    }

    // ── Bombas ───────────────────────────────────
    public void UpdateBombs(int count)
    {
        if (bombText)
            bombText.text = $"◉  x{count}";
    }

    // ── Inventario ───────────────────────────────
    public void ToggleInventory(bool open, string content = "")
    {
        if (inventoryPanel) inventoryPanel.SetActive(open);
        if (inventoryContent) inventoryContent.text = content;
    }

    // ── Dialogo ──────────────────────────────────
    public void ShowDialogue(string npcName, string line, bool hasMore)
    {
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (dialogueNPCName) dialogueNPCName.text = npcName;
        if (dialogueText) dialogueText.text = line;
        if (dialogueContinueHint)
            dialogueContinueHint.text = hasMore ? "[R] Continuar" : "[R] Cerrar";
    }

    public void HideDialogue()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }

    // ── Prompt interaccion ───────────────────────
    public void ShowInteractPrompt(string text)
    {
        if (interactPrompt)
        {
            interactPrompt.gameObject.SetActive(true);
            interactPrompt.text = text;
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt)
            interactPrompt.gameObject.SetActive(false);
    }
}