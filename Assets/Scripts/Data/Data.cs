using System;
using System.Collections.Generic;

[Serializable]
public class Data
{
    public string PlayerId;
    public string PlayerName;
    
    public int IconId;
    public List<IconUnlocked> IconsUnlocked;

    public int GamesPlayed;
    public int BoardsCompleted;

    public Data() { }
    public Data(string playerName)
    {
        PlayerId = Guid.NewGuid().ToString("N");
        PlayerName = playerName;
        IconId = 0;
        GamesPlayed = 9;
        IconsUnlocked = new List<IconUnlocked>();
        IconsUnlocked.Add(new IconUnlocked(0, true));
    }
}

[Serializable]
public class IconUnlocked
{
    public int IconId;
    public bool Unlocked;

    public IconUnlocked(int iconId, bool unlocked)
    {
        IconId = iconId;
        Unlocked = unlocked;
    }
}

[Serializable]
public class Records
{
    public int ModeId;
    public int BestRecord;
}