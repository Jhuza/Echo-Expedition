using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour, Interactable {

    [SerializeField] private string sceneName;
    [SerializeField] private string promptText = "Entrar al nivel";

    public void Interact() {
        SceneManager.LoadScene(sceneName);
    }

    public string GetPromptText() {
        return $"[E] {promptText}";
    }
}