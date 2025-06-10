using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public SO_Tiles _tilesSO;

    [SerializeField] Match _match;

    private TilesType[] _allPlayableTypes = new TilesType[]
    {
        TilesType.Red,
        TilesType.Blue,
        TilesType.Green,
        TilesType.Yellow,
        TilesType.Orange,
    };

    public Tile SpawnRandomTileNoMatch(int r, int c, float animDuration = 0.7f)
    {
        Vector3 pos = _match.Grid.GetWorldPosition(c, r);
        pos.y += 5; // spawn Positon
        
        // Generate Random Tiles without matching of 3
        List<TilesType> validChoices = new(_allPlayableTypes);

        if (c >= 2)
        {
            TilesType leftType1 = _match.Grid.GetGridObjectAtIndex(r, c - 1).TileType;
            TilesType leftType2 = _match.Grid.GetGridObjectAtIndex(r, c - 2).TileType;

            if (leftType1 == leftType2 && validChoices.Contains(leftType1))
            {
                validChoices.Remove(leftType1);
            }
        }
        if (r >= 2)
        {
            TilesType leftType1 = _match.Grid.GetGridObjectAtIndex(r - 1, c).TileType;
            TilesType leftType2 = _match.Grid.GetGridObjectAtIndex(r - 2, c).TileType;

            if (leftType1 == leftType2 && validChoices.Contains(leftType1))
            {
                validChoices.Remove(leftType1);
            }
        }

        int rand = Random.Range(0, validChoices.Count);

        Tile tile = Instantiate(GetTile(validChoices[rand]), pos, Quaternion.identity);
        pos.y -= 5; // final position
        tile.transform.DOMove(pos, animDuration).SetEase(Ease.InQuad);
        return tile;
    }

    private Tile GetTile(TilesType tilesType)
    {
        return tilesType switch
        {
            TilesType.Blue => _tilesSO.Tiles[0],
            TilesType.Red => _tilesSO.Tiles[1],
            TilesType.Green => _tilesSO.Tiles[2],
            TilesType.Yellow => _tilesSO.Tiles[3],
            TilesType.Orange => _tilesSO.Tiles[4],
            _ => null,
        };
    }
}
