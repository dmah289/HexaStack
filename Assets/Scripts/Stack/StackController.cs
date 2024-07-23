using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    // Layer
    [SerializeField] LayerMask stackSpawnedLayer;
    [SerializeField] LayerMask gridCellLayer;
    [SerializeField] LayerMask groundLayer;


    // Data
    [SerializeField] HexagonStack currentStack;
    [SerializeField] Vector3 currentStackInitPos;
    [SerializeField] GridCell targetCell;
    [SerializeField] GridCell lastTargetCell;
    [SerializeField] Color highlightCell;
    [SerializeField] Color originalColorCell;
      
 
    // Events
    public static Action<GridCell> onStackPlaced;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleMouseDown();
        else if (Input.GetMouseButton(0) && currentStack != null)
            HandleMouseDrag();
        else if (Input.GetMouseButtonUp(0) && currentStack != null)
            HandleMouseUp();
    }

    private Ray GetClickedRay() => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void HandleMouseDown()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, stackSpawnedLayer);

        if (hit.collider == null)
            return;

        currentStack = hit.collider.transform.parent.GetComponent<Hexagon>().HexagonStack;
        currentStackInitPos = currentStack.transform.position;
    }

    private void HandleMouseDrag()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, gridCellLayer);

        if (hit.collider != null)
            DragAboveGrid(hit);
        else
            DragAboveGround();
    }

    private void HandleMouseUp()
    {
        if(targetCell == null)
        {
            currentStack.transform.position = currentStackInitPos;
            currentStack = null;
            return;
        }

        currentStack.transform.position = targetCell.transform.position.With(y: 0.28f);
        currentStack.transform.SetParent(targetCell.transform);

        targetCell.stack = currentStack;
        onStackPlaced?.Invoke(targetCell);
        
        targetCell.Color = originalColorCell;
        targetCell = null;
        lastTargetCell = null;
        currentStack = null;
    }

    private void DragAboveGround()
    {
        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, groundLayer);

        if (hit.collider == null)
            return;

        currentStack.transform.position = Vector3
            .MoveTowards(currentStack.transform.position, hit.point.With(y: 1), Time.deltaTime * 40);

        targetCell = null;

        if(lastTargetCell != null)
        {
            lastTargetCell.Color = originalColorCell;
            lastTargetCell = null;
        }
    }

    private void DragAboveGrid(RaycastHit hit)
    {
        // Chỉ xét kéo stack trên lưới
        GridCell gridCell = hit.collider.transform.parent.GetComponent<GridCell>();
        currentStack.transform.position = Vector3
                .MoveTowards(currentStack.transform.position, hit.point.With(y: 1), Time.deltaTime * 40);

        if (gridCell.IsOccupied)
            targetCell = null;
        else
        {
            if(targetCell == null && lastTargetCell == null)
            {
                targetCell = gridCell;
                targetCell.Color = highlightCell;
                lastTargetCell = targetCell;
            }
            else if(gridCell != lastTargetCell)
            {
                targetCell = gridCell;
                targetCell.Color = highlightCell;
                lastTargetCell.Color = originalColorCell;
                lastTargetCell = targetCell;
            }
        }

    }

    
}
