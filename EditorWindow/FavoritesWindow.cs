using I2.Loc.SimpleJSON;
using Mono.CecilX.Cil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Valve.VR.InteractionSystem;

[Serializable]
public class FavoriteObject
{
    public string Project;
    public string ObjectName;
    public AssetType Type;
    public int ID;
    public string AssetPath;
    public enum AssetType
    {
        Prefab,
        SceneObject,
        ProjectAsset,
        Folder
    }
}

public class FavoritesWindow : EditorWindow
{
    static FavoritesWindow _instance;
    public static FavoritesWindow Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new FavoritesWindow();

            }

            return _instance;
        }
    }
    [MenuItem("Window/My Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FavoritesWindow), false, "My Favorites", true);
        Debug.Log("Welelele");
    }
    static string _pdp;
    static string PersDatPat
    {
        get
        {
            if (string.IsNullOrEmpty(_pdp))
            {
                _pdp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Unity_MyFavorites";
                Directory.CreateDirectory(_pdp);
                if (string.IsNullOrEmpty(_pdp))
                {
                    return "";
                }
                return _pdp;
            }
            return _pdp;
        }
        set
        {
            _pdp = value;
        }
    }
    FavoritesWindow()
    {
        Debug.Log("wo?");
        _instance = this;
        GetProjectPath();
        Debug.Log(PersDatPat);

        if (System.IO.File.Exists(PersDatPat + "/Dev-Favorites.json"))
        {
            myList = JsonConvert.DeserializeObject<FavoriteObject[]>(File.ReadAllText(PersDatPat + "/Dev-Favorites.json")).ToList();

        }
        else myList = new List<FavoriteObject>();
    }

    List<FavoriteObject> myList = new List<FavoriteObject>();
    private int selectedIndex = -1;
    Vector2 scroll;
    bool contextOpen = false;

    public void AddToFavorite(FavoriteObject @object)
    {
        Repaint();
        myList.Add(@object);
        var jsonstring = JsonConvert.SerializeObject(myList.ToArray(), Formatting.Indented);
        System.IO.File.WriteAllText(PersDatPat + "/Dev-Favorites.json", jsonstring);
    }
    public void RemoveFromFavorite(FavoriteObject @object)
    {
        var i = myList.FindIndex(x => x.ID == @object.ID);
        myList.RemoveAt(i);
        var jsonstring = JsonConvert.SerializeObject(myList.ToArray(), Formatting.Indented);
        System.IO.File.WriteAllText(PersDatPat + "/Dev-Favorites.json", jsonstring);
    }


    public bool InFavorites(FavoriteObject @object)
    {
        var res =  myList.Any(x => x.ID == @object.ID);
        Debug.Log(res ? "Var" : "Yok");
        return res;
    }
    public static string GetProjectPath()
    {
        return PersDatPat;
    }
    public static string GetProjectName()
    {
        var s = Application.persistentDataPath.Split('/');
        return s[s.Length - 1];

    }
    void RemoveFromFavorites(int i)
    {
        myList.RemoveAt(i);
        var jsonstring = JsonConvert.SerializeObject(myList.ToArray(), Formatting.Indented);
        System.IO.File.WriteAllText(PersDatPat + "/Dev-Favorites.json", jsonstring);
    }
    GenericMenu CreateMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove"), false, () => { RemoveFromFavorites(selectedIndex); });
        return menu;
    }

    GUIStyle GetWrapperItemStyle()
    {
        var itemStyle = new GUIStyle(GUI.skin.button);  //make a new GUIStyle
        itemStyle.alignment = TextAnchor.MiddleLeft; //align text to the left
        itemStyle.active.background = itemStyle.normal.background;  //gets rid of button click background style.
        itemStyle.margin = new RectOffset(0, 0, 0, 0);


        return itemStyle;
    }
    GUIStyle GetIconStyle()
    {
        var textureStyle = new GUIStyle(GUI.skin.label);
        textureStyle.fixedWidth = 26f;
        textureStyle.active.background = GetWrapperItemStyle().normal.background;
        textureStyle.fixedHeight = 26f;
        textureStyle.alignment = TextAnchor.MiddleCenter;
        return textureStyle;
    }
    GUIStyle GetLabelStyle()
    {
        var labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fixedHeight = 26f;
        labelStyle.alignment = TextAnchor.MiddleLeft;
        return labelStyle;
    }


    double clickTime;
    double doubleClickTime = 0.05;
    void OpenFile(FavoriteObject item)
    {
        var x = item.ObjectName.Split('.');
        var ext = x[x.Length - 1];
        switch (ext)
        {
            case "prefab":
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(item.AssetPath, typeof(GameObject)));
                break;
            case "cs":
                AssetDatabase.OpenAsset(item.ID, 0, 0);
                break;
            case "unity":
                EditorSceneManager.OpenScene(item.AssetPath);
                break;

            default:
                break;
        }
    }
    private void OnGUI()
    {
        var color_default = GUI.backgroundColor;
        var color_selected = Color.gray;

        var e = Event.current;
        if (contextOpen)
        {
            var menu = CreateMenu();
            menu.ShowAsContext();
            contextOpen = false;
        }
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < myList.Count; i++)
        {
            FavoriteObject item = myList[i];
            if (item.Project != GetProjectName()) continue;

            GUI.backgroundColor = (selectedIndex == i) ? color_selected : Color.clear;
            var r = EditorGUILayout.BeginHorizontal(GetWrapperItemStyle());
            GUI.Label(r, GUIContent.none);

            if (GUI.Button(r, GUIContent.none, GetWrapperItemStyle()))
            {

                selectedIndex = i;
                if (e.clickCount == 1)
                    EditorGUIUtility.PingObject(item.ID);
                if ((EditorApplication.timeSinceStartup - clickTime) < doubleClickTime)
                {
                    OpenFile(item);
                }
                if (r.Contains(e.mousePosition) && e.type == EventType.Used && e.button == 1)
                {
                    contextOpen = true;
                    e.Use();
                }
                clickTime = EditorApplication.timeSinceStartup;
            }
            //Favori tipine göre icon bul
            Texture2D texture = UnityEditorInternal.InternalEditorUtility.GetIconForFile(item.ObjectName);
            var textureContent = new GUIContent(texture);
            GUILayout.Box(textureContent, GetIconStyle());
            GUILayout.Label(myList[i].ObjectName, GetLabelStyle());
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list
        }

        EditorGUILayout.EndScrollView();

    }

}
