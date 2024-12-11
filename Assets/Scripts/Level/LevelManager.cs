using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [SerializeField] public Camera _camera;

    // Prefabs
    [SerializeField] GridCell gridCell;
    [SerializeField] HexagonStack hexagonStack;
    [SerializeField] Hexagon hexagon;

    // Parents
    [SerializeField] GridMatrix gridMatrix;
    [SerializeField] StackSpawner stackSpawner;

    // Management
    [SerializeField] public List<HexagonStack> spawnedStacks;
    [SerializeField] public int currentHexaAmount;
    [SerializeField] public int maxLevel = 4;
    [SerializeField] public Text HexaCountText;

    // Level Data
    [SerializeField] public LevelData currentLevel;
    [SerializeField] int currentLevelIndex;
    [SerializeField] Vector3 gridCenter;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(instance == null)
            instance = this;

        // Initialization
        spawnedStacks = new List<HexagonStack>();
        SetUpPool();

        // Load level data
        currentLevel = LevelData.Database.GetCurrentLevelByIndex(currentLevelIndex);
        gridCenter = gridMatrix.GenerateMatrix();
        stackSpawner.SpawnStacks();

        currentHexaAmount = 0;
        HexaCountText.text = $"{currentHexaAmount} / {currentLevel.maxHexAmount}";

    }

    private void SetUpPool()
    {
        ObjectPooler.SetUpPool(KeySave.gridCell, 5, gridCell);
        ObjectPooler.SetUpPool(KeySave.hexagonStack, 5, hexagonStack);
        ObjectPooler.SetUpPool(KeySave.hexagon, 5, hexagon);
    }

    public void SetUpStackSpawned()
    {
        spawnedStacks.Clear();
        for (int i = 0; i < 3; i++)
            spawnedStacks.Add(ObjectPooler.DequeueObject<HexagonStack>(KeySave.hexagonStack));

        spawnedStacks[1].transform.position = gridCenter.With(z: gridCenter.z-8.5f);
        spawnedStacks[0].transform.position = spawnedStacks[1].transform.position.With(x: spawnedStacks[1].transform.position.x-2.75f);
        spawnedStacks[2].transform.position = spawnedStacks[1].transform.position.With(x: spawnedStacks[1].transform.position.x+2.75f);
    }

    public IEnumerator CheckLose()
    {
        yield return new WaitForEndOfFrame();
        for(int i = 0; i < currentLevel.gridSize; i++)
        {
            for(int j = 0; j < currentLevel.gridSize; j++)
            {
                if (gridMatrix.gridMatrix[i, j] != null && !gridMatrix.gridMatrix[i, j].IsOccupied)
                {
                    yield break;
                }    
            }
        }
        yield return VanishAllStacks();
        stackSpawner.SpawnStacks();
    }

    private IEnumerator VanishAllStacks()
    {
        currentHexaAmount = 0;
        float maxDelay = float.MinValue;

        for(int i = 0; i < currentLevel.gridSize; i++)
        {
            for(int j = 0; j < currentLevel.gridSize; j++)
            {
                GridCell cell = gridMatrix.gridMatrix[i, j];
                if (cell != null && cell.IsOccupied)
                {
                    float delay = 0.25f;
                    while(cell.IsOccupied && cell.stack.hexagons.Count > 0)
                    {
                        Hexagon hexa = cell.stack.PopHexagon();
                        delay += 0.07f;
                        hexa.Vanish(delay);
                    }
                    maxDelay = Mathf.Max(maxDelay, delay);
                    cell.isMoving = false;
                    cell.isCompleting = false;
                }
            }
        }

        yield return new WaitForSeconds(1f + maxDelay);

        maxDelay = float.MinValue;
        for(int i = 0; i < 3; i++)
        {
            HexagonStack stack = spawnedStacks[i];
            if (stack != null)
            {
                float delay = 0.25f;
                while (stack.hexagons.Count > 0)
                {
                    Hexagon hexa = stack.PopHexagon();
                    delay += 0.07f;
                    hexa.Vanish(delay);
                }
                maxDelay = Mathf.Max(maxDelay, delay);
            }        
        }

        yield return new WaitForSeconds(1f + maxDelay);
    }

    public IEnumerator NextLevel()
    {
        yield return VanishAllStacks();

        currentLevelIndex = (currentLevelIndex + 1) % maxLevel;
        currentLevel = LevelData.Database.GetCurrentLevelByIndex(currentLevelIndex);
        HexaCountText.text = $"{currentHexaAmount} / {currentLevel.maxHexAmount}";

        gridMatrix.RevokeCell();
        yield return new WaitForSeconds(0.5f);
        gridCenter = gridMatrix.GenerateMatrix();
        yield return new WaitForEndOfFrame();
        stackSpawner.SpawnStacks();
    }
}
