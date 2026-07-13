using NUnit.Framework.Internal.Builders;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileBoard : MonoBehaviour
{
    public Tile tilePrefab;
    public TileState[] tileStates;

    private TileGrid grid;
    private List<Tile> tiles;

    public bool waiting {  get; private set; }

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    private void Start()
    {
        CreateTile();
        CreateTile();
    }

    private void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);

        float randomValue = Random.Range(0f, 1f);
        if (randomValue < 0.9f)
            tile.SetState(tileStates[0], 2);
        else
            tile.SetState(tileStates[1], 4);

        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);

        if (!waiting)
            StartCoroutine(WaitForChanges(false));
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        Debug.Log(waiting);
        if (ctx.performed && !waiting)
        {
            var moveVector = ctx.ReadValue<Vector2>();
            Debug.Log(moveVector);
            //  W
            if (moveVector.y == 1)
            {
                Debug.Log("W");
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            //  S
            else if (moveVector.y == -1)
            {
                Debug.Log("S");
                MoveTiles(Vector2Int.down, 0, 1, grid.height - 2, -1);
            }
            //  A
            else if(moveVector.x == -1)
            {
                Debug.Log("A");
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            //  D
            else if(moveVector.x == 1)
            {
                Debug.Log("D");
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
            }
            
        }
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
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;

        b.SetState(tileStates[index], number);
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

    private IEnumerator WaitForChanges(bool createTile = true, float time = 0.1f)
    {
        waiting = true;

        yield return new WaitForSeconds(time);

        waiting = false;

        if (createTile)
        {
            foreach(var tile in tiles)
            {
                tile.locked = false;
            }

            if(tiles.Count != grid.size)
            {
                CreateTile();
            }
        }
    }
}