using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchHandler : MonoBehaviour
{
    public event Action<int> OnTileDestroyed;

    [SerializeField] Match _match;
    private List<Tile> _matches = new();

    public void FindMatches() 
    {
        FindMatchingTiles(true);
        FindMatchingTiles(false);
    }

    private void FindMatchingTiles(bool isHorizontal)
    {
        int outerLimit = isHorizontal ? _match.Grid.MaxRow : _match.Grid.MaxColumn;
        int innerLimit = isHorizontal ? _match.Grid.MaxColumn : _match.Grid.MaxRow;

        for (int r = 0; r < outerLimit; r++)
        {
            int runCount = 0;
            TilesType currentTile = TilesType.None;

            for (int c = 0; c < innerLimit; c++)
            {
                Tile tile = isHorizontal
                    ? _match.Grid.GetGridObjectAtIndex(r, c)
                    : _match.Grid.GetGridObjectAtIndex(c, r);

                if (tile == null)
                {
                    CollectMatches(r, c - runCount, runCount);
                    runCount = 0;
                    currentTile = TilesType.None;
                    continue;
                }

                if (tile.TileType != currentTile)
                {
                    CollectMatches(r, c - runCount, runCount);
                    runCount = 1;
                    currentTile = tile.TileType;
                }
                else
                {
                    runCount++;
                }
            }

            CollectMatches(r, innerLimit - runCount, runCount);

        }

        void CollectMatches(int r, int startingIndex, int runCount)
        {
            if (runCount < 3) return;

            int end = startingIndex + runCount - 1;

            for (int i = startingIndex; i <= end; i++)
            {
                Tile duplicatetile;
                if (isHorizontal)
                {

                    duplicatetile = _match.Grid.GetGridObjectAtIndex(r, i);
                    //tiles[row, i] = null;
                }
                else
                {
                    duplicatetile = _match.Grid.GetGridObjectAtIndex(i, r);
                    //tiles[i, row] = null;
                }
                _matches.Add(duplicatetile);
                //Destroy(duplicatetile.gameObject);

            }
        }
    }
    public void RemoveMatches()
    {
        int score = 0;
        for (int i = 0; i < _matches.Count; i++)
        {
            if (_matches[i] != null)
            {
                Vector2 index = _match.Grid.WorldToGridIndex(_matches[i].transform.position.x, _matches[i].transform.position.y);
                var tile = _matches[i];
                tile.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
                {
                    Destroy(tile.gameObject);
                });
                score += 10;
                //Debug.Log((int)index.x + " "+ (int)index.y);
                _match.Grid.SetGridObjectAtIndex((int)index.y, (int)index.x, null);
            }
        }
        OnTileDestroyed?.Invoke(score);
        _matches.Clear();
    }
    public void ClearMatches()
    {
        for (int i = 0; i < _matches.Count; i++)
        {
            Destroy(_matches[i].gameObject);
            //Destroy(matches[i]);
        }
        _matches.Clear();

    }
    public bool IsMatchFound()
    {
        return _matches.Count > 0;
    }
}
