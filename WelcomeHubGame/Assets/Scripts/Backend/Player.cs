using System;
[Serializable]
public class Player
{
    public string guid { get; }
    public string username { get; }
    public Player(string guid, string username)
    {
        this.guid = guid;
        this.username = username;
    }
}