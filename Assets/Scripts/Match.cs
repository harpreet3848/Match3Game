using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Match : MonoBehaviour
{
    public Grid<Tile> Grid;
    [SerializeField] Vector3Int _origin;
    [SerializeField] TileSpawner _tileSpawner;
    [SerializeField] MatchHandler _matchFinder;

    [SerializeField] float tileSpawnDuration = 0.7f;
    
    private List<GameObject> _bgList = new();

    void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        ClearGrid();

        Grid = new Grid<Tile>(6, 6, _origin);

        GenerateBG();

        StartCoroutine(GenerateTiles());
    }

    private void GenerateBG()
    {
        for (int r = 0; r < Grid.MaxRow; r++)
        {
            for (int c = 0; c < Grid.MaxColumn; c++)
            {
                Vector3 pos = Grid.GetWorldPosition(c, r);
                _bgList.Add(Instantiate(_tileSpawner._tilesSO.Bg, pos, Quaternion.identity));
            }
        }
    }

    private IEnumerator GenerateTiles()
    {
        GameManager.instance.gameState = GameState.Animating;

        for (int r = 0; r < Grid.MaxRow; r++)
        {
            for (int c = 0; c < Grid.MaxColumn; c++)
            {
                if (Grid.GetGridObjectAtIndex(r, c) != null) continue;

                Tile tile = _tileSpawner.SpawnRandomTileNoMatch(r,c,tileSpawnDuration);

                Grid.SetGridObjectAtIndex(r, c, tile);
            }
        }
        
        yield return new WaitForSeconds(tileSpawnDuration);

        _matchFinder.FindMatches();

        if (_matchFinder.IsMatchFound())
        {
            OnMatchesFound();
        }
        else
        {
            GameManager.instance.gameState = GameState.Idle;
        }
    }
    public void SwapTiles(Vector2Int pos1, Vector2Int pos2)
    {
        GameManager.instance.gameState = GameState.Animating;

        Tile tileA = Grid.GetGridObjectAtIndex(pos1.y, pos1.x);
        Tile tileB = Grid.GetGridObjectAtIndex(pos2.y, pos2.x);

        Vector3 startA = tileA.transform.position;
        Vector3 startB = tileB.transform.position;

        DOTween.Kill("Swaping");
        var seq = DOTween.Sequence().SetId("Swaping");
        seq.Append(tileA.transform.DOMove(startB, 0.2f));
        seq.Join(tileB.transform.DOMove(startA, 0.2f));
        seq.OnComplete(() =>
        {
            Grid.SetGridObjectAtIndex(pos1.y, pos1.x, tileB);
            Grid.SetGridObjectAtIndex(pos2.y, pos2.x, tileA);

            _matchFinder.FindMatches();
            if (_matchFinder.IsMatchFound())
            {
                OnMatchesFound();
            }
            else
            {
                GameManager.instance.gameState = GameState.Animating;
                var reverseSeq = DOTween.Sequence().SetId("Swaping");
                reverseSeq.Append(tileA.transform.DOMove(startA, 0.2f));
                reverseSeq.Join(tileB.transform.DOMove(startB, 0.2f));

                reverseSeq.OnComplete(() =>
                {
                    Grid.SetGridObjectAtIndex(pos1.y, pos1.x, tileA);
                    Grid.SetGridObjectAtIndex(pos2.y, pos2.x, tileB);

                    GameManager.instance.gameState = GameState.Idle;
                });
            }
        });
    }
    private void OnMatchesFound()
    {
        _matchFinder.RemoveMatches();
        CollapseTiles();
    }
    private void CollapseTiles()
    {
        for (int r = 1; r < Grid.MaxRow; r++)
        {
            for (int c = 0; c < Grid.MaxColumn; c++)
            {
                if (Grid.GetGridObjectAtIndex(r, c) == null)
                {
                    for (int k = r - 1; k >= 0; k--)
                    {
                        if (Grid.GetGridObjectAtIndex(k, c) != null)
                        {
                            Transform tileTransform = Grid.GetGridObjectAtIndex(k, c).transform;
                            Vector3 targetPos = Grid.GetWorldPosition(c, k + 1);

                            tileTransform.DOMove(targetPos, 0.7f).SetEase(Ease.InQuad);

                            Grid.SetGridObjectAtIndex(k + 1, c ,Grid.GetGridObjectAtIndex(k, c));
                            Grid.SetGridObjectAtIndex(k, c, null);
                        }
                    }
                }
            }
        }

        OnAllTilesCollapsed();
    }

    private void OnAllTilesCollapsed()
    {
        StartCoroutine(GenerateTiles());
    }

  
    private void ClearGrid()
    {
        DOTween.KillAll();
        GameManager.instance.gameState = GameState.Idle;

        _matchFinder.ClearMatches();

        if (Grid != null)
        {
            for (int r = 0; r < Grid.MaxRow; r++)
            {
                for (int c = 0; c < Grid.MaxColumn; c++)
                {
                    if (Grid.GetGridObjectAtIndex(r, c) != null)
                    {
                        Destroy(Grid.GetGridObjectAtIndex(r, c).gameObject);
                        Grid.SetGridObjectAtIndex(r, c, null);
                    }
                }
            }


            for (int i = 0; i < _bgList.Count; i++)
            {
                Destroy(_bgList[i].gameObject);
            }
            _bgList.Clear();
        }
    }
  
    private void Update()
    {
        Grid.DrawGridBorders(); 
    }
}

