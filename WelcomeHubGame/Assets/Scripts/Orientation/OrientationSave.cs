using System;
using UnityEngine;

public enum PlayMode
{
    Unselected,
    Sandbox,
    Story
}
public enum StoryGender { Unselected, Male, Female, NonBinary }
public enum StudentType { Unselected, Exchange, Finnish }

[Serializable]
public sealed class OrientationSave
{
    private const string CookieKey = "welcomehub_save_v1";

    public PlayMode mode;
    public bool hasMapPosition;
    public Vector2 mapPosition;

    public string storyName;
    public StoryGender storyGender;
    public StudentType studentType;

    public static OrientationSave Load()
    {
        string json = BrowserCookies.Get(CookieKey);

        if (string.IsNullOrEmpty(json))
            return new OrientationSave();

        try
        {
            OrientationSave save = JsonUtility.FromJson<OrientationSave>(json);

            if (save != null && Enum.IsDefined(typeof(PlayMode), save.mode))
                return save;
        }
        catch (ArgumentException)
        {
            Debug.LogWarning("Could not read the Welcome Hub save. Starting fresh.");
        }

        return new OrientationSave();
    }

    public void Write()
    {
        BrowserCookies.Set(CookieKey, JsonUtility.ToJson(this), expirationDays: 30);
    }
}