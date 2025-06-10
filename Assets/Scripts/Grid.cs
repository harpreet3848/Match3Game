using System;
using UnityEngine;

public class Grid<TGridObject>
{
    public int MaxRow { private set; get; }
    public int MaxColumn { private set; get; }

    Vector3 origin;

    TGridObject[,] gridObjects;

    public Grid(int x, int y,Vector3 worldOrigin)
    {
        this.MaxRow = x;
        this.MaxColumn = y;
        this.origin = worldOrigin;
        gridObjects = new TGridObject[MaxRow,MaxColumn];
    }

    public Vector3 GetCornerdWorldPosition(float X,float Y)
    {
        Vector3 result = GetWorldPosition(X,Y) - new Vector3(0.5f,-0.5f,0);
        return result;
    }
    public Vector3 GetWorldPosition(float X,float Y)
    {
        return new Vector3(X, -Y,0) + origin;
    }
    public Vector3 WorldToGridIndex(float x, float y)
    {
        Vector3 result = new Vector3(Mathf.RoundToInt(x),Mathf.RoundToInt(y)) - origin;

        result.y *= -1;

        return result;
    }
    public int GetIndex(int x, int y)
    {
        return (x * MaxColumn) + y;
    }
    public void SetGridObjectAtIndex(int r, int c, TGridObject gridObject)
    {
        gridObjects[r,c] = gridObject;
    }
    public TGridObject GetGridObjectAtIndex(int r, int c)
    {
        return gridObjects[r, c];
    }
    // use Update
    public void DrawGridBorders()
    {
        for (int x = 0; x < MaxRow; x++)
        {
            for (int y = 0; y < MaxColumn; y++)
            {
                Debug.DrawLine(GetCornerdWorldPosition(x, y), GetCornerdWorldPosition(x + 1, y));
                Debug.DrawLine(GetCornerdWorldPosition(x, y), GetCornerdWorldPosition(x, y + 1));

                //Debug.Log(x + " " + y + " :" + grid.GetIndex(x, y));
            }
        }
        Debug.DrawLine(GetCornerdWorldPosition(0, MaxColumn), GetCornerdWorldPosition(MaxRow, MaxColumn));
        Debug.DrawLine(GetCornerdWorldPosition(MaxRow, 0), GetCornerdWorldPosition(MaxRow, MaxColumn));
    }
}
