using UnityEngine;

public class NPCDialogue : MonoBehaviour, Interactable {

    [SerializeField] private string npcName = "NPC";
    [TextArea][SerializeField] private string[] dialogueLines;

    public void Interact() {
        DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
    }

    public string GetPromptText() {
        return $"[E] Hablar con {npcName}, [R] para continuar la interacción";
    }
}