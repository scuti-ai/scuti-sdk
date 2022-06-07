using System.IO;
using UnityEngine;
using UnityEditor;
using Scuti;
using UnityEditor.Experimental.SceneManagement;


[InitializeOnLoad]
public class ScutiEditorWindow : EditorWindow
{
    private static string REPEAT = "repeat";
    private static string PREVENT = "prevent";

    private static int WINDOW_HEIGHT = 440;
    private static int BUTTON_WIDTH = 120;
    private static int BUTTON_HEIGHT = 40;
    private static string ID_LABEL = "App ID: (found on the dashboard)";

    private int type;
    private static ScutiSettings settings;
    private string developerKey;
    private string secret;
    private bool isInitialized;
    private bool showAgain = true;
    private int orientation;


    static ScutiEditorWindow()
    {
        EditorApplication.update += OnUpdate;
       
    }

    private static void OnUpdate()
    {
        EditorApplication.update -= OnUpdate;
        settings = GetOrCreateSettings();

        string preventKey = GetProjectBasedPrefsKey(PREVENT);
        string repeatKey = GetProjectBasedPrefsKey(REPEAT);
        if (!EditorPrefs.HasKey(repeatKey) || (settings == null) || string.IsNullOrEmpty(settings.developerKey))
        {
            EditorPrefs.SetInt(repeatKey, 1);
            if (!EditorPrefs.HasKey(preventKey) || EditorPrefs.GetInt(preventKey) == 1)
            {
                EditorPrefs.SetInt(preventKey, 1);
                ShowConfiguration();
            }  
        }  
    }

    private static string GetProjectBasedPrefsKey(string subkey)
    {
        var dp = Application.dataPath;
        var s = dp.Split("/"[0]);
        var projectName = s[s.Length - 2];
        string prefsKey = $"ScutiIntro{projectName}_{subkey}";
        return prefsKey;
    }

    [MenuItem("Scuti/Configuration")]
    private static void ShowConfiguration()
    {
        ScutiEditorWindow mywindow = GetWindow<ScutiEditorWindow>();
        mywindow.titleContent = new GUIContent("Scuti");
        mywindow.minSize = new Vector2(400, WINDOW_HEIGHT + 10);
        mywindow.maxSize = new Vector2(400, WINDOW_HEIGHT + 10);
        string prefsKey = GetProjectBasedPrefsKey(PREVENT);
        mywindow.showAgain = (!EditorPrefs.HasKey(prefsKey) || EditorPrefs.GetInt(prefsKey) == 1) ? true : false;
        mywindow.Show();
    }

    [MenuItem("Scuti/Documentation")]
    static void ShowDocs()
    {
        Application.OpenURL("http://mindtrust.com");
    }
    [MenuItem("Scuti/Create Prefab")]
    static void CreateGameObject()
    {
        Transform parent = Selection.activeTransform;
        if (parent != null)
        {
            Canvas canv;

            canv = parent.GetComponent<Canvas>();

            if (canv == null)
                canv = parent.GetComponentInParent<Canvas>();
       
            string prefabName = ScutiConstants.BUTTON_PREFAB_NAME;
            if (canv == null)
                prefabName = ScutiConstants.BUTTON_PREFAB_3D_NAME;

            Transform r = parent.root;

            var go = Instantiate(Resources.Load<GameObject>(prefabName));
            go.name = go.name.Substring(0, go.name.Length - 7);
            go.transform.SetParent(parent, false);

            if (PrefabUtility.GetPrefabAssetType(parent) == PrefabAssetType.Regular)
            {
                PrefabUtility.ApplyPrefabInstance(parent.gameObject, InteractionMode.AutomatedAction);
            }
            else
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(r);
                EditorUtility.SetDirty(r);
            }
        }else
        {
            var go = Instantiate(Resources.Load<GameObject>(ScutiConstants.BUTTON_PREFAB_3D_NAME));
            go.name = go.name.Substring(0, go.name.Length - 7);
        }
    }

    [MenuItem("Scuti/Settings")]
    static void ShowSettings()
    {
        if (settings == null)
            settings = GetOrCreateSettings();
        AssetDatabase.OpenAsset(settings);
    }

    private static ScutiSettings GetOrCreateSettings()
    {
        var results = AssetDatabase.FindAssets("t:ScutiSettings");
        ScutiSettings settings = null;
        if(results!=null && results.Length>0)
        {
            var res = results[0];
            var path = AssetDatabase.GUIDToAssetPath(res);
            settings = AssetDatabase.LoadAssetAtPath<ScutiSettings>(path);
        }
        else
        {
            string assetPath = "Assets/Resources/"+ ScutiConstants.SCUTI_SETTINGS_PATH_FILE;
            if (!Directory.Exists(assetPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            }
            ScriptableObject settingsInstance = CreateInstance(typeof(ScutiSettings));
            AssetDatabase.CreateAsset(settingsInstance, assetPath + ScutiConstants.SCUTI_SETTINGS_FILE);
            settings = AssetDatabase.LoadAssetAtPath<ScutiSettings>(assetPath + ScutiConstants.SCUTI_SETTINGS_FILE);
        }
        return settings;
    }

    void OnGUI()
    {
        if (settings == null)
        {
            GUILayout.Label("You don't have any settings files. You should create one to be able to use scuti", EditorStyles.wordWrappedLabel);
            settings = GetOrCreateSettings();
            AssetDatabase.OpenAsset(settings);
        }

        if (!isInitialized && settings!=null)
        {
            isInitialized = true;
            developerKey = settings.developerKey;
            secret = settings.secret;
            type = (int)settings.ExchangeMethod;
            orientation = (int)settings.Orientation;
        }
        GUILayout.BeginArea(new Rect(10, 10, 380, WINDOW_HEIGHT));
        //GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        //info
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Scuti SDK", EditorStyles.whiteLargeLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Version: "+ScutiConstants.VERSION, EditorStyles.whiteLargeLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        bool installed = false;
        if (settings != null)
        {
            installed = (settings.ExchangeMethod != ScutiSettings.CurrencyExchangeMethod.GameServer && !string.IsNullOrEmpty(settings.developerKey))
               || (settings.ExchangeMethod == ScutiSettings.CurrencyExchangeMethod.GameServer && !string.IsNullOrEmpty(settings.developerKey) && !string.IsNullOrEmpty(settings.secret));
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(installed ? $"SDK initialized" : $"SDK not initialized", EditorStyles.whiteLargeLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        
        GUILayout.Space(10);
        if (settings != null)
        {

            GUILayout.Label("Currency Exchange:");
            var priorType = type;
            type = GUILayout.Toolbar(type, new string[] { "None", "Game Client", "Game Server" });


            GUILayout.Space(10);
            if (type != (int)ScutiSettings.CurrencyExchangeMethod.GameServer)
            {
                GUILayout.Label(ID_LABEL);
                developerKey = GUILayout.TextField(developerKey);
            }
            else
            {
                GUILayout.Label(ID_LABEL);
                developerKey = GUILayout.TextField(developerKey);

                GUILayout.Label("Secret");
                secret = GUILayout.TextField(secret);
            }

            GUILayout.Space(10);
            GUILayout.Label("Application Orientation:");
            var priorOrientation = orientation;
            orientation = GUILayout.Toolbar(orientation, new string[] { "None", "Autorotation", "Portrait", "Landscape" });
            var updated = priorType != type || priorOrientation != orientation;

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (updated || GUILayout.Button("Save"))
            {
                settings.developerKey = developerKey;
                settings.ExchangeMethod = (ScutiSettings.CurrencyExchangeMethod)type;
                settings.secret = secret;
                settings.Orientation = (ScutiSettings.AppOrientation)orientation;
                if (settings.ExchangeMethod != ScutiSettings.CurrencyExchangeMethod.GameServer)
                    settings.secret = "";
                else
                    settings.secret = secret;
                EditorUtility.SetDirty(settings);
                AssetDatabase.OpenAsset(settings);
            }
            if (GUILayout.Button("Open Settings"))
            {
                AssetDatabase.OpenAsset(settings);
            }
            GUILayout.EndHorizontal();

            
        }
        //end info
        GUILayout.Space(10);

        GUILayout.Label("Add the Scuti Store button to your active scene: ");
        if (GUILayout.Button("Create Scuti Button", GUILayout.Height(30)))
        {
            CreateGameObject();
        }
        GUILayout.Space(20);
        //button bar
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Documentation", GUILayout.Height(BUTTON_HEIGHT), GUILayout.Width(BUTTON_WIDTH)))
        {
            Application.OpenURL(ScutiConstants.DOCUMENTATION_URL);
        }
        GUILayout.Space(40);
        if (GUILayout.Button("Dashboard", GUILayout.Height(BUTTON_HEIGHT), GUILayout.Width(BUTTON_WIDTH)))
        {
            Application.OpenURL("https://web-test.scuti.mindtrust.co/");
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //end button bar

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();

        GUI.changed = false;
        showAgain = GUILayout.Toggle(showAgain, "Show Window if Developer Key is Missing");
        if (GUI.changed)
        {
            EditorPrefs.SetInt(GetProjectBasedPrefsKey(PREVENT), showAgain ? 1 : 0);
            
        }

        GUILayout.Space(15);
        GUILayout.EndArea();
    }
}