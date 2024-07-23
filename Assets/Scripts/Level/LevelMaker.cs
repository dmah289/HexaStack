#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum TypeObject
{
    HexaOnGrid,
    Remove
}

public class LevelMaker : MonoBehaviour
{
    public static LevelMaker instance;

    [SerializeField] public Camera _camera;
    [SerializeField] float hexa_R;
    [SerializeField] float hexa_r;

    // Prefabs
    [SerializeField] GridCellEditor gridCellEditor;

    // Level Data
    [SerializeField] int levelIndex;
    [SerializeField] int gridSize;
    [SerializeField] int maxHexAmount;
    [SerializeField] public LevelData newLevel;

    // Type
    [SerializeField] public TypeObject currentType;


    private void Awake()
    {
        if (instance == null)
            instance = this;

        newLevel = new LevelData();
        newLevel.gridSize = gridSize;
        newLevel.maxHexAmount = maxHexAmount;
        newLevel.hexGridActiveFlat = new bool[gridSize * gridSize];

        currentType = TypeObject.Remove;
        ObjectPooler.SetUpPool(KeySave.gridCellEditor, 70, gridCellEditor);
        GenerateFullSizeGrid();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleMouseDown();
        else if (Input.GetKeyDown(KeyCode.Return))
            SaveAssets();
    }

    private void GenerateFullSizeGrid()
    {
        Vector3 centerGrid = new Vector3(0, 0, 0);
        for(int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                newLevel.hexGridActiveFlat[i * gridSize + j] = true;

                GridCellEditor hex = ObjectPooler.DequeueObject<GridCellEditor>(KeySave.gridCellEditor);
                
                hex.i = i;
                hex.j = j;

                Vector3 spawnPos = CellToWorld(i, j);
                centerGrid += spawnPos;
                hex.gameObject.transform.position = spawnPos;
            }
        }
        centerGrid /= (gridSize * gridSize);
        _camera.transform.position = _camera.transform.position.With(x: centerGrid.x);
    }

    private Vector3 CellToWorld(int i, int j)
    {
        float posX = 0, posZ = 0;

        if (i % 2 == 1)
            posX = (3 * j + 1.5f) * hexa_R;
        else
            posX = 3 * j * hexa_R;

        posZ = i * hexa_r;
        return new Vector3(posX, 0, posZ);
    }

    private void HandleMouseDown()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500);

        if(hit.collider == null)
        {
            Debug.Log("Null");
            return;
        }

        GridCellEditor hex = hit.collider.transform.parent.GetComponent<GridCellEditor>();
        newLevel.hexGridActiveFlat[hex.i * gridSize +  hex.j] = false;
        ObjectPooler.EnqueueObject(KeySave.gridCellEditor, hit.collider.transform.parent.GetComponent<GridCellEditor>());
    }

    private Ray GetClickedRay() => _camera.ScreenPointToRay(Input.mousePosition);

    private void SaveAssets()
    {
        string assetPath = $"Assets/Resources/LevelData/Level {levelIndex}.asset";
        EditorUtility.SetDirty(newLevel);
        AssetDatabase.CreateAsset(newLevel, assetPath);
        AssetDatabase.SaveAssets();
    }    
}
#endif