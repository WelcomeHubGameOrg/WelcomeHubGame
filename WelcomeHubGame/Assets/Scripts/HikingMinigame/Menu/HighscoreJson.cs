using System;

namespace HikingMinigame.Menu
{
    [Serializable]
    internal sealed class ScoreListJson
    {
        public ScoreJson[] data = Array.Empty<ScoreJson>();
    }

    [Serializable]
    internal sealed class ScoreJson
    {
        public string user_id;
        public int score;
    }

    [Serializable]
    internal sealed class UserListJson
    {
        public UserJson[] data = Array.Empty<UserJson>();
    }

    [Serializable]
    internal sealed class UserJson
    {
        public string guid;
        public string username;
    }
}
