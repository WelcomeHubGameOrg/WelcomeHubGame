using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ScenePathSO", menuName = "Game/ScenePathSO")]
public sealed class ScenePathSO : ScriptableObject
{
    [SerializeField, HideInInspector] private string _path;
    public string Path => _path;
    
    #if UNITY_EDITOR
    [SerializeField] private SceneAsset _sceneAsset;

    private void OnValidate()
    {
        _path = _sceneAsset != null ? AssetDatabase.GetAssetPath(_sceneAsset) : string.Empty; 
    }
    #endif
}
