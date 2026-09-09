using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float scrollSpeed;

    private Transform[] _tiles;
    private float _tileWidth;
    private int _leftmostTileIndex;
    
    private Camera _cam;
    private float _camLeftEdge;

    private void Start()
    {
        SetupTiles();
        CacheCameraLeftEdge();
    }

    private void Update()
    {
        var moveAmount = -scrollSpeed * Time.deltaTime;
        var tileCount = _tiles.Length;
        for (var i = 0; i < tileCount; i++)
            _tiles[i].position += new Vector3(moveAmount, 0f, 0f);

        var leftmostTile = _tiles[_leftmostTileIndex];

        if (leftmostTile.position.x + _tileWidth / 2f < _camLeftEdge)
        {
            var attachTileIndex = (_leftmostTileIndex == 0) ? tileCount - 1 : _leftmostTileIndex - 1;
            var attachTile = _tiles[attachTileIndex];

            leftmostTile.position = attachTile.position + new Vector3(_tileWidth, 0f, 0f);
            _leftmostTileIndex = (_leftmostTileIndex + 1) % tileCount;
        }
    }

    private void SetupTiles()
    {
        var tileCount = transform.childCount;
        _tiles = new Transform[tileCount];
        for (var i = 0; i < tileCount; i++)
            _tiles[i] = transform.GetChild(i);

        _tileWidth = _tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;

        for (var i = 0; i < tileCount; i++)
        {
            var pos = _tiles[i].localPosition;
            pos.x = i * _tileWidth;
            pos.y = 0f;
            pos.z = 0f;
            _tiles[i].localPosition = pos;
        }

        _leftmostTileIndex = 0;
    }

    private void CacheCameraLeftEdge()
    {
        _cam = Camera.main;
        _camLeftEdge = _cam.transform.position.x - _cam.orthographicSize * _cam.aspect;
    }
}