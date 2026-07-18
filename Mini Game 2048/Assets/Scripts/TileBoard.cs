using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;

public class TileBoard : MonoBehaviour
{
    public GameManager gameManager;

    public Tile tilePrefab;
    public GameObject floatingTextPrefab;
    public TileState[] tileStates;

    private TileGrid grid;
    private List<Tile> tiles;

    public bool waiting {  get; private set; }

    private Vector2 _touchStartPos;
    private bool _isSwiping = false;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable(); // 必须启用
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleTouchSwipe();
    }

    private void HandleTouchSwipe()
    {
        if (waiting) return; // 动画期间直接忽略所有触摸（包括 Begin 和 Ended）

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 0) return;

        var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            // 只有在非等待状态下才记录滑动起点
            _touchStartPos = touch.screenPosition;
            _isSwiping = true;
        }
        else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && _isSwiping)
        {
            _isSwiping = false;
            Vector2 swipeDelta = touch.screenPosition - _touchStartPos;

            float threshold = Mathf.Min(Screen.width, Screen.height) * 0.05f;
            if (swipeDelta.magnitude < threshold) return;

            Vector2Int direction = Vector2Int.zero;
            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                direction.x = swipeDelta.x > 0 ? 1 : -1;
            else
                direction.y = swipeDelta.y > 0 ? 1 : -1;

            // 调用移动逻辑（与键盘一致）
            if (direction.y == 1)
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            else if (direction.y == -1)
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
            else if (direction.x == -1)
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            else if (direction.x == 1)
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
    }

    public void ClearBoard()
    {
        foreach(var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);

        float randomValue = Random.Range(0f, 1f);
        if (randomValue < 0.9f)
            tile.SetState(tileStates[0], 2);
        else
            tile.SetState(tileStates[1], 4);

        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        // ----- 其他
        // 只处理 performed，且必须可移动（waiting == false）
        if (ctx.control.device is Touchscreen || ctx.control.device is Pointer) return;

        if (!ctx.performed || waiting) return;

        Vector2 moveVector = ctx.ReadValue<Vector2>();

        // 你原来的方向判断，完全照搬
        if (moveVector.y == 1)
        {
            MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (moveVector.y == -1)
        {
            MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
        }
        else if (moveVector.x == -1)
        {
            MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (moveVector.x == 1)
        {
            MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
        }
        return;
    }
    
    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        for(int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for(int  y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if(CanMerge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }

                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.number == b.number && b.number != 2048 && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;

        b.SetState(tileStates[index], number);
        StartCoroutine(b.MergeAnimate());

        gameManager.IncreaseScore(number);
        GameObject floatingText = Instantiate(floatingTextPrefab, this.transform.parent);
        StartCoroutine(floatingText.GetComponent<FloatingText>().SetText("+" + number));
    }

    private int IndexOf(TileState state)
    {
        for(int i = 0; i < tileStates.Length; i++)
        {
            if(tileStates[i] == state)
            {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForChanges(float time = 0.1f)
    {
        waiting = true;

        yield return new WaitForSeconds(time);

        waiting = false;

        foreach(var tile in tiles)
            tile.locked = false;

        if(tiles.Count != grid.size)
            CreateTile();

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    private bool CheckForGameOver()
    {
        if(tiles.Count != grid.size)
        {
            return false;
        }

        foreach(var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.tile))
                return false;
            if (down != null && CanMerge(tile, down.tile))
                return false;
            if (left != null && CanMerge(tile, left.tile))
                return false;
            if (right != null && CanMerge(tile, right.tile))
                return false;
        }

        return true;
    }
}