using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class HierarchyContext
{



    [MenuItem("GameObject/Favorite", false, -99)]
    public static void AddToFavorites()
    {
        var Fav = ObjectInQuestion(Selection.activeGameObject);

        FavoritesWindow.Instance.AddToFavorite(Fav);
    }
    static FavoriteObject ObjectInQuestion(Object selected)
    {
        var instID = selected.GetInstanceID();
        var Fav = new FavoriteObject();
        Fav.ID = instID;

        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(selected);

        Fav.AssetPath = AssetDatabase.GetAssetPath(prefab);
        var isPrefab = prefab != null;
        if (isPrefab)
        {
            Fav.Type = FavoriteObject.AssetType.Prefab;
            Fav.ObjectName = Path.GetFileName(Fav.AssetPath);
        }
        else
        {
            Fav.Type = FavoriteObject.AssetType.SceneObject;

        Fav.ObjectName = selected.name;
        }
        //Fav.Type = FavoriteObject.AssetType.ProjectAsset;


        Fav.Project = FavoritesWindow.GetProjectName();
        return Fav;
    }
    [MenuItem("GameObject/Favorite", true, -99)]
    public static bool IsItInFavorites()
    {
        var Fav = ObjectInQuestion(Selection.activeGameObject);
        return !FavoritesWindow.Instance.InFavorites(Fav);

    }
}
