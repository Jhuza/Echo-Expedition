using UnityEngine;

public class DialogueManager : MonoBehaviour {

    public static DialogueManager Instance;

    private string[] currentLines;
    private string currentNPCName;
    private int currentIndex = 0;
    private bool isOpen = false;

    private void Awake() {
        Instance = this;
    }

    public void StartDialogue(string npcName, string[] lines) {
        currentNPCName = npcName;
        currentLines = lines;
        currentIndex = 0;
        isOpen = true;
    }

    private void Update() {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.R)) {
            currentIndex++;
            if (currentIndex >= currentLines.Length) {
                isOpen = false; // Se acabó el diálogo
            }
        }
    }

    private void OnGUI() {
        if (!isOpen) return;

        // Medidas responsivas 16:9
        float boxW = Screen.width * 0.7f;
        float boxH = Screen.height * 0.25f;
        float boxX = Screen.width / 2 - boxW / 2;
        float boxY = Screen.height * 0.72f;

        // Fondo de la caja
        GUI.Box(new Rect(boxX, boxY, boxW, boxH), "");

        // Nombre del NPC
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.03f);
        nameStyle.normal.textColor = Color.yellow;
        nameStyle.fontStyle = FontStyle.Bold;

        GUI.Label(
            new Rect(boxX + Screen.width * 0.02f, boxY + Screen.height * 0.01f, boxW, Screen.height * 0.04f),
            currentNPCName,
            nameStyle
        );

        // Texto del diálogo
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.025f);
        textStyle.normal.textColor = Color.white;
        textStyle.wordWrap = true;

        GUI.Label(
            new Rect(boxX + Screen.width * 0.02f, boxY + Screen.height * 0.06f, boxW - Screen.width * 0.04f, boxH - Screen.height * 0.08f),
            currentLines[currentIndex],
            textStyle
        );

        // Indicador de continuar
        GUIStyle continueStyle = new GUIStyle();
        continueStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.02f);
        continueStyle.normal.textColor = Color.gray;
        continueStyle.alignment = TextAnchor.LowerRight;

        string continueText = currentIndex < currentLines.Length - 1 ? "[R] Continuar" : "[R] Cerrar";
        GUI.Label(
            new Rect(boxX, boxY + boxH - Screen.height * 0.04f, boxW - Screen.width * 0.02f, Screen.height * 0.03f),
            continueText,
            continueStyle
        );
    }
}