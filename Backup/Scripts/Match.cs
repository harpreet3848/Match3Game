using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Match : MonoBehaviour
{
    [SerializeField] Vector3Int origin;
    [SerializeField] SO_Tiles s_tiles;
    Grid<Tile> grid;

    TilesType[] _allPlayableTypes = new TilesType[]
    {
        TilesType.Red,
        TilesType.Blue,
        TilesType.Green,
        TilesType.Yellow,
        TilesType.Orange,
    };


    List<GameObject> bgList = new();

    List<Tile> matches = new();
    void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        ClearGrid();

        grid = new Grid<Tile>(6, 6, origin);

        for (int r = 0; r < grid.MaxRow; r++)
        {
            for (int c = 0; c < grid.MaxColumn; c++)
            {
                Vector3 pos = grid.GetWorldPosition(c, r);
                bgList.Add(Instantiate(s_tiles.Bg, pos, Quaternion.identity));
            }
        }

        GenerateRandomTiles();
    }

    private void GenerateRandomTiles()
    {
        Sequence collapseSequence = DOTween.Sequence();
        collapseSequence.OnStart(() =>
        {
            isAnimating = true;
        });

        for (int r = 0; r < grid.MaxRow; r++)
        {
            for (int c = 0; c < grid.MaxColumn; c++)
            {
                if (grid.GetGridObjectAtIndex(r, c) != null) continue;

                Vector3 pos = grid.GetWorldPosition(c, r);
                pos.y += 5;
                // Generate Random Tiles without matching of 3
                List<TilesType> validChoices = new List<TilesType>(_allPlayableTypes);

                if (c >= 2)
                {
                    TilesType leftType1 = grid.GetGridObjectAtIndex(r, c - 1).tileType;
                    TilesType leftType2 = grid.GetGridObjectAtIndex(r, c - 2).tileType;

                    if (leftType1 == leftType2 && validChoices.Contains(leftType1))
                    {
                        validChoices.Remove(leftType1);
                    }
                }
                if (r >= 2)
                {
                    TilesType leftType1 = grid.GetGridObjectAtIndex(r - 1, c).tileType;
                    TilesType leftType2 = grid.GetGridObjectAtIndex(r - 2, c).tileType;

                    if (leftType1 == leftType2 && validChoices.Contains(leftType1))
                    {
                        validChoices.Remove(leftType1);
                    }
                }

                int rand = UnityEngine.Random.Range(0, validChoices.Count);


                Tile tile = Instantiate(GetTile(validChoices[rand]), pos, Quaternion.identity);
                pos.y -= 5;
                collapseSequence.Join(tile.transform.DOMove(pos, 0.7f).SetEase(Ease.InQuad));

                grid.SetGridObjectAtIndex(r, c, tile);
            }

        }

        collapseSequence.OnComplete(() =>
        {
            ClearMatchingTiles(true);
            ClearMatchingTiles(false);
            if (IsMatchFound())
            {
                RemoveMatches();
            }
            else
            {
                isAnimating = false;
            }
        });
    }

    private Tile GetTile(TilesType tilesType)
    {
        switch (tilesType)
        {
            case TilesType.Blue:
                 return s_tiles.Tiles[0];
            case TilesType.Red:
                return s_tiles.Tiles[1];
            case TilesType.Green:
                return s_tiles.Tiles[2];
            case TilesType.Yellow:
                return s_tiles.Tiles[3];
            case TilesType.Orange:
                return s_tiles.Tiles[4];
            default:
                return null;
        }
    }
    private Vector2 enterMousePosition;
    Vector2Int SelectedGridIndex;
    bool isAnimating = false;
    bool isSwaped = false;
    private void InitilizeSwapTilePositions()
    {
        if (isAnimating || isSwaped) return;
        enterMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        SelectedGridIndex = new Vector2Int((int)grid.WorldToGridIndex(enterMousePosition.x, enterMousePosition.y).x, 
                                           (int)grid.WorldToGridIndex(enterMousePosition.x, enterMousePosition.y).y);
    }
    private void CheckSwapPosition()
    {
        if (isAnimating || isSwaped) return;
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(enterMousePosition, currentMousePosition) > 0.7f)
        {
            if (SelectedGridIndex.y >= 0 && SelectedGridIndex.y < grid.MaxRow&&
                SelectedGridIndex.x >= 0 && SelectedGridIndex.x < grid.MaxColumn)
            {
                Tile tile = grid.GetGridObjectAtIndex(SelectedGridIndex.y, SelectedGridIndex.x);

                if (tile != null)
                {
                    Vector3 relative = tile.transform.InverseTransformPoint(currentMousePosition);
                    float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;

                    // Now check four quadrants (each covers a 90° slice):
                    if (angle >= -45f && angle < 45f)
                    {
                        SwapTiles(SelectedGridIndex, new Vector2Int(SelectedGridIndex.x, Mathf.Max(0, SelectedGridIndex.y - 1)));
                        //Debug.Log("Direction = Top");

                    }
                    else if (angle >= 45f && angle < 135f)
                    {
                        SwapTiles(SelectedGridIndex, new Vector2Int(Mathf.Min(grid.MaxColumn - 1, SelectedGridIndex.x + 1), SelectedGridIndex.y));
                        //Debug.Log("Direction = Right");
                    }
                    else if (angle >= -135f && angle < -45f)
                    {
                        SwapTiles(SelectedGridIndex, new Vector2Int(Mathf.Max(0, SelectedGridIndex.x - 1), SelectedGridIndex.y));
                        //Debug.Log("Direction = Left");
                    }
                    else
                    {
                        SwapTiles(SelectedGridIndex, new Vector2Int(SelectedGridIndex.x, Mathf.Min(grid.MaxRow - 1, SelectedGridIndex.y + 1)));
                        //Debug.Log("Direction = Bottom");
                    }
                }
            }
        }
    }
    private void SwapTiles(Vector2Int pos1, Vector2Int pos2)
    {
        isAnimating = true;
        isSwaped = true;
        DOTween.Kill("Swaping");
        Tile tileA = grid.GetGridObjectAtIndex(pos1.y, pos1.x);
        Tile tileB = grid.GetGridObjectAtIndex(pos2.y, pos2.x);

        Vector3 startA = tileA.transform.position;
        Vector3 startB = tileB.transform.position;

        var seq = DOTween.Sequence();
        seq.Append(tileA.transform.DOMove(startB, 0.2f));
        seq.Join(tileB.transform.DOMove(startA, 0.2f));
        seq.OnComplete(() =>
        {
            grid.SetGridObjectAtIndex(pos1.y, pos1.x, tileB);
            grid.SetGridObjectAtIndex(pos2.y, pos2.x, tileA);
            ClearMatchingTiles(true);
            ClearMatchingTiles(false);
            if (IsMatchFound())
            {
                RemoveMatches();
                //isAnimating = false;
            }
            else
            {
                isAnimating = true;
                var reverseSeq = DOTween.Sequence().SetId("Swaping");
                reverseSeq.Append(tileA.transform.DOMove(startA, 0.2f));
                reverseSeq.Join(tileB.transform.DOMove(startB, 0.2f));

                reverseSeq.OnComplete(() =>
                {
                    grid.SetGridObjectAtIndex(pos1.y, pos1.x, tileA);
                    grid.SetGridObjectAtIndex(pos2.y, pos2.x, tileB);

                    isAnimating = false;
                });
            }
        });
    }

    private void CollapseTiles()
    {
        Sequence collapseSequence = DOTween.Sequence();

        for (int r = 1; r < grid.MaxRow; r++)
        {
            for (int c = 0; c < grid.MaxColumn; c++)
            {
                if (grid.GetGridObjectAtIndex(r, c) == null)
                {
                    for (int k = r - 1; k >= 0; k--)
                    {
                        if (grid.GetGridObjectAtIndex(k, c) != null)
                        {
                            Transform tileTransform = grid.GetGridObjectAtIndex(k, c).transform;
                            Vector3 targetPos = grid.GetWorldPosition(c, k + 1);
                            //Debug.Log("Swaped" + grid.GetWorldPosition(c, k + 1));
                            Tween t = tileTransform
                                     .DOMove(targetPos, 0.7f)
                                    .SetEase(Ease.InQuad);
                            collapseSequence.Join(t);
                            grid.SetGridObjectAtIndex(k + 1, c ,grid.GetGridObjectAtIndex(k, c));
                            grid.SetGridObjectAtIndex(k, c, null);
                        }
                    }
                }
            }
        }

        OnAllTilesCollapsed();
    }

    private void OnAllTilesCollapsed()
    {
        GenerateRandomTiles();

    }
    private void RemoveMatches()
    {
        for (int i = 0;i < matches.Count; i++) 
        {
            if (matches[i] != null)
            {
                Vector2 index = grid.WorldToGridIndex(matches[i].transform.position.x, matches[i].transform.position.y);
                var tile = matches[i];
                tile.transform.DOScale(Vector3.zero, 0.2f).OnComplete(()=> 
                {
                    Destroy(tile.gameObject);
                });

                //Debug.Log((int)index.x + " "+ (int)index.y);
                grid.SetGridObjectAtIndex((int)index.y, (int)index.x, null);
            }
        }
        matches.Clear();
        CollapseTiles();
    }

    private void ClearGrid()
    {
        DOTween.KillAll();
        isAnimating = false;

        for (int i = 0; i < matches.Count; i++)
        {
            Destroy(matches[i].gameObject);
            //Destroy(matches[i]);
        }
        matches.Clear();

        if (grid != null)
        {
            for (int r = 0; r < grid.MaxRow; r++)
            {
                for (int c = 0; c < grid.MaxColumn; c++)
                {
                    if (grid.GetGridObjectAtIndex(r, c) != null)
                    {
                        Destroy(grid.GetGridObjectAtIndex(r, c).gameObject);
                        grid.SetGridObjectAtIndex(r, c, null);
                    }
                }
            }


            for (int i = 0; i < bgList.Count; i++)
            {
                Destroy(bgList[i].gameObject);
            }
            bgList.Clear();
        }
    }

    private void ClearMatchingTiles(bool isHorizontal)
    {
        int outerLimit = isHorizontal ? grid.MaxRow : grid.MaxColumn;
        int innerLimit = isHorizontal ? grid.MaxColumn : grid.MaxRow;

        for (int r = 0; r < outerLimit; r++)
        {
            int runCount = 0;
            TilesType currentTile = TilesType.Bg;

            for (int c = 0; c < innerLimit; c++)
            {
                Tile tile = isHorizontal
                    ? grid.GetGridObjectAtIndex(r, c)
                    : grid.GetGridObjectAtIndex(c, r);

                if (tile == null)
                {
                    CollectMatches(r, c - runCount, runCount);
                    runCount = 0;
                    currentTile = TilesType.Bg;
                    continue;
                }

                if (tile.tileType != currentTile)
                {
                    CollectMatches(r, c - runCount, runCount);
                    runCount = 1;
                    currentTile = tile.tileType;
                }
                else
                {
                    runCount++;
                }
            }

            CollectMatches(r, innerLimit - runCount, runCount);

        }

        void CollectMatches( int r, int startingIndex,int runCount)
        {
            if (runCount < 3) return;
            
            int end = startingIndex + runCount - 1;

            for (int i = startingIndex; i <= end; i++)
            {
                Tile duplicatetile;
                if (isHorizontal)
                {

                    duplicatetile = grid.GetGridObjectAtIndex(r, i);
                    //tiles[row, i] = null;
                }
                else
                {
                    duplicatetile = grid.GetGridObjectAtIndex(i, r);
                    //tiles[i, row] = null;
                }
                matches.Add(duplicatetile);
                //Destroy(duplicatetile.gameObject);

            }
        }
        
    }
    private bool IsMatchFound()
    {
        return matches.Count > 0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            InitilizeSwapTilePositions();
        }
        else if (Input.GetMouseButton(0))
        {
            CheckSwapPosition();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSwaped = false;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            InitializeGrid();
        }
       
        grid.DrawGridBorders(); 
    }
}
