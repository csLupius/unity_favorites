
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ProjectContext
{



    [MenuItem("Assets/Favorite", false, -99)]
    public static void AddToFavorites()
    {
        var Fav = ObjectInQuestion(Selection.activeObject);

        FavoritesWindow.Instance.AddToFavorite(Fav);
    }
    static FavoriteObject ObjectInQuestion(Object selected)
    {
        var instID = selected.GetInstanceID();
        var Fav = new FavoriteObject();
        Fav.ID = instID;

        Fav.Type = FavoriteObject.AssetType.ProjectAsset;


        Fav.AssetPath = AssetDatabase.GetAssetPath(selected);

        Fav.Project = FavoritesWindow.GetProjectName();
        Fav.ObjectName = Path.GetFileName(Fav.AssetPath);
        return Fav;
    }
    [MenuItem("Assets/Favorite", true, -99)]
    public static bool IsItInFavorites()
    {
        var Fav = ObjectInQuestion(Selection.activeObject);
        //return true;
        return !FavoritesWindow.Instance.InFavorites(Fav);

    }
}
