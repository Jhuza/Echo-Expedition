using UnityEngine;

public enum ItemType {
    Material,
    Bomba
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject {
    public string itemName;
    public Sprite icon;
    public string description;
    public ItemType tipo;
    public float damage = 30f;
}