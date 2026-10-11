using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public SoundID id;
        public AudioClip clip;
    }

    public List<Entry> sounds;
    private Dictionary<SoundID, AudioClip> _lookup;

    public AudioClip Get(SoundID id)
    {
        if (_lookup == null) BuildLookup();
        if (_lookup.TryGetValue(id, out var clip)) return clip;

        Debug.LogWarning($@"SoundLibrary: no clip assigned for {id} (check the SoundLibrary asset in Inspector");
        return null;
    }

    private void BuildLookup()
    {
        _lookup = new Dictionary<SoundID, AudioClip>();
        foreach (var entry in sounds)
        {
            if (entry.id == SoundID.None) continue;
            _lookup[entry.id] = entry.clip;
        }
    }
}
