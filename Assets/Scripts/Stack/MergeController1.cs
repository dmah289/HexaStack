using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeController1 : MonoBehaviour
{
    Queue<GridCell> updateCells = new Queue<GridCell>();

    private void OnEnable()
    {
        StackController.onStackPlaced += StackPlacedCallBack;
    }

    private void OnDisable()
    {
        StackController.onStackPlaced -= StackPlacedCallBack;
    }

    private void StackPlacedCallBack(GridCell gridCell)
    {
        StartCoroutine(StackPlacedCoroutine(gridCell));
    }

    private IEnumerator StackPlacedCoroutine(GridCell gridCell)
    {
        updateCells.Enqueue(gridCell);

        while (updateCells.Count > 0)
            yield return CheckForMerge(updateCells.Dequeue());
    }

    private IEnumerator CheckForMerge(GridCell currentCell)
    {
        if (!currentCell.IsOccupied)
            yield break;

        Color hexaOnTopCellColor = currentCell.stack.hexagons.Peek().Color;

        List<GridCell> similarColorCells = GetSimilarColorNeighborCells(currentCell, hexaOnTopCellColor);

        if(similarColorCells.Count == 0)
            yield break;

        Queue<Hexagon> hexagonsToAdd = GetHexagonsToAdd(similarColorCells, hexaOnTopCellColor);
        int delayMoveTimes = hexagonsToAdd.Count + 1;

        yield return MoveHexagons(currentCell, hexagonsToAdd);

        yield return CheckForCompleted(currentCell, hexaOnTopCellColor);
    }

    private List<GridCell> GetSimilarColorNeighborCells(GridCell curentCell, Color hexaOnTopCellColor)
    {
        LayerMask gridCellLayer = 1 << curentCell.gameObject.layer;
        List<GridCell> similarColorCells = new List<GridCell>();

        Collider[] neighborGridCellColliders = Physics.OverlapSphere(curentCell.transform.position, 2.1f, gridCellLayer);

        foreach(Collider collider in neighborGridCellColliders)
        {
            GridCell cell = collider.transform.parent.GetComponent<GridCell>();

            if(cell != curentCell && cell.IsOccupied && hexaOnTopCellColor == cell.stack.hexagons.Peek().Color)
                similarColorCells.Add(cell);
        }

        return similarColorCells;
    }

    private Queue<Hexagon> GetHexagonsToAdd(List<GridCell> similarColorCells, Color hexaOnTopCellColor)
    {
        Queue<Hexagon> hexagonsToAdd = new Queue<Hexagon>();

        foreach(GridCell cell in similarColorCells)
        {
            while(cell.IsOccupied && cell.stack.hexagons.Count > 0 && cell.stack.hexagons.Peek().Color == hexaOnTopCellColor)
            {
                Hexagon hexa = cell.stack.PopHexagon();
                hexagonsToAdd.Enqueue(hexa);
            }

            if(cell.IsOccupied)
                updateCells.Enqueue(cell);
        }
        return hexagonsToAdd;
    }

    private IEnumerator MoveHexagons(GridCell gridCell, Queue<Hexagon> hexagonsToAdd)
    {
        float initY = (gridCell.stack.hexagons.Count-1) * 0.28f;
        float delay = 0.25f;

        while(hexagonsToAdd.Count > 0)
        {
            Hexagon hexa = hexagonsToAdd.Dequeue();

            initY += 0.28f;
            Vector3 targetLocalPos = Vector3.up * initY;
            delay += 0.07f;

            gridCell.stack.PushHexagon(hexa);
            hexa.MoveToLocal(targetLocalPos, delay);
        }

        yield return new WaitForSeconds(delay + 0.75f);
    }

    private IEnumerator CheckForCompleted(GridCell gridCell, Color hexaOnTopCellColor)
    {
        if (gridCell.IsOccupied && gridCell.stack.hexagons.Count < 10)
            yield break;

        List<Hexagon> similarColorHexagons = new List<Hexagon>();

        while (gridCell.IsOccupied && gridCell.stack.hexagons.Count > 0 && gridCell.stack.hexagons.Peek().Color == hexaOnTopCellColor)
        {
            similarColorHexagons.Add(gridCell.stack.hexagons.Pop());
        }

        if (gridCell.IsOccupied && similarColorHexagons.Count < 10)
        {
            for(int i =  similarColorHexagons.Count-1 ; i >= 0; i--)
                gridCell.stack.hexagons.Push(similarColorHexagons[i]);

            yield break;
        }

        float delay = 0.2f;
        for(int i = 0; i < similarColorHexagons.Count; i++)
        {
            similarColorHexagons[i].transform.SetParent(null);
            similarColorHexagons[i].Vanish(delay);
            delay += 0.05f;
        }

        if (gridCell.IsOccupied && gridCell.stack.hexagons.Count == 0)
        {
            gridCell.stack.transform.SetParent(null);
            ObjectPooler.EnqueueObject(KeySave.hexagonStack, gridCell.stack);
            gridCell.stack = null;
        }
        else updateCells.Enqueue(gridCell);

        yield return new WaitForSeconds(0.3f + delay);
    }
}
