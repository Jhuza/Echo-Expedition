using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD basado en los assets del Space Game GUI Kit.
///
/// Uso:
///   1. Crea un GameObject vacío "HUDManager" en la escena.
///   2. Añade este script.
///   3. Asigna los sprites y prefabs del kit en el Inspector. Los campos
///      marcados como "(opcional)" tienen fallback programático si los dejas
///      en None.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    // -------- SPRITES (Inspector) --------

    [Header("Sprites - Inventario")]
    [Tooltip("Background del panel grande. Recomendado: Containers/Large/shop-container-large")]
    [SerializeField] private Sprite panelBackgroundSprite;
    [Tooltip("Slot del grid. Recomendado: Grid_Components/Large/inventory-item-container copy-large")]
    [SerializeField] private Sprite slotBackgroundSprite;
    [Tooltip("Slot vacío (opcional, más tenue). Mismo sprite que el normal con alpha bajo si lo dejas en None")]
    [SerializeField] private Sprite slotEmptySprite;

    [Header("Sprites - HP / Bombas / Prompt")]
    [Tooltip("Corazón lleno. Icons/heart-128")]
    [SerializeField] private Sprite heartFullSprite;
    [Tooltip("Corazón vacío. Icons/empty-heart-128")]
    [SerializeField] private Sprite heartEmptySprite;
    [Tooltip("Icono bomba. Icons/commet-128 o el que prefieras")]
    [SerializeField] private Sprite bombIconSprite;
    [Tooltip("Icono inventario. Icons/inventory-128")]
    [SerializeField] private Sprite inventoryIconSprite;
    [Tooltip("Background prompt interacción. Containers/Medium/item-info-container-medium")]
    [SerializeField] private Sprite promptPanelSprite;

    // -------- PREFABS DEL KIT (opcionales) --------

    [Header("Prefabs del kit (opcional)")]
    [Tooltip("Si se asigna, se usa el prefab del kit en lugar de construir la barra de corazones programáticamente. UI_Elements/Medium/Currency Bars/Heart_Amount_Medium")]
    [SerializeField] private GameObject heartAmountPrefab;
    [Tooltip("Si se asigna, se usa el prefab del kit como contador de bombas. UI_Elements/Medium/Currency Bars/Coin_Amount_Medium (el icono será reemplazado por el de bomba)")]
    [SerializeField] private GameObject bombAmountPrefab;

    // -------- FUENTE --------

    [Header("Fuente TMP")]
    [SerializeField] private TMP_FontAsset font;

    // -------- CONFIG --------

    [Header("Configuración")]
    [SerializeField] private int heartCount = 5;
    [SerializeField] private int inventoryColumns = 5;
    [SerializeField] private int inventoryRows = 4;
    [SerializeField] private Vector2 inventorySize = new Vector2(1100, 700);
    [SerializeField] private Vector2 slotSize = new Vector2(140, 140);
    [SerializeField] private float slotSpacing = 18f;
    [SerializeField] private KeyCode toggleInventoryKey = KeyCode.I;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color bombTextColor = new Color(1f, 0.65f, 0.15f);

    // -------- RUNTIME --------

    private Canvas canvas;
    private Image[] heartImages;            // si heartAmountPrefab == null
    private TMP_Text heartValueText;        // si heartAmountPrefab != null
    private TMP_Text bombCountText;
    private Image bombIconImage;
    private RectTransform inventoryPanel;
    private RectTransform inventoryGrid;
    private RectTransform promptPanel;
    private TMP_Text promptText;

    private bool inventoryOpen = false;
    private readonly List<GameObject> inventorySlotObjects = new();

    // ============================================================
    //  Unity lifecycle
    // ============================================================

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
        SetInventoryOpen(false);
        HidePrompt();
        UpdateBombCount(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleInventoryKey)) ToggleInventory();
    }

    // ============================================================
    //  Public API
    // ============================================================

    public void UpdateHealth(float current, float max)
    {
        if (heartValueText != null)
        {
            // modo prefab del kit: numérico
            heartValueText.text = $"{Mathf.CeilToInt(current)}";
            return;
        }

        if (heartImages == null || heartImages.Length == 0) return;
        float ratio = max > 0 ? Mathf.Clamp01(current / max) : 0f;
        int filled = Mathf.CeilToInt(ratio * heartImages.Length);
        for (int i = 0; i < heartImages.Length; i++)
            heartImages[i].sprite = i < filled ? heartFullSprite : heartEmptySprite;
    }

    public void UpdateBombCount(int count)
    {
        if (bombCountText != null) bombCountText.text = $"x{count}";
    }

    public void ShowPrompt(string text)
    {
        if (promptPanel == null) return;
        promptPanel.gameObject.SetActive(true);
        promptText.text = text;
    }

    public void HidePrompt()
    {
        if (promptPanel != null) promptPanel.gameObject.SetActive(false);
    }

    public void ToggleInventory() => SetInventoryOpen(!inventoryOpen);

    public void SetInventoryOpen(bool open)
    {
        inventoryOpen = open;
        if (inventoryPanel != null) inventoryPanel.gameObject.SetActive(open);
        if (open) RefreshInventory();
    }

    public void RefreshInventory()
    {
        // Sincroniza contador de bombas con el inventario
        if (Inventory.Instance != null)
            UpdateBombCount(Inventory.Instance.ContarBombas());

        if (inventoryGrid == null) return;
        foreach (var go in inventorySlotObjects) Destroy(go);
        inventorySlotObjects.Clear();

        var items = Inventory.Instance != null ? Inventory.Instance.GetItems() : null;
        int totalSlots = inventoryColumns * inventoryRows;

        var grouped = new Dictionary<string, (ItemData data, int qty)>();
        if (items != null)
            foreach (var it in items)
            {
                if (it == null) continue;
                if (grouped.TryGetValue(it.itemName, out var e))
                    grouped[it.itemName] = (e.data, e.qty + 1);
                else
                    grouped[it.itemName] = (it, 1);
            }

        int slotIndex = 0;
        foreach (var kvp in grouped)
        {
            if (slotIndex >= totalSlots) break;
            CreateSlot(kvp.Value.data, kvp.Value.qty);
            slotIndex++;
        }
        for (; slotIndex < totalSlots; slotIndex++) CreateSlot(null, 0);
    }

    // ============================================================
    //  Construcción del Canvas
    // ============================================================

    private void BuildCanvas()
    {
        var canvasGO = new GameObject("HUDCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        canvasGO.layer = LayerMask.NameToLayer("UI");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        BuildHealth();
        BuildBombCounter();
        BuildAttackHint();
        BuildInventoryHint();
        BuildPrompt();
        BuildInventory();
    }

    private void BuildHealth()
    {
        if (heartAmountPrefab != null)
        {
            // Usar el prefab del kit (Heart_Amount_Large)
            var inst = Instantiate(heartAmountPrefab, canvas.transform);
            inst.name = "Health";
            var rt = inst.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(55, -45);
                rt.localScale = Vector3.one;
            }
            heartValueText = FindChildText(inst.transform, "Heart Value");
            HideChild(inst.transform, "Plus Button"); // oculta el "+"
            return;
        }

        // Fallback programático: fila de corazones
        var group = CreateRect("HealthBar", canvas.transform);
        group.anchorMin = new Vector2(0, 1);
        group.anchorMax = new Vector2(0, 1);
        group.pivot = new Vector2(0, 1);
        group.anchoredPosition = new Vector2(30, -30);
        group.sizeDelta = new Vector2(64 * heartCount + 8 * (heartCount - 1), 72);

        var hg = group.gameObject.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 8;
        hg.childAlignment = TextAnchor.MiddleLeft;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;
        hg.childControlWidth = true;
        hg.childControlHeight = true;

        heartImages = new Image[heartCount];
        for (int i = 0; i < heartCount; i++)
        {
            var heart = CreateImage($"Heart{i}", group, heartFullSprite, new Vector2(64, 64));
            var le = heart.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 64; le.preferredHeight = 64;
            heartImages[i] = heart.GetComponent<Image>();
        }
    }

    private void BuildBombCounter()
    {
        // Construcción programática (el prefab del kit Coin_Amount_Large tiene
        // wrapper interno que descoloca el contenido al anclarlo top-left).
        var group = CreateRect("BombCounter", canvas.transform);
        group.anchorMin = new Vector2(0, 1);
        group.anchorMax = new Vector2(0, 1);
        group.pivot = new Vector2(0, 1);
        group.anchoredPosition = new Vector2(55, -140);
        group.sizeDelta = new Vector2(280, 84);

        var hg = group.gameObject.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 12;
        hg.childAlignment = TextAnchor.MiddleLeft;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;
        hg.childControlWidth = true;
        hg.childControlHeight = true;

        // Icono cometa/bomba
        var icon = CreateImage("BombIcon", group, bombIconSprite, new Vector2(76, 76));
        var le1 = icon.gameObject.AddComponent<LayoutElement>();
        le1.preferredWidth = 76; le1.preferredHeight = 76;
        bombIconImage = icon.GetComponent<Image>();

        // Contador "x2" (auto-ancho según contenido)
        var txtRT = CreateRect("BombCount", group);
        var le2 = txtRT.gameObject.AddComponent<LayoutElement>();
        le2.minWidth = 50; le2.preferredHeight = 72;
        var fitter = txtRT.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        bombCountText = txtRT.gameObject.AddComponent<TextMeshProUGUI>();
        bombCountText.font = font;
        bombCountText.fontSize = 56;
        bombCountText.fontStyle = FontStyles.Bold;
        bombCountText.color = bombTextColor;
        bombCountText.alignment = TextAlignmentOptions.MidlineLeft;
        bombCountText.text = "x0";

        // Chip de tecla [Q] (más grande que los hints de la derecha)
        CreateKeyChip("Q", group, 64, 36);
    }

    private void BuildAttackHint()
    {
        BuildRightHint("AttackHint", null, "Atacar", "F", -45);
    }

    private void BuildInventoryHint()
    {
        BuildRightHint("InventoryHint", inventoryIconSprite, "Inventario", "I", -140);
    }

    /// <summary>
    /// Hint con formato [icono opcional] [texto] [chip], anclado top-right.
    /// </summary>
    private void BuildRightHint(string name, Sprite icon, string label, string key, float y)
    {
        var group = CreateRect(name, canvas.transform);
        group.anchorMin = new Vector2(1, 1);
        group.anchorMax = new Vector2(1, 1);
        group.pivot = new Vector2(1, 1);
        group.anchoredPosition = new Vector2(-55, y);
        group.sizeDelta = new Vector2(340, 60);

        var hg = group.gameObject.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 10;
        hg.childAlignment = TextAnchor.MiddleRight;
        hg.childForceExpandWidth = false;
        hg.childForceExpandHeight = false;
        hg.childControlWidth = true;
        hg.childControlHeight = true;

        if (icon != null)
        {
            var iconRT = CreateImage($"{name}Icon", group, icon, new Vector2(48, 48));
            var le1 = iconRT.gameObject.AddComponent<LayoutElement>();
            le1.preferredWidth = 48; le1.preferredHeight = 48;
            le1.minWidth = 48; le1.minHeight = 48;
        }

        var lblRT = CreateRect($"{name}Label", group);
        var le = lblRT.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 56;
        var fitter = lblRT.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        var t = lblRT.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = label;
        t.fontSize = 36;
        t.fontStyle = FontStyles.Bold;
        t.color = textColor;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.raycastTarget = false;

        CreateKeyChip(key, group);
    }

    private RectTransform CreateKeyChip(string letter, Transform parent, float size = 48, float fontSize = 28)
    {
        var chipRT = CreateRect($"Key{letter}", parent);
        var le = chipRT.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = size; le.preferredHeight = size;
        le.minWidth = size; le.minHeight = size;

        var bg = chipRT.gameObject.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);
        bg.raycastTarget = false;

        var lblRT = CreateRect("Label", chipRT);
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;

        var t = lblRT.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = letter;
        t.fontSize = fontSize;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;

        return chipRT;
    }

    private void BuildPrompt()
    {
        var panel = CreateRect("PromptPanel", canvas.transform);
        panel.anchorMin = new Vector2(0.5f, 0);
        panel.anchorMax = new Vector2(0.5f, 0);
        panel.pivot = new Vector2(0.5f, 0);
        panel.anchoredPosition = new Vector2(0, 220);
        panel.sizeDelta = new Vector2(640, 100);

        var bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = promptPanelSprite;
        bg.type = Image.Type.Simple;
        bg.color = promptPanelSprite != null ? Color.white : new Color(0, 0, 0, 0.65f);
        bg.raycastTarget = false;

        var txtRT = CreateRect("PromptText", panel);
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(30, 14);
        txtRT.offsetMax = new Vector2(-30, -14);

        promptText = txtRT.gameObject.AddComponent<TextMeshProUGUI>();
        promptText.font = font;
        promptText.fontSize = 36;
        promptText.fontStyle = FontStyles.Bold;
        promptText.color = textColor;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.text = "";

        promptPanel = panel;
    }

    private void BuildInventory()
    {
        var panel = CreateRect("InventoryPanel", canvas.transform);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = inventorySize;

        var bg = panel.gameObject.AddComponent<Image>();
        bg.sprite = panelBackgroundSprite;
        bg.type = Image.Type.Simple;
        bg.color = panelBackgroundSprite != null ? Color.white : new Color(0.1f, 0.1f, 0.15f, 0.94f);

        // Title
        var titleRT = CreateRect("Title", panel);
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -30);
        titleRT.sizeDelta = new Vector2(-60, 80);

        var title = titleRT.gameObject.AddComponent<TextMeshProUGUI>();
        title.font = font;
        title.fontSize = 54;
        title.fontStyle = FontStyles.Bold;
        title.color = textColor;
        title.alignment = TextAlignmentOptions.Center;
        title.text = "Inventario";

        // Grid
        var gridRT = CreateRect("Grid_Content", panel);
        gridRT.anchorMin = new Vector2(0, 0);
        gridRT.anchorMax = new Vector2(1, 1);
        gridRT.pivot = new Vector2(0.5f, 0.5f);
        gridRT.offsetMin = new Vector2(60, 70);
        gridRT.offsetMax = new Vector2(-60, -130);

        var grid = gridRT.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = new Vector2(slotSpacing, slotSpacing);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = inventoryColumns;

        inventoryGrid = gridRT;

        inventoryPanel = panel;
    }

    private void CreateSlot(ItemData data, int qty)
    {
        var slotGO = new GameObject("Inventory_Block", typeof(RectTransform));
        slotGO.transform.SetParent(inventoryGrid, false);
        inventorySlotObjects.Add(slotGO);

        // background del slot
        var bg = slotGO.AddComponent<Image>();
        bool hasItem = data != null;
        bg.sprite = !hasItem && slotEmptySprite != null ? slotEmptySprite : slotBackgroundSprite;
        bg.type = Image.Type.Simple;
        bg.color = bg.sprite != null
            ? new Color(1, 1, 1, hasItem ? 1f : 0.55f)
            : new Color(0.2f, 0.2f, 0.25f, hasItem ? 0.9f : 0.4f);
        bg.raycastTarget = false;

        if (!hasItem) return;

        // icono
        if (data.icon != null)
        {
            var iconRT = CreateRect("Icon", slotGO.transform);
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = slotSize * 0.7f;
            var img = iconRT.gameObject.AddComponent<Image>();
            img.sprite = data.icon;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        else
        {
            var lblRT = CreateRect("Letter", slotGO.transform);
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
            var t = lblRT.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = (data.itemName != null && data.itemName.Length > 0) ? data.itemName[0].ToString().ToUpper() : "?";
            t.fontSize = 64;
            t.fontStyle = FontStyles.Bold;
            t.color = textColor;
            t.alignment = TextAlignmentOptions.Center;
        }

        // badge cantidad
        if (qty > 1)
        {
            var badgeRT = CreateRect("Qty", slotGO.transform);
            badgeRT.anchorMin = new Vector2(1, 0);
            badgeRT.anchorMax = new Vector2(1, 0);
            badgeRT.pivot = new Vector2(1, 0);
            badgeRT.anchoredPosition = new Vector2(-8, 8);
            badgeRT.sizeDelta = new Vector2(64, 36);
            var t = badgeRT.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = $"x{qty}";
            t.fontSize = 30;
            t.fontStyle = FontStyles.Bold;
            t.color = textColor;
            t.alignment = TextAlignmentOptions.BottomRight;
            t.raycastTarget = false;
        }
    }

    // ============================================================
    //  Helpers
    // ============================================================

    private RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private RectTransform CreateImage(string name, Transform parent, Sprite sprite, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (sprite == null) img.color = new Color(1, 1, 1, 0.25f);
        return rt;
    }

    private TMP_Text FindChildText(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    private Image FindChildImage(Transform root, string name)
    {
        foreach (var img in root.GetComponentsInChildren<Image>(true))
            if (img.gameObject.name == name) return img;
        return null;
    }

    private void HideChild(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.gameObject.name == name) { t.gameObject.SetActive(false); return; }
    }
}
