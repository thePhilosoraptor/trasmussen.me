using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/*
 *  Random selections when stuck (beginning and middle/end)
 *                                                              TOTAL
 *      games               1000        1000        1000        24846       3000
 *      flubs               935         866         994         23234       2795
 *      wins                312         326         317         8000        955
 *      percent wins        16.124      17.47       15.80769    16.6389     16.479
 *      avg win duration    .8733332s   .7253373s   .732492s    .6801745s   .777054s
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */

public class AIPlayer : MonoBehaviour
{
    public GameObject logicTilePrefab;

    private GameController gc;
    public bool RunAI { get; private set; }
    public int LogicFieldSize
    {
        get { return unrevealedLTs.Count + unexpandedRevealedLTs.Count; }
    }
    private LogicTile[,] LTfield;
    private List<LogicTile> unrevealedLTs;
    private List<LogicTile> unexpandedRevealedLTs;
    private bool newGame;
    public int numMinesRemaining;

    public void Start()
    {
        gc = FindObjectOfType<GameController>();
        RunAI = false;

        InitializeAI();
    }

    /*
     *  If AI is running, steps AI once
     *  Steps AI until game is deactivated or there are no more unrevealed LogicTiles, then stops AI
     */
    public void Update()
    {
        if (!RunAI)
        {
            return;
        }

        StepAI();

        if (unrevealedLTs.Count == 0)
        {
            RunAI = false;
        }

        if (newGame && gc.GameIsActive)
        {
            newGame = false;
        }
        else if (!newGame && !gc.GameIsActive)
        {
            RunAI = false;
        }
    }

    /*
     *  Returns a list of all adjacent LogicTiles, revealed or unrevealed
     *  Ignores expanded adjacent LogicTiles
     *      (x, y) +/- 1
     */
    private List<LogicTile> FindAdjacentLTs(LogicTile lt)
    {
        List<LogicTile> adjacentLTs = new List<LogicTile>();

        // Iterate over x position -/+ 1
        for (int x = lt.Coordinates.x - 1; x <= lt.Coordinates.x + 1; x++)
        {
            // Skip iteration if x is outside bounds of LTfield
            if (x < 0 || x >= LTfield.GetLength(0))
            {
                continue;
            }
            else
            {
                // Iterate over y position -/+ 1
                for (int y = lt.Coordinates.y - 1; y <= lt.Coordinates.y + 1; y++)
                {
                    // Skip iteration if y is outside bounds of LTfield, or if [x, y] = [lt.Coordinates.x, lt.Coordinates.y],
                    //      otherwise, if adjacent LogicTile is not expanded, add it to the adjacent list
                    if (y < 0 || y >= LTfield.GetLength(1) || (x == lt.Coordinates.x && y == lt.Coordinates.y))
                    {
                        continue;
                    }
                    else
                    {
                        if (!LTfield[x, y].IsExpanded)
                        {
                            adjacentLTs.Add(LTfield[x, y]);
                        }
                    }
                }
            }
        }

        return adjacentLTs;
    }

    private void FindAndSetNumMinesRemaining()
    {
        bool ifSuccess = int.TryParse(gc.numMinesLeftText.text, out numMinesRemaining);
    }

    /*
     *  Set up a new LogicTile field:
     *      If an old LTfield exists, destroys it first
     *      Generates a new LTfield of the same dimensions as Minefield, one LogicTile for each Tile
     */
    public void InitializeAI()
    {
        newGame = true;
        RunAI = false;

        FindAndSetNumMinesRemaining();

        // Deletes all old LogicTiles
        if (LTfield != null)
        {
            foreach (LogicTile lt in unrevealedLTs)
            {
                Destroy(lt.gameObject);
            }
            foreach (LogicTile lt in unexpandedRevealedLTs)
            {
                Destroy(lt.gameObject);
            }
        }

        unrevealedLTs = new List<LogicTile>();
        unexpandedRevealedLTs = new List<LogicTile>();

        // Create new LogicTiles to represent every Tile in the Minefield and add them to LTfield and unrevealedLTs list
        LTfield = new LogicTile[gc.Minefield.GetLength(0), gc.Minefield.GetLength(1)];

        for (int x = 0; x < LTfield.GetLength(0); x++)
        {
            for(int y = 0; y < LTfield.GetLength(1); y++)
            {
                Tile curr = gc.Minefield[x, y];
                GameObject go = Instantiate(logicTilePrefab, curr.transform.position, Quaternion.identity, transform.GetChild(0));
                LogicTile newLT = go.GetComponent<LogicTile>();
                newLT.LogicTileSetup(curr, x, y);

                LTfield[x, y] = newLT;
                unrevealedLTs.Add(newLT);
            }
        }

        // Initializes unrevealed adjacent LogicTiles list for all LogicTiles
        foreach (LogicTile lt in LTfield)
        {
            lt.UnrevealedAdjacentLTs = FindAdjacentLTs(lt);
        }
    }

    /*
     *  Search through unrevealedLTs list for Tiles that are newly revealed
     */
    private List<LogicTile> FindNewlyRevealedTiles()
    {
        return unrevealedLTs.FindAll(lt => lt.IsRevealed);
    }

    /*
     *  Check for newly revealed Tiles and updates unexpandedRevealedLTs and unrevealedLTs accordingly
     *  For any newly revealed LogicTiles:
     *      Sets color
     *      Updates number of active adjacent mines
     *      Updates unexpanded adjacency list
     */
    private void UpdateRevealedLTsLists()
    {
        List<LogicTile> newlyRevealedLTs = FindNewlyRevealedTiles();

        foreach (LogicTile lt in newlyRevealedLTs)
        {
            lt.SetLogicTileColor(Color.green);
            lt.UpdateNumActiveAdjacentMines();
            lt.UpdateAdjacentLTs();

            unexpandedRevealedLTs.Add(lt);
            unrevealedLTs.Remove(lt);
        }
    }

    /*
     *  Checks if any unrevealed LogicTiles have been flagged, if so expands them
     *      Updates adjacent LogicTiles' UnexpandedAdjacentLTs and NumActiveAdjacentMines
     */
    private void UpdateFlaggedLTs()
    {
        List<LogicTile> newlyFlaggedLTs = new List<LogicTile>();

        foreach (LogicTile lt in unrevealedLTs)
        {
            if (lt.IsFlagged)
            {
                newlyFlaggedLTs.Add(lt);
            }
        }

        foreach (LogicTile lt in newlyFlaggedLTs)
        {
            numMinesRemaining--;
            ExpandLT(lt);
        }
    }

    /*
     *  Simulates a left click on unrevealed adjacent LogicTiles
     */
    public void RevealUnrevealedAdjacentLTs(LogicTile lt)
    {
        foreach (LogicTile adjacent in lt.UnrevealedAdjacentLTs)
        {
            adjacent.LeftClick();
        }
    }

    /*
     *  Expands LogicTile (which updates adjacent LogicTiles), removes it from the LogicTile lists, and destroys it
     */
    public void ExpandLT(LogicTile lt)
    {
        lt.Expand();
        unrevealedLTs.Remove(lt);
        unexpandedRevealedLTs.Remove(lt);
        Destroy(lt.gameObject);
    }

    /*
     *  Applies basic game logic exactly once on every LogicTile
     */
    private List<LogicTile> ApplyBasicLogic()
    {
        List<LogicTile> newlyExpandedLTs = new List<LogicTile>();

        foreach (LogicTile lt in unexpandedRevealedLTs)
        {
            // If there are no more active adjacent mines,
            //      reveal all unrevealed adjacent tiles and add this LogicTile to the expanded list
            //      then remove this tile from the unrevealed list
            if (lt.NumActiveAdjacentMines <= 0)
            {
                RevealUnrevealedAdjacentLTs(lt);
                newlyExpandedLTs.Add(lt);
                continue;
            }
            else
            {
                // Find and store unrevealed adjacent LogicTiles
                List<LogicTile> uaLTs = lt.UnrevealedAdjacentLTs;

                // If there are no unrevealed adjacent LogicTiles,
                //      all adjacent Tiles are known so add this LogicTile to the newly expanded list
                // Else if the # of unrevealed adjacent LogicTiles = # active adjacent mines,
                //      all adjacent unrevealed Tiles are mines, so flag them
                //      then add this LogicTile to the newly expanded list
                if (uaLTs.Count == 0)
                {
                    newlyExpandedLTs.Add(lt);
                    continue;
                }
                else if (uaLTs.Count == lt.NumActiveAdjacentMines)
                {
                    foreach (LogicTile adjacent in uaLTs)
                    {
                        if (!adjacent.IsFlagged)
                        {
                            adjacent.RightClick();
                        }
                    }

                    newlyExpandedLTs.Add(lt);

                    continue;
                }
            }
        }

        return newlyExpandedLTs;
    }

    /*
     *  Instantiates two new LogicTiles in the same location based on the intersecting unrevealed LogicTiles with an adjacent LogicTile
     *      Unexpanded adjacent LogicTiles lists for both contain disjoint unrevealed LogicTiles and the same revealed LogicTiles
     *      Doesn't affect the original LogicTile
     *  Splits the number of active adjacent mines across the disjoint unrevealed adjacent LogicTiles
     *  Sets LogicTile color to yellow
     */
    private List<LogicTile> SplitLT(LogicTile ltToSplit, List<LogicTile> adjacentIntersect, List<LogicTile> adjacentDisjoint, int numSplitOffMines)
    {
        GameObject goNewDisjoint = Instantiate(logicTilePrefab, ltToSplit.transform.position, Quaternion.identity, transform.GetChild(0));
        GameObject goNewIntersect = Instantiate(logicTilePrefab, ltToSplit.transform.position, Quaternion.identity, transform.GetChild(0));

        LogicTile ltNewDisjoint = goNewDisjoint.GetComponent<LogicTile>();
        LogicTile ltNewIntersect = goNewIntersect.GetComponent<LogicTile>();

        ltNewDisjoint.LogicTileSetup(ltToSplit);
        ltNewIntersect.LogicTileSetup(ltToSplit);

        ltNewDisjoint.UnrevealedAdjacentLTs = ltToSplit.UnrevealedAdjacentLTs.Except(adjacentIntersect).ToList();
        ltNewIntersect.UnrevealedAdjacentLTs = ltToSplit.UnrevealedAdjacentLTs.Except(adjacentDisjoint).ToList();

        ltNewDisjoint.RevealedAdjacentLTs = ltToSplit.RevealedAdjacentLTs;
        ltNewIntersect.RevealedAdjacentLTs = ltToSplit.RevealedAdjacentLTs;

        ltNewDisjoint.ActiveAdjacentMinesModifier += numSplitOffMines;
        ltNewIntersect.ActiveAdjacentMinesModifier += ltToSplit.NumActiveAdjacentMines - numSplitOffMines;

        ltNewDisjoint.UpdateNumActiveAdjacentMines();
        ltNewIntersect.UpdateNumActiveAdjacentMines();

        ltNewDisjoint.SetLogicTileColor(Color.yellow);
        ltNewIntersect.SetLogicTileColor(Color.yellow);

        return new List<LogicTile>() { ltNewDisjoint, ltNewIntersect };
    }

    /*
     *  Applies Logic to split LogicTiles if information can be gained by the fact that they share intersecting unrevealed adjacent LogicTiles
     */
    private List<LogicTile> ApplySplitLogic()
    {
        List<LogicTile> allNewSplitLTs = new List<LogicTile>();
        List<LogicTile> newlyExpandedLTs = new List<LogicTile>();

        foreach (LogicTile lt in unexpandedRevealedLTs)
        {
            foreach (LogicTile adjacent in unexpandedRevealedLTs)
            {
                // If LogicTiles are the same or either was already split in this function call, skip the split step
                if (lt != adjacent && !newlyExpandedLTs.Contains(lt) && !newlyExpandedLTs.Contains(adjacent))
                {
                    // Generate intersecting unrevealed adjacent LogicTiles list
                    List<LogicTile> intersect = lt.FindUnrevealedIntersect(adjacent);

                    // If there is an intersect
                    //      Generate disjoint lists of LogicTiles such that
                    //      ltDisjoint + intersect + adjacentDisjoint represents all unrevealed adjacent LogicTiles
                    //      Calculate the maximum and minimum number of mines the intersect must contain
                    //      If min = max, intersect must contain exactly that number of mines
                    //          If the LogicTiles have disjoint unrevealed adjacent LogicTiles, split into two new LogicTiles
                    if (intersect.Count != 0)
                    {
                        List<LogicTile> ltDisjoint = lt.UnrevealedAdjacentLTs.Except(intersect).ToList();
                        List<LogicTile> adjacentDisjoint = adjacent.UnrevealedAdjacentLTs.Except(intersect).ToList();

                        int maxNumMinesInIntersect = Mathf.Min(lt.NumActiveAdjacentMines, adjacent.NumActiveAdjacentMines, intersect.Count);
                        int minNumMinesInIntersect = Mathf.Max((lt.NumActiveAdjacentMines - ltDisjoint.Count), (adjacent.NumActiveAdjacentMines - adjacentDisjoint.Count));

                        if (minNumMinesInIntersect == maxNumMinesInIntersect)
                        {
                            List<LogicTile> newSplitLTs = new List<LogicTile>();

                            if (ltDisjoint.Count != 0)
                            {
                                newSplitLTs.AddRange(SplitLT(lt, intersect, ltDisjoint, minNumMinesInIntersect));
                                newlyExpandedLTs.Add(lt);
                            }

                            if (adjacentDisjoint.Count != 0)
                            {
                                newSplitLTs.AddRange(SplitLT(adjacent, intersect, adjacentDisjoint, minNumMinesInIntersect));
                                newlyExpandedLTs.Add(adjacent);
                            }

                            foreach (LogicTile nlt in newSplitLTs)
                            {
                                foreach (LogicTile unexpandedAdjacent in nlt.UnrevealedAdjacentLTs.Concat(nlt.RevealedAdjacentLTs))
                                {
                                    unexpandedAdjacent.RevealedAdjacentLTs.Add(nlt);
                                }

                                foreach (LogicTile othernlt in newSplitLTs)
                                {
                                    if (nlt.Coordinates != othernlt.Coordinates)
                                    {
                                        nlt.RevealedAdjacentLTs.Add(othernlt);
                                    }
                                }
                            }

                            allNewSplitLTs.AddRange(newSplitLTs);
                        }
                    }
                }
            }
        }

        // Add newly created LogicTiles to the unexpanded revealed LogicTiles list
        foreach (LogicTile nlt in allNewSplitLTs)
        {
            unexpandedRevealedLTs.Add(nlt);
        }

        return newlyExpandedLTs;
    }

    /*
     *  
     */
    public void StepAI()
    {
        UpdateRevealedLTsLists();
        UpdateFlaggedLTs();

        //  If there are no more unflagged mines to find
        //      reveal all unrevealed tiles and do nothing else
        //  Else if the number of remaining mines equals the number of unrevealed LogicTiles
        //      flag all remaining unrevealed LogicTiles and do nothing else
        if (numMinesRemaining == 0)
        {
            foreach (LogicTile lt in unrevealedLTs)
            {
                lt.LeftClick();
            }

            return;
        }
        else if (numMinesRemaining == unrevealedLTs.Count)
        {
            foreach (LogicTile lt in unrevealedLTs)
            {
                lt.RightClick();
            }

            return;
        }

        List<LogicTile> newlyExpandedLTs;

        // Apply basic logic rules and record if any tiles were expanded
        newlyExpandedLTs = ApplyBasicLogic();

        // If there were any newly expanded LogicTiles,
        //      Finishes expansion to destroy it
        // Else
        //      Checks to see if more information can be gained by splitting LogicTiles
        if (newlyExpandedLTs.Count > 0)
        {
            foreach (LogicTile lt in newlyExpandedLTs)
            {
                ExpandLT(lt);
            }

            return;
        }
        else
        {
            // Apply split logic rules and record if any tiles were expanded/split
            newlyExpandedLTs = ApplySplitLogic();

            // If any LogicTiles were split, expands the original LogicTile that was split to destroy it
            foreach (LogicTile lt in newlyExpandedLTs)
            {
                ExpandLT(lt);
            }
        }


        // If nothing else worked, pick a random LogicTile to reveal
        if (newlyExpandedLTs.Count == 0)
        {
            int rand = Random.Range(0, unrevealedLTs.Count);
            //print(unrevealedLTs[rand].Coordinates);
            unrevealedLTs[rand].LeftClick();
        }
    }

    public void ToggleRunAI()
    {
        RunAI = !RunAI;
    }
}
