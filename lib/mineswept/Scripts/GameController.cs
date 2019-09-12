using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject tilePrefab;
    public Text numMinesLeftText;
    public Text stopwatchText;
    public int defaultRows;
    public int defaultCols;
    public int defaultNumMines;

    public Tile[,] Minefield { get; private set; }
    public float DurationOfGame { get; private set; }
    public int MinefieldSize { get { return rows * cols; } }
    public bool GameIsActive { get; private set; }
    public bool WonGame { get; private set; }
    private bool newGame;
    private bool runStopwatch;
    private bool generateNewMineLocations;

    private float tileWidth;
    private float tileHeight;
    private int rows;
    private int cols;
    private int numMines;
    private int numFlagged;
    

    void Awake()
    {
        RectTransform rt = tilePrefab.GetComponent<RectTransform>();
        tileWidth = rt.rect.width;
        tileHeight = rt.rect.height;

        //NewDefaultGame();
        NewHardGame();
    }

    /*
     *  Updates stopwatch
     */
    void FixedUpdate()
    {
        if (GameIsActive && runStopwatch)
        {
            DurationOfGame += Time.deltaTime;
        }

        if (newGame && GameIsActive)
        {
            newGame = false;
            runStopwatch = true;
        }
        else if (!newGame && !GameIsActive)
        {
            runStopwatch = false;
        }

        stopwatchText.text = DurationOfGame.ToString("F2");

    }

    public void NewDefaultGame()
    {
        GenerateNewGame(defaultRows, defaultCols, defaultNumMines);
    }

    public void NewEasyGame()
    {
        GenerateNewGame(9, 9, 10);
    }

    public void NewIntermediateGame()
    {
        GenerateNewGame(16, 16, 40);
    }

    public void NewHardGame()
    {
        GenerateNewGame(16, 30, 99);
    }

    public void NewGame()
    {
        GenerateNewGame(rows, cols, numMines);
    }

    /*
     *  Set up a new game:
     *      If an old minefield exists, destroys it first
     *      Generates a new minefield of specified dimensions
     *      (if # of rows is larger than # of columns, swaps them so the board is wider than it is tall)
     *      Repositions the camera so it is centered over the new minefield
     */
    public void GenerateNewGame(int newRows, int newCols, int newNumMines)
    {
        numFlagged = 0;
        GameIsActive = false;
        newGame = true;
        runStopwatch = false;
        DurationOfGame = 0;
        WonGame = false;
        generateNewMineLocations = true;

        if (newCols < newRows)
        {
            (newRows, newCols) = (newCols, newRows);
        }

        rows = newRows;
        cols = newCols;
        numMines = newNumMines;

        UpdateMinesLeftDisplay();

        // If this is not the first game, Destroy the old minefield
        if (Minefield != null)
        {
            foreach (Tile t in Minefield)
            {
                Destroy(t.gameObject);
            }
        }

        // Generate the new empty minefield Tiles, mines are generated on first click
        Minefield = new Tile[cols, rows];

        for (int h = 0; h < rows; h++)
        {
            for (int w = 0; w < cols; w++)
            {
                GameObject go = Instantiate(tilePrefab, new Vector3(w * tileWidth, h * tileHeight, 0), Quaternion.identity, transform);
                Minefield[w, h] = go.GetComponent<Tile>();
            }
        }

        // Reposition UI elements
        transform.GetChild(0).transform.position = new Vector3((cols * tileWidth / 2) - 275f, 0, 0);

        // Reposition camera so minefield is centered
        FindObjectOfType<Camera>().transform.position = new Vector3((cols * tileWidth / 2) - (tileWidth / 2), (rows * tileHeight / 2) - (tileWidth / 2) - 125, -10);
    }

    /*
     *  Randomly determines mine locations and turns those Tiles into mines
     *  Updates surrounding non-mine Tiles to correctly reflect the number of adjacent mines
     *  Ensures first Tile clicked is not a mine
     */
    private void GenerateMines(Tile firstTileClicked)
    {
        List<Vector2Int> locs = new List<Vector2Int>();

        // Loop until the desired number of mines are generated
        while (locs.Count < numMines)
        {
            Vector2Int randomLoc = new Vector2Int(Random.Range(0, cols), Random.Range(0, rows));

            // Checks to make sure random location is not already a mine and is not the location of the first clicked Tile
            // If it is neither, Tile is made into a mine and adjacent Tiles are updated
            if (!locs.Contains(randomLoc) && (firstTileClicked != Minefield[randomLoc.x, randomLoc.y]))
            {
                Minefield[randomLoc.x, randomLoc.y].MakeMine();

                // Increment surrounding Tiles' numAdjacentMines
                foreach (Tile adjacent in FindAdjacentTiles(Minefield[randomLoc.x, randomLoc.y]))
                {
                    adjacent.IncrementNumAdjacentMines();
                }

                locs.Add(randomLoc);
            }
        }
    }

    public void ResetGame()
    {
        numFlagged = 0;
        GameIsActive = false;
        newGame = true;
        runStopwatch = false;
        DurationOfGame = 0;
        WonGame = false;

        UpdateMinesLeftDisplay();

        foreach (Tile t in Minefield)
        {
            t.Reset();
        }

        generateNewMineLocations = false;
    }


    /*
     *  If the game is not activated already, generates mine locations and activates
     *  Reveals clicked Tile unless it is flagged or already revealed
     *  If revealed Tile is a mine, explodes that mine and player loses game
     *  If revealed Tile has no adjacent mines, reveals adjacent Tiles
     *  Checks to see if all safe Tiles are revealed, if so player wins game
     */
    public void RevealTile(Tile t)
    {
        if (!GameIsActive && newGame)
        {
            GameIsActive = true;

            if (generateNewMineLocations)
            {
                GenerateMines(t);
            }
            else
            {
                generateNewMineLocations = true;
            }
        }

        if (t.IsRevealed || t.IsFlagged)
        {
            return;
        }

        t.Reveal();

        if (t.IsMine)
        {
            t.Explode();
            GameOver();
        }
        else
        {
            if (t.NumAdjacentMines == 0)
            {
                RevealAdjacentTiles(t);
            }

            CheckIfAllSafeTilesRevealed();
        }
    }

    /*
     *  Returns a list of all adjacent Tiles
     *      (x, y) = +/- 1
     */
    public List<Tile> FindAdjacentTiles(Tile t)
    {
        List<Tile> adjacentTiles = new List<Tile>();

        int xPos = Mathf.RoundToInt(t.GetComponent<Transform>().position.x / tileWidth);
        int yPos = Mathf.RoundToInt(t.GetComponent<Transform>().position.y / tileHeight);

        // Iterate over xPos -/+ 1
        for (int x = xPos - 1; x <= xPos + 1; x++)
        {
            // Skip iteration if x is outside bounds of minefield
            if (x < 0 || x >= this.cols)
            {
                continue;
            }
            else
            {
                // Iterate over yPos -/+ 1
                for (int y = yPos - 1; y <= yPos + 1; y++)
                {
                    // Skip iteration if y is outside bounds of minefield, or if [x, y] = [xPos, yPos], otherwise add adjacent Tile to list
                    if (y < 0 || y >= this.rows || (x == xPos && y == yPos))
                    {
                        continue;
                    }
                    else
                    {
                        adjacentTiles.Add(Minefield[x, y]);
                    }
                }
            }
        }

        return adjacentTiles;
    }

    /*
     *  Reveals all adjacent tiles
     *      (if an adjacent Tile is flagged or already revealed, nothing will happen)
     */
    public void RevealAdjacentTiles(Tile t)
    {
        foreach (Tile adjacent in FindAdjacentTiles(t))
        {
            RevealTile(adjacent);
        }
    }

    private int GetNumAdjacentTilesFlagged(Tile t)
    {
        int aTf = 0;

        foreach (Tile adjacent in FindAdjacentTiles(t))
        {
            if (adjacent.IsFlagged)
            {
                aTf++;
            }
        }

        return aTf;
    }

    /*
     *  Reveals all adjacent tiles ONLY IF # of adjacent flagged tiles = NumAdjacentMines
     *      (if an adjacent Tile is flagged or already revealed, nothing will happen)
     */
    public void SafeRevealAdjacentTiles(Tile t)
    {
        if (t.NumAdjacentMines == GetNumAdjacentTilesFlagged(t))
        {
            RevealAdjacentTiles(t);
        }
    }

    private void IncrementNumFlagged()
    {
        numFlagged++;
    }

    private void DecrementNumFlagged()
    {
        numFlagged--;
    }

    /*
     *  Toggles the flagged status of a Tile if it is not already revealed
     */
    public void ToggleFlagged(Tile t)
    {
        if (t.IsRevealed)
        {
            return;
        }
 
        if (t.IsFlagged)
        {
            t.Unflag();
            DecrementNumFlagged();
        }
        else
        {
            t.Flag();
            IncrementNumFlagged();
        }

        UpdateMinesLeftDisplay();
    }

    private void UpdateMinesLeftDisplay()
    {
        numMinesLeftText.text = (numMines - numFlagged).ToString("000");
    }

    /*
     *  Aww shucks, lost the game!
     *  Reveals all bombs & incorrectly flagged Tiles, leaves all other Tiles as they were
     *  Halts interaction of all Tiles
     */
    public void GameOver()
    {
        WonGame = false;

        foreach (Tile t in Minefield)
        {
            t.EndGame(false);
        }

        GameIsActive = false;
    }

    /* 
     *  Woohoo won the game!
     *  Defuses and reveals all bombs (all other Tiles are already revealed by definition)
     *  Halts interaction of all Tiles
     */
    private void GameWon()
    {
        WonGame = true;

        foreach (Tile t in Minefield)
        {
            t.EndGame(true);
        }

        numFlagged = numMines;
        UpdateMinesLeftDisplay();

        GameIsActive = false;
    }

    /* 
     *  Determines if all safe Tiles are revealed by counting the number of unrevealed Tiles and comparing to the number of mines
     */
    public void CheckIfAllSafeTilesRevealed()
    {
        int count = 0;
        foreach(Tile t in Minefield)
        {
            if (!t.IsRevealed)
            {
                count++;
            }
        }

        if (count == numMines)
        {
            GameWon();
        }
    }
}
