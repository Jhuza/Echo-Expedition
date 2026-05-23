using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventario/Item")]
public class ItemData : ScriptableObject {
    public string itemName;
    public Sprite icon;
    public string description;
    
}