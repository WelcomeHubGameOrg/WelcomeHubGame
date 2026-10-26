using System;
[Serializable]
public class Player
{
    private string guid;
    private string username;
    public Player(string guid, string username)
    {
        this.guid = guid;
        this.username = username;
    }
    public string getGUID()
    {
        return guid;
    }
    
    public string getUsername()
    {
        return username;
    }
}