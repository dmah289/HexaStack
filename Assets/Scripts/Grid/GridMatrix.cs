using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridMatrix : MonoBehaviour
{
    [SerializeField] float hexa_R;
    [SerializeField] float hexa_r;

    public GridCell[,] gridMatrix;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public Vector3 GenerateMatrix()
    {  
        int gridSize = LevelManager.instance.currentLevel.gridSize;
        int activeCount = 0;
        Vector3 centerGrid = new Vector3(0, 0, 0);

        gridMatrix = new GridCell[gridSize, gridSize];
        for(int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                if (LevelManager.instance.currentLevel.hexGridActiveFlat[i * gridSize + j])
                {
                    activeCount++;
                    Vector3 spawnPos = CellToWorldPos(i, j);

                    gridMatrix[i,j] = ObjectPooler.DequeueObject<GridCell>(KeySave.gridCell);

                    gridMatrix[i, j].row = i;
                    gridMatrix[i, j].col = j;
                    gridMatrix[i, j].gameObject.transform.SetParent(gameObject.transform);
                    gridMatrix[i, j].gameObject.transform.localPosition = spawnPos;
#if UNITY_EDITOR
                    gridMatrix[i, j].name = $"Cell[{i}][{j}]";
#endif

                    centerGrid += gridMatrix[i, j].gameObject.transform.position;
                }
            }
        }
        centerGrid /= activeCount;
        LevelManager.instance._camera.gameObject.transform.position = LevelManager.instance._camera.gameObject.transform.position
            .With(x: centerGrid.x, z: centerGrid.z-9);
        return centerGrid;
    }

    public void RevokeCell()
    {
        for(int i = 0; i < gridMatrix.GetLength(0); i++)
        {
            for(int j = 0; j < gridMatrix.GetLength(1); j++)
            {
                if (gridMatrix[i,j] != null)
                {
                    gridMatrix[i, j].transform.parent = null;
                    ObjectPooler.EnqueueObject(KeySave.gridCell, gridMatrix[i,j]);
                }
            }
        }
    }

    private Vector3 CellToWorldPos(int i, int j)
    {
        float posX = 0, posZ = 0;

        if (i % 2 == 1)
            posX = (3 * j + 1.5f) * hexa_R;
        else
            posX = 3 * j * hexa_R;

        posZ = i * hexa_r;
        return new Vector3(posX, 0, posZ);
    }
}
