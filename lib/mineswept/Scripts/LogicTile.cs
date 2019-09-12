using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LogicTile : MonoBehaviour
{
    private Tile baseTile;
    private int numFlaggedAdjacentLTs;
    private int activeAdjacentMinesModifier;

    public Vector2Int Coordinates { get; set; }
    public bool IsExpanded { get; private set; }
    public int NumActiveAdjacentMines { get; private set; }
    public int ActiveAdjacentMinesModifier
    {
        get { return activeAdjacentMinesModifier;}
        set { if (value >= 0) { activeAdjacentMinesModifier = value; } }
    }
    public List<LogicTile> RevealedAdjacentLTs { get; set; }
    public List<LogicTile> UnrevealedAdjacentLTs { get; set; }

    public int NumAdjacentMines
    {
        get { return baseTile.NumAdjacentMines; }
    }
    public bool IsRevealed
    {
        get { return baseTile.IsRevealed; }
    }
    public bool IsFlagged
    {
        get { return baseTile.IsFlagged; }
    }

    
    public void LogicTileSetup(Tile t, int xCoord, int yCoord)
    {
        baseTile = t;
        Coordinates = new Vector2Int(xCoord, yCoord);

        IsExpanded = false;
        RevealedAdjacentLTs = new List<LogicTile>();
        UnrevealedAdjacentLTs = new List<LogicTile>();

        activeAdjacentMinesModifier = 0;
        numFlaggedAdjacentLTs = 0;
        UpdateNumActiveAdjacentMines();
    }

    public void LogicTileSetup(LogicTile lt)
    {
        baseTile = lt.baseTile;
        Coordinates = lt.Coordinates;

        IsExpanded = lt.IsExpanded;
        RevealedAdjacentLTs = lt.RevealedAdjacentLTs;
        UnrevealedAdjacentLTs = lt.UnrevealedAdjacentLTs;

        activeAdjacentMinesModifier = lt.activeAdjacentMinesModifier;
        numFlaggedAdjacentLTs = lt.numFlaggedAdjacentLTs;
        UpdateNumActiveAdjacentMines();
    }

    public void LeftClick()
    {
        baseTile.RevealTileBtn();
    }

    public void RightClick()
    {
        baseTile.ToggleFlaggedBtn();
    }

    public void Expand()
    {
        IsExpanded = true;
        SetLogicTileColor(Color.red);

        UpdateAdjacentLTs();
    }

    public void UpdateAdjacentLTs()
    {
        foreach (LogicTile adjacent in UnrevealedAdjacentLTs.Concat(RevealedAdjacentLTs))
        {
            if (IsExpanded)
            {
                if (IsFlagged)
                {
                    adjacent.numFlaggedAdjacentLTs++;
                }

                adjacent.RevealedAdjacentLTs.Remove(this);
                adjacent.UnrevealedAdjacentLTs.Remove(this);
            }
            else if (IsRevealed)
            {
                adjacent.UnrevealedAdjacentLTs.Remove(this);

                if (!adjacent.RevealedAdjacentLTs.Contains(this))
                {
                    adjacent.RevealedAdjacentLTs.Add(this);
                }
            }

            adjacent.UpdateNumActiveAdjacentMines();
        }
    }

    public void UpdateNumActiveAdjacentMines()
    {
        NumActiveAdjacentMines = NumAdjacentMines - numFlaggedAdjacentLTs - activeAdjacentMinesModifier;

        /*
        if (NumActiveAdjacentMines < 0)
        {
            NumActiveAdjacentMines = 0;
        }
        */
    }

    public List<LogicTile> FindUnrevealedIntersect(LogicTile adjacent)
    {
        List<LogicTile> intersect = new List<LogicTile>();

        foreach (LogicTile lt in UnrevealedAdjacentLTs)
        {
            if (adjacent.UnrevealedAdjacentLTs.Contains(lt))
            {
                intersect.Add(lt);
            }
        }

        return intersect;
    }

    public void SetLogicTileColor(Color c)
    {
        c.a = 0.25f;
        GetComponent<Image>().color = c;
    }
}


/*
 * public class LogicTile : MonoBehaviour
{
    private Tile baseTile;
    private int numFlaggedAdjacentLTs;
    private int activeAdjacentMinesModifier;

    public Vector2Int Coordinates { get; set; }
    public bool IsExpanded { get; private set; }
    public int NumActiveAdjacentMines { get; private set; }
    public int ActiveAdjacentMinesModifier
    {
        get { return activeAdjacentMinesModifier;}
        set { if (value >= 0) { activeAdjacentMinesModifier = value; } }
    }
    public List<LogicTile> UnexpandedAdjacentLTs { get; set; }

    public int NumAdjacentMines
    {
        get { return baseTile.NumAdjacentMines; }
    }
    public bool IsRevealed
    {
        get { return baseTile.IsRevealed; }
    }
    public bool IsFlagged
    {
        get { return baseTile.IsFlagged; }
    }

    
    public void LogicTileSetup(Tile t, int xCoord, int yCoord)
    {
        baseTile = t;
        Coordinates = new Vector2Int(xCoord, yCoord);

        IsExpanded = false;
        UnexpandedAdjacentLTs = new List<LogicTile>();

        activeAdjacentMinesModifier = 0;
        numFlaggedAdjacentLTs = 0;
        UpdateNumActiveAdjacentMines();
    }

    public void LogicTileSetup(LogicTile lt)
    {
        baseTile = lt.baseTile;
        Coordinates = lt.Coordinates;

        IsExpanded = lt.IsExpanded;
        UnexpandedAdjacentLTs = lt.UnexpandedAdjacentLTs;

        activeAdjacentMinesModifier = lt.activeAdjacentMinesModifier;
        numFlaggedAdjacentLTs = lt.numFlaggedAdjacentLTs;
        UpdateNumActiveAdjacentMines();
    }

    public void LeftClick()
    {
        baseTile.RevealTileBtn();
    }

    public void RightClick()
    {
        baseTile.ToggleFlaggedBtn();
    }

    public void Expand()
    {
        IsExpanded = true;
        SetLogicTileColor(Color.red);

        UpdateUnexpandedAdjacentLTs();
    }

    public void UpdateUnexpandedAdjacentLTs()
    {
        foreach (LogicTile adjacent in UnexpandedAdjacentLTs)
        {
            if (IsFlagged)
            {
                adjacent.numFlaggedAdjacentLTs++;
            }

            if (IsExpanded)
            {
                adjacent.UnexpandedAdjacentLTs.Remove(this);
            }

            adjacent.UpdateNumActiveAdjacentMines();
        }
    }

    public void UpdateNumActiveAdjacentMines()
    {
        NumActiveAdjacentMines = NumAdjacentMines - numFlaggedAdjacentLTs - activeAdjacentMinesModifier;

        if (NumActiveAdjacentMines < 0)
        {
            NumActiveAdjacentMines = 0;
        }
    }

    public List<LogicTile> FindRevealedAdjacentLTs()
    {
        List<LogicTile> ruaLTs = new List<LogicTile>();

        foreach (LogicTile adjacent in UnexpandedAdjacentLTs)
        {
            if (adjacent.IsRevealed)
            {
                ruaLTs.Add(adjacent);
            }
        }

        return ruaLTs;
    }

    public List<LogicTile> FindUnrevealedAdjacentLTs()
    {
        List<LogicTile> adjacentLTs = new List<LogicTile>();

        foreach (LogicTile lt in UnexpandedAdjacentLTs)
        {
            if (!lt.IsRevealed)
            {
                adjacentLTs.Add(lt);
            }
        }

        return adjacentLTs;
    }

    public List<LogicTile> FindUnrevealedIntersect(LogicTile adjacent)
    {
        List<LogicTile> intersect = new List<LogicTile>();

        foreach (LogicTile lt in FindUnrevealedAdjacentLTs())
        {
            if (adjacent.UnexpandedAdjacentLTs.Contains(lt))
            {
                intersect.Add(lt);
            }
        }

        return intersect;
    }

    public void SetLogicTileColor(Color c)
    {
        c.a = 0.25f;
        GetComponent<Image>().color = c;
    }
}
 */
