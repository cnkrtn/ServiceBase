using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class PlayFromSceneAutoReturn
    {
        private const string TargetScenePath = "Assets/Scenes/MainScene.unity";
        private const string PreviousSceneKey = "ScenePlaySwitcher.PreviousScene";

        static PlayFromSceneAutoReturn()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var prevScene = EditorPrefs.GetString(PreviousSceneKey, "");
                if (!string.IsNullOrEmpty(prevScene))
                {
                    EditorPrefs.DeleteKey(PreviousSceneKey);
                    EditorSceneManager.OpenScene(prevScene);
                }
            }
        }

        public static void PlayFromTargetScene()
        {
            if (EditorApplication.isPlaying)
                return;

            string currentScene = SceneManager.GetActiveScene().path;
            EditorPrefs.SetString(PreviousSceneKey, currentScene);

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(TargetScenePath);
                EditorApplication.isPlaying = true;
            }
        }
    }

    [EditorToolbarElement(id)]
    public class PlayFromSceneButton : EditorToolbarButton
    {
        public const string id = "ScenePlaySwitcher/PlayFromSceneButton";

        public PlayFromSceneButton()
        {
            text = "▶ GameStart";
            tooltip = "Play from GameStartScene";
            clicked += PlayFromSceneAutoReturn.PlayFromTargetScene;
        }
    }

    [Overlay(typeof(UnityEditor.SceneView), "Play From Scene", true)]
    public class PlayFromSceneOverlay : ToolbarOverlay
    {
        public PlayFromSceneOverlay() : base(PlayFromSceneButton.id) { }
    }
}