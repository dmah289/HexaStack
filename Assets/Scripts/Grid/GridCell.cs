using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCellSort2Comparer : IComparer<GridCell>
{
    public int Compare(GridCell x, GridCell y)
    {
        if(x.stack.colorsAmount.Count == y.stack.colorsAmount.Count)
        {
            if (x.stack.colorsAmount[x.stack.hexagons.Peek().Color] == y.stack.colorsAmount[y.stack.hexagons.Peek().Color])
            {
                if (x.stack.hexagons.Count == y.stack.hexagons.Count)
                    return x.row.CompareTo(y.row);

                return x.stack.hexagons.Count.CompareTo(y.stack.hexagons.Count);
            }
            return x.stack.colorsAmount[x.stack.hexagons.Peek().Color].CompareTo(y.stack.colorsAmount[y.stack.hexagons.Peek().Color]);
        }
        return x.stack.colorsAmount.Count.CompareTo(y.stack.colorsAmount.Count);
    }
}

public class GridCellSortFrom3CellsComparer : IComparer<GridCell>
{
    public int Compare(GridCell x, GridCell y)
    {
        if(x.row == y.row)
            return x.col.CompareTo(y.col);

        return x.row.CompareTo(y.row);
    }
}

public class GridCell : MonoBehaviour
{
    [SerializeField] Renderer renderer;
    public int row;
    public int col;
    public bool isMoving;
    public bool isCompleting;

    public HexagonStack stack;

    public Color Color
    {
        get => renderer.material.color;
        set => renderer.material.color = value;
    }

    public bool IsOccupied
    {
        get => stack != null;
        private set { }
    }

    public bool IsBusy
    {
        get => isMoving || isCompleting;
        set { }
    }    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        IsBusy = false;
    }
}
