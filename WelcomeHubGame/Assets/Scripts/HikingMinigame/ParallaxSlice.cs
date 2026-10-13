using System;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace HikingMinigame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxSlice : MonoBehaviour
    {
        [Serializable]
        private class WeightedSprite
        {
            public int weight = 1;
            public Sprite sprite;
        }

        [SerializeField] private WeightedSprite[] _possibleSprites;
        [SerializeField] private int _spriteCount = 8;
        [SerializeField] private int _orderInLayer = 2;
        [SerializeField] private float _minScale = 0.8f;
        [SerializeField] private float _maxScale = 1.2f;
        [SerializeField] private float _edgeMargin = 2f;

        [SerializeField] private float _farScale = 0.5f;
        [SerializeField] private float _zPerUnit = 0.01f;

        [SerializeField] private float _topY = 7.5f;
        [SerializeField] private float _bottomY = -3f;
        [SerializeField] private float _nearSpeed = 3f;
        [SerializeField] private float _farSpeed = 0.3f;

        private SpriteRenderer _area;
        private SpriteRenderer[] _sprites;
        private float _depthFactor;
        private float _scrollSpeed;
        private float _camLeftEdge;
        private int _totalWeight;
        
        private bool _isClone;
        private float _depthScale;
        
        private float _wrapDistance;
        
        private void Start()
        {
            _area = GetComponent<SpriteRenderer>();
            var bounds = _area.bounds;

            var depthRate = (_nearSpeed / _farSpeed - 1f) / (_topY - _bottomY);
            _depthFactor = 1f / (1f + depthRate * (bounds.min.y - _bottomY));
            _scrollSpeed = _nearSpeed * _depthFactor;

            var cam = Camera.main;
            var camHalfWidth = cam.orthographicSize * cam.aspect;
            _camLeftEdge = cam.transform.position.x - camHalfWidth;

            var t = Mathf.InverseLerp(_bottomY, _topY, bounds.min.y);
            _depthScale = Mathf.Lerp(1f, _farScale, t);

            if (!_isClone)
            {
                var width = bounds.size.x;
                var copies = Mathf.CeilToInt((camHalfWidth * 2f + _edgeMargin * 2f) / width);
                _wrapDistance = (copies + 1) * width;

                for (var i = 1; i <= copies; i++)
                {
                    var offset = new Vector3(i * width, 0f, 0f);
                    var clone = Instantiate(this, transform.position + offset, transform.rotation, transform.parent);
                    clone._isClone = true;
                    clone._wrapDistance = _wrapDistance;
                }
            }

            foreach (var s in _possibleSprites) _totalWeight += s.weight;

            CreateSprites();
            Populate();
        }

        private void Update()
        {
            transform.position += new Vector3(-_scrollSpeed * Time.deltaTime, 0f, 0f);

            if (_area.bounds.max.x + _edgeMargin >= _camLeftEdge) return;

            transform.position += new Vector3(_wrapDistance, 0f, 0f);
            Populate();
        }

        private void CreateSprites()
        {
            _sprites = new SpriteRenderer[_spriteCount];
            for (var i = 0; i < _spriteCount; i++)
            {
                var child = new GameObject("SliceSprite");
                child.transform.SetParent(transform, false);
                var sr = child.AddComponent<SpriteRenderer>();
                sr.sortingOrder = _orderInLayer;
                _sprites[i] = sr;
            }
        }

        private void Populate()
        {
            var bounds = _area.bounds;
            var parentScale = transform.lossyScale;

            foreach (var sr in _sprites)
            {
                sr.sprite = SelectSprite();
                sr.flipX = URandom.value > 0.5f;
                sr.transform.position = new Vector3(
                    URandom.Range(bounds.min.x, bounds.max.x),
                    URandom.Range(bounds.min.y, bounds.max.y),
                    transform.position.z);

                var scale = URandom.Range(_minScale, _maxScale) * _depthScale;
                sr.transform.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, 1f);

                var pos = sr.transform.position;
                pos.z = sr.bounds.min.y * _zPerUnit;
                sr.transform.position = pos;
            }
        }

        private Sprite SelectSprite()
        {
            var roll = URandom.Range(0, _totalWeight);
            foreach (var s in _possibleSprites)
            {
                roll -= s.weight;
                if (roll < 0) return s.sprite;
            }

            return _possibleSprites[^1].sprite;
        }
    }
}