using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StackSpawner : MonoBehaviour
{
    [SerializeField] int stackPlacedCount;

    [SerializeField] int minStackSize, maxStackSize;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        StackController.onStackPlaced += StackPlacedCallBack;
    }

    private void OnDisable()
    {
        StackController.onStackPlaced -= StackPlacedCallBack;
    }

    private void StackPlacedCallBack(GridCell cell)
    {
        stackPlacedCount++;
        if(stackPlacedCount >= 3)
        {
            stackPlacedCount = 0;
            SpawnStacks();
        }
    }

    public void SpawnStacks()
    {
        stackPlacedCount = 0;
        LevelManager.instance.SetUpStackSpawned();
        for(int i = 0; i < 3; i++)
            SpawnStack(LevelManager.instance.spawnedStacks[i], i);
    }

    private void SpawnStack(HexagonStack hexagonStack, int index)
    {
        int stackSize = UnityEngine.Random.Range(minStackSize, maxStackSize);
        int firstColorHexaCount = UnityEngine.Random.Range(0, stackSize);

        Color[] stackColors = GetRandomColors();

        for(int i = 0; i < stackSize; i++)
        {
            Vector3 hexaLocalPos = Vector3.up * i * 0.28f;
            Vector3 spawnPos = hexagonStack.transform.TransformPoint(hexaLocalPos);

            Hexagon hexa = ObjectPooler.DequeueObject<Hexagon>(KeySave.hexagon);
            hexa.gameObject.transform.position = spawnPos;
            hexa.Color = i < firstColorHexaCount ? stackColors[0] : stackColors[1];
            hexa.HexagonStack = hexagonStack;
            
            hexagonStack.PushHexagon(hexa);
        }
    }

    private Color[] GetRandomColors()
    {
        List<Color> colorList = new List<Color>();
        colorList.AddRange(LevelManager.instance.currentLevel.colors);

        if (colorList.Count < 2)
            return null;

        Color firstColor = colorList.OrderBy(x => UnityEngine.Random.value).First();
        colorList.Remove(firstColor);
        Color secondColor = colorList.OrderBy(x => UnityEngine.Random.value).First();
        return new Color[] { firstColor, secondColor };
    }
}
