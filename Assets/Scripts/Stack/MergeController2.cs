using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeController2 : MonoBehaviour
{
    [SerializeField] GridMatrix grid;
    int[] neighborOffsetX, neighborOffsetZ;

    [SerializeField] Queue<GridCell> updateCells = new Queue<GridCell>();

    int[,] neighborsMergableCount;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        neighborOffsetZ = new int[12] { -1, -2, -1, 1, 2, 1, -1, -2, -1, 1, 2, 1};
        neighborOffsetX = new int[12] { -1, 0, 0, 0, 0, -1, 0, 0, 1, 1, 0, 0};
    }

    private void OnEnable()
    {
        StackController.onStackPlaced += StackPlacedCallBack;
    }

    private void OnDisable()
    {
        StackController.onStackPlaced -= StackPlacedCallBack;
    }

    private void StackPlacedCallBack(GridCell currentCell)
    {
        StartCoroutine(StackPlacedCheck(currentCell));
    }

    private IEnumerator StackPlacedCheck(GridCell curentCell)
    {
        yield return new WaitForEndOfFrame();
        updateCells.Enqueue(curentCell);

        while (updateCells.Count > 0)
        {
            if(LevelManager.instance.currentHexaAmount < LevelManager.instance.currentLevel.maxHexAmount)
            {
                yield return HandleMerging(updateCells.Dequeue());
                if(updateCells.Count == 0)
                    yield return LevelManager.instance.CheckLose();
            }
            else
            {
                updateCells.Clear();
                yield break;
            }
        }    
        
    }

    private IEnumerator HandleMerging(GridCell currentCell)
    {
        if (!currentCell.IsOccupied)
            yield break;

        Color topCellColor = currentCell.stack.hexagons.Peek().Color;

        List<GridCell> similarColorNeighborCells = GetSimilarColorNeighborCells(currentCell.row, currentCell.col, grid.gridMatrix.GetLength(0), topCellColor);

        if (similarColorNeighborCells.Count == 0)
            yield break;

        similarColorNeighborCells.Add(currentCell);

        switch (similarColorNeighborCells.Count)
        {
            case 2:
                similarColorNeighborCells.QuickSortHoare(0, similarColorNeighborCells.Count - 1, new GridCellSort2Comparer());
                yield return Merge2Cells(similarColorNeighborCells[1], similarColorNeighborCells[0], topCellColor, true);
                break;

            case 3:
                yield return Merge3Cells(similarColorNeighborCells, topCellColor);
                break;

            case 4:
                yield return Merge4Cells(similarColorNeighborCells, topCellColor);
                break;

            default:
                yield break;
        }
    }

    private List<GridCell> GetSimilarColorNeighborCells(int row, int col, int gridSize, Color topCellColor)
    {
        List<GridCell> similarColorCells = new List<GridCell>();
        int upperBound = row % 2 == 1 ? 12 : 6;

        for(int i = row % 2 == 1 ? 6 : 0; i < upperBound; i++)
        {
            if (row + neighborOffsetZ[i] < 0 || row + neighborOffsetZ[i] >= gridSize || col + neighborOffsetX[i] < 0 || col + neighborOffsetX[i] >= gridSize)
                continue;
            GridCell cell = grid.gridMatrix[row + neighborOffsetZ[i], col + neighborOffsetX[i]];
            if (cell != null)
            {
                if(cell.IsOccupied && topCellColor == cell.stack.hexagons.Peek().Color)
                    similarColorCells.Add(cell);
            }
        }
        return similarColorCells;
    }

    private IEnumerator Merge2Cells(GridCell fromCell, GridCell toCell, Color topCellColor, bool checkCompleted)
    {
        while (toCell.IsBusy || fromCell.IsBusy)
        {
            Debug.Log($"{fromCell.name} - {fromCell.IsBusy} - {toCell.name} - {toCell.IsBusy}");
            yield return null;
        }
            

        if (!fromCell.IsOccupied)
            yield break;

        Queue<Hexagon> hexagonsToMove = GetHexagonsToMove(fromCell, toCell, topCellColor);

        if (hexagonsToMove.Count == 0)
            yield break;

        yield return MoveHexagons(fromCell, toCell, hexagonsToMove);

        if (checkCompleted)
        {
            yield return CheckForCompleted(toCell, topCellColor);
            yield break;
        }
    }

    private IEnumerator Merge3Cells(List<GridCell> similarColorNeighborCells, Color topCellColor)
    {
        similarColorNeighborCells.QuickSortHoare(0, similarColorNeighborCells.Count - 1, new GridCellSortFrom3CellsComparer());

        if (similarColorNeighborCells.UniqueRow())
        {
            yield return Merge2Cells(similarColorNeighborCells[2], similarColorNeighborCells[1], topCellColor, false);
            yield return new WaitForSeconds(0.2f);
            yield return Merge2Cells(similarColorNeighborCells[1], similarColorNeighborCells[0], topCellColor, true);
        }
        else
        {
            if (similarColorNeighborCells[0].row == similarColorNeighborCells[1].row)
            {
                yield return Merge2Cells(similarColorNeighborCells[0], similarColorNeighborCells[2], topCellColor, false);
                yield return new WaitForSeconds(0.2f);
                yield return Merge2Cells(similarColorNeighborCells[2], similarColorNeighborCells[1], topCellColor, true);
            }
            else
            {
                yield return Merge2Cells(similarColorNeighborCells[2], similarColorNeighborCells[0], topCellColor, false);
                yield return new WaitForSeconds(0.2f);
                yield return Merge2Cells(similarColorNeighborCells[0], similarColorNeighborCells[1], topCellColor, true);
            }
        }
    }

    private IEnumerator Merge4Cells(List<GridCell> similarColorNeighborCells, Color topCellColor)
    {
        similarColorNeighborCells.QuickSortHoare(0, similarColorNeighborCells.Count - 1, new GridCellSortFrom3CellsComparer());

        if (similarColorNeighborCells[0].row == similarColorNeighborCells[1].row)
        {
            yield return Merge2Cells(similarColorNeighborCells[0], similarColorNeighborCells[2], topCellColor, false);
            yield return new WaitForSeconds(0.2f);
            yield return Merge2Cells(similarColorNeighborCells[1], similarColorNeighborCells[2], topCellColor, false);
            yield return new WaitForSeconds(0.2f);
            yield return Merge2Cells(similarColorNeighborCells[2], similarColorNeighborCells[3], topCellColor, true);
        }
        else
        {
            yield return Merge2Cells(similarColorNeighborCells[2], similarColorNeighborCells[1], topCellColor, false);
            yield return new WaitForSeconds(0.2f);
            yield return Merge2Cells(similarColorNeighborCells[3], similarColorNeighborCells[1], topCellColor, false);
            yield return new WaitForSeconds(0.2f);
            yield return Merge2Cells(similarColorNeighborCells[1], similarColorNeighborCells[0], topCellColor, true);
        }
    }

    private Queue<Hexagon> GetHexagonsToMove(GridCell fromCell, GridCell toCell, Color topCellColor)
    {
        if (!fromCell.IsOccupied || toCell.isCompleting)
            return null;

        fromCell.isMoving = true;
        toCell.isMoving = true;

        Queue<Hexagon> hexagons = new Queue<Hexagon>();

        while(fromCell.IsOccupied && fromCell.stack.hexagons.Count > 0 && fromCell.stack.hexagons.Peek().Color.Equals(topCellColor))
        {
            Hexagon hexa = fromCell.stack.PopHexagon();
            hexagons.Enqueue(hexa);
        }

        if (fromCell.IsOccupied)
            updateCells.Enqueue(fromCell);

        return hexagons;
    }

    private IEnumerator MoveHexagons(GridCell fromCell, GridCell toCell, Queue<Hexagon> hexagonsToMove)
    {
        if (!toCell.IsOccupied || toCell.isCompleting)
        {
            fromCell.isMoving = false;
            toCell.isMoving = false;
            yield break;
        }

        float initY = (toCell.stack.hexagons.Count - 1) * 0.28f;
        float delay = 0.25f;

        while(hexagonsToMove.Count > 0)
        {
            Hexagon hexa = hexagonsToMove.Dequeue();

            initY += 0.28f;
            Vector3 targetLocalPos = Vector3.up * initY;
            delay += 0.07f;

            toCell.stack.PushHexagon(hexa);
            hexa.MoveToLocal(targetLocalPos, delay);
        }

        yield return new WaitForSeconds(delay + 0.3f);

        fromCell.isMoving = false;
        toCell.isMoving = false;
    }

    private IEnumerator CheckForCompleted(GridCell targetCell, Color topCellColor)
    {
        if (!targetCell.IsOccupied || targetCell.stack.hexagons.Count < 10)
            yield break;

        int similarColorHexaCount = 0;
        Hexagon[] hexagonsInStack = targetCell.stack.hexagons.ToArray();
        for(int i = 0; i < hexagonsInStack.Length; i++)
        {
            if (hexagonsInStack[i].Color == topCellColor)
                similarColorHexaCount++;
        }

        if(similarColorHexaCount < 10)
            yield break;

        targetCell.isCompleting = true;

        float delay = 0.3f;
        while(targetCell.IsOccupied && targetCell.stack.hexagons.Count > 0 && targetCell.stack.hexagons.Peek().Color == topCellColor)
        {
            Hexagon hexa = targetCell.stack.PopHexagon();
            delay += 0.07f;
            hexa.Vanish(delay);
        }

        if(targetCell.IsOccupied)
            updateCells.Enqueue(targetCell);

        yield return new WaitForSeconds(0.5f + delay);

        LevelManager.instance.currentHexaAmount += similarColorHexaCount;
        LevelManager.instance.HexaCountText.text = $"{LevelManager.instance.currentHexaAmount} / {LevelManager.instance.currentLevel.maxHexAmount}";
        if (LevelManager.instance.currentHexaAmount >= LevelManager.instance.currentLevel.maxHexAmount)
        {
            LevelManager.instance.currentHexaAmount = LevelManager.instance.currentLevel.maxHexAmount;
            LevelManager.instance.HexaCountText.text = $"{LevelManager.instance.currentHexaAmount} / {LevelManager.instance.currentLevel.maxHexAmount}";
            yield return new WaitForSeconds(2f);
            yield return LevelManager.instance.NextLevel();
        }

        targetCell.isCompleting = false;
    }
}
