using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonStack : MonoBehaviour
{
    public Stack<Hexagon> hexagons;
    public Dictionary<Color, int> colorsAmount;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        StackController.onStackPlaced += DisableHexaColliders;
    }

    private void OnDisable()
    {
        StackController.onStackPlaced -= DisableHexaColliders;
    }

    public void PushHexagon(Hexagon hexa)
    {
        if(hexagons == null) hexagons = new Stack<Hexagon>();
        if(colorsAmount == null) colorsAmount = new Dictionary<Color, int>();

        hexagons.Push(hexa);

        if (!colorsAmount.ContainsKey(hexa.Color))
            colorsAmount.Add(hexa.Color, 1);
        else colorsAmount[hexa.Color]++;

        hexa.transform.SetParent(transform);
    }

    public Hexagon PopHexagon()
    {
        Hexagon hexa = hexagons.Pop();
        hexa.transform.parent = null;

        colorsAmount[hexa.Color]--;
        if (colorsAmount[hexa.Color] == 0)
            colorsAmount.Remove(hexa.Color);

        if (hexagons.Count == 0)
        {
            GridCell parent;
            if(transform.parent != null && transform.parent.TryGetComponent<GridCell>(out parent))
            {
                transform.parent = null;
                parent.stack = null;
            }
            ObjectPooler.EnqueueObject(KeySave.hexagonStack, gameObject.GetComponent<HexagonStack>());

        }

        return hexa;
    }

    public void DisableHexaColliders(GridCell gridCell)
    {
        Hexagon[] hexaArray = gridCell.stack.hexagons.ToArray();
        for (int i = hexaArray.Length - 1; i >= 0; i--)
            hexaArray[i].DisableCollider();
    }
}
