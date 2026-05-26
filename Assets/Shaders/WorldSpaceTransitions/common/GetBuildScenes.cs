#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Dropdown))]
    public class GetBuildScenes : MonoBehaviour
    {

        private Dropdown sceneDropdown;

        private void Start()
        {
            sceneDropdown = GetComponent<Dropdown>();
            FindAllBuildScenes();
            //sceneDropdown.onValueChanged.AddListener(delegate { SwitchScene(sceneDropdown.value); });
        }

        void FindAllBuildScenes()
        {
            List<string> sceneNames = new List<string>();
            foreach (EditorBuildSettingsScene sc in EditorBuildSettings.scenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(sc.path);
                //Debug.Log(sceneName);
                sceneName = sceneName.Replace("_", " ");
                char[] letters = sceneName.ToCharArray();
                if (sceneName == string.Empty) continue;
                // upper case the first char
                letters[0] = char.ToUpper(letters[0]);
                // return the array made of the new char array
                sceneName = new string(letters);
                if (sc.enabled) sceneNames.Add(sceneName);
            }
            sceneDropdown.ClearOptions();
            sceneDropdown.AddOptions(sceneNames);
        }
        private void OnEnable()
        {
            sceneDropdown = GetComponent<Dropdown>();
            FindAllBuildScenes();
        }
    }
}
#endif