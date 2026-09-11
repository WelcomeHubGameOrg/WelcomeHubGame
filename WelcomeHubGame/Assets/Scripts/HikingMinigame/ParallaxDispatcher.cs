using System;
using System.Collections.Generic;
using System.Linq;
using HikingMinigame;
using UnityEngine;
using UnityEngine.Pool;
using Random = System.Random;
using URandom = UnityEngine.Random;

public class ParallaxDispatcher : MonoBehaviour
{
    [Serializable]
    private class WeightedSprite
    {
        public int weight;
        public Sprite sprite;
    }
    
    [SerializeField] private WeightedSprite[] _possibleSprites;
    [SerializeField] private ParallaxObject prefab;
    [SerializeField] private int prewarmCount = 10;
    [SerializeField] private int maxCount = 100;
    
    [SerializeField] private float _minInterval = 1f;
    [SerializeField] private float _maxInterval = 3f;
    
    [SerializeField] private float _topY = 7.5f;
    [SerializeField] private float _bottomY = -3f;
    
    [SerializeField] private float _nearSpeed = 3f;   // speed at the bottom border
    [SerializeField] private float _farSpeed = 0.3f;  // speed at the top border

    [SerializeField] private float _minScale = 0.8f;
    [SerializeField] private float _maxScale = 1.2f;
    
    private float _depthRate;
   
    private ObjectPool<ParallaxObject>  _spritePool;
    private readonly List<ParallaxObject> _active = new();
    
    private Camera _cam;
    private float _camLeftEdge;
    private float _camRightEdge;
    
    private float _spawnTimer;
    
    private void Start()
    {
        Debug.Log("ParallaxDispatcher Start");
        
        CacheCameraEdges();
        
        var temp = new ParallaxObject[prewarmCount];
        for (var i = 0; i < prewarmCount; i++) temp[i] = _spritePool.Get();
        foreach (var sr in temp) _spritePool.Release(sr);
        
        _spawnTimer = URandom.Range(_minInterval, _maxInterval);
    }

    private void Awake()
    {
        Debug.Log("ParallaxDispatcher Awake");
        
        _depthRate = (_nearSpeed / _farSpeed - 1f) / (_topY - _bottomY);
        
        _spritePool = new ObjectPool<ParallaxObject>(
            createFunc:      () => Instantiate(prefab, transform),
            actionOnGet:     sr => sr.gameObject.SetActive(true),
            actionOnRelease: sr => sr.gameObject.SetActive(false),
            actionOnDestroy: sr => Destroy(sr.gameObject),
            collectionCheck: true,
            defaultCapacity: prewarmCount,
            maxSize:         maxCount);
    }
    
    // Update is called once per frame
    private void Update()
    {
        ReleaseOldSprites();
        
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f) return;

        SpawnNewSprite();
        _spawnTimer += URandom.Range(_minInterval, _maxInterval);
    }

    private void SpawnNewSprite()
    {
        Debug.Log("ParallaxDispatcher SpawnNewSprite");
        
        var newObject = _spritePool.Get();
        var newSprite = newObject.SpriteRenderer;

        if (URandom.Range(0, 100) > 50)
            newSprite.flipX = true;

        var y = URandom.Range(_bottomY, _topY);
        var randomScale = URandom.Range(_minScale, _maxScale);
        
        newObject.transform.position = new Vector3(_camRightEdge + newSprite.size.x * 2, y, 0); 
        
        newSprite.sprite = SelectSprite();
        newObject.scrollSpeed = GetScrollSpeed(y);
        newObject.transform.localScale = Vector3.one * (randomScale * GetDepthFactor(y));
        
        _active.Add(newObject);
    }

    private Sprite SelectSprite()
    {
        var sum = _possibleSprites.Sum(s => s.weight);
        var rng = URandom.Range(0, sum);

        foreach (var s in _possibleSprites)
        {
            sum -= s.weight;
            if(sum > rng) continue;
            return s.sprite;
        }

        return _possibleSprites.Last().sprite;
    }
    
    private void ReleaseOldSprites()
    {
        for (var i = _active.Count - 1; i >= 0; i--)
        {
            var obj = _active[i];
            var halfWidth = obj.SpriteRenderer.bounds.extents.x;

            if (obj.transform.position.x + halfWidth > _camLeftEdge) continue;
            
            _spritePool.Release(_active[i]);
            _active.RemoveAt(i);
        }
    }
    
    private float GetDepthFactor(float y) => 1f / (1f + _depthRate * (y - _bottomY));

    private float GetScrollSpeed(float y) => _nearSpeed * GetDepthFactor(y);
    
    private void CacheCameraEdges()
    {
        _cam = Camera.main;
        _camLeftEdge = _cam!.transform.position.x - _cam.orthographicSize * _cam.aspect;
        _camRightEdge = _cam!.transform.position.x + _cam.orthographicSize * _cam.aspect;
    }
}
