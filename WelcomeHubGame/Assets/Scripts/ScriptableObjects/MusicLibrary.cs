using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Audio/Music Library")]
public class MusicLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public MusicID id;
        public AudioClip clip;
    }

    public List<Entry> tracks;
    private Dictionary<MusicID, AudioClip> _lookup;

    public AudioClip Get(MusicID id)
    {
        if (id == MusicID.None) return null;
        if (_lookup == null) BuildLookup();
        if (_lookup.TryGetValue(id, out var clip)) return clip;

        Debug.LogWarning($"MusicLibrary: no clip assigned for {id} (check the MusicLibrary asset in Inspector");
        return null;
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<MusicID, AudioClip>();
        foreach (var entry in tracks)
        {
            if (entry.id == MusicID.None) continue;
            _lookup[entry.id] = entry.clip;
        }
    }
}
