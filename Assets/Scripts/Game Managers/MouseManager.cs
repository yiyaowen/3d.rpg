using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public Texture2D moveCursor, attackCursor, adjustCursor;

    public static MouseManager instance;

    public event Action<RaycastHit> onMousePrimaryButtonClick;
    public event Action<RaycastHit> onMouseSecondaryButtonClick;

    RaycastHit mouseClickHitInfo;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMouseClickInfo();
        ExecuteMouseCommands();
    }

    void UpdateMouseClickInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out mouseClickHitInfo))
        {
            ChangeMouseCursor(mouseClickHitInfo.collider.gameObject.tag);
        }
    }

    void ExecuteMouseCommands()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onMousePrimaryButtonClick?.Invoke(mouseClickHitInfo);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            onMouseSecondaryButtonClick?.Invoke(mouseClickHitInfo);
        }
    }

    void ChangeMouseCursor(string tag)
    {
        switch (tag)
        {
        case "Ground":
            Cursor.SetCursor(moveCursor, Vector2.zero, CursorMode.Auto);
            break;
        case "Enemy":
            Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
            break;
        default:
            break;
        }
    }
}
