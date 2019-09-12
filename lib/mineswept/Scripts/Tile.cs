using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public Sprite hiddenSprite;
    public Sprite flaggedSprite;
    public Sprite incorrectlyFlaggedSprite;
    public Sprite[] numberSprites;
    public Sprite mineSprite;
    public Sprite mineExplodedSprite;
    public Sprite mineDefusedSprite;

    private Image displayedImg;
    private Sprite revealedSprite;

    public bool IsRevealed { get; private set; }
    public bool IsFlagged { get; private set; }
    private bool isMine;
    public bool IsMine
    {
        get
        {
            if (IsRevealed) { return isMine; }
            else { return false; }
        }
    }
    private int numAdjacentMines;
    public int NumAdjacentMines
    {
        get
        {
            if (IsRevealed) { return numAdjacentMines; }
            else { return -1; }
        }
    }

    public void Awake()
    {
        GetComponent<Button>().onClick.AddListener(RevealTileBtn);
        GetComponent<RightClick>().onClick.AddListener(ToggleFlaggedBtn);

        displayedImg = GetComponent<Image>();

        numAdjacentMines = 0;
        revealedSprite = numberSprites[numAdjacentMines];

        IsRevealed = false;
        IsFlagged = false;

        isMine = false;
    }

    public void RevealTileBtn()
    {
        if (GetComponent<Button>().interactable)
        {
            if (IsRevealed)
            {
                GetComponentInParent<GameController>().SafeRevealAdjacentTiles(this);
            }
            else
            {
                GetComponentInParent<GameController>().RevealTile(this);
            }
        }
    }

    public void ToggleFlaggedBtn()
    {
        if (GetComponent<RightClick>().interactable)
        {
            GetComponentInParent<GameController>().ToggleFlagged(this);
        }
    }

    public void Reveal()
    {
        IsRevealed = true;
        displayedImg.sprite = revealedSprite;
    }

    public void Reset()
    {
        IsRevealed = false;
        IsFlagged = false;

        if (isMine)
        {
            revealedSprite = mineSprite;
        }
        else
        {
            revealedSprite = numberSprites[numAdjacentMines];
        }

        displayedImg.sprite = hiddenSprite;

        EnableInteraction();
    }

    public void Flag()
    {
        IsFlagged = true;
        displayedImg.sprite = flaggedSprite;
    }

    public void Unflag()
    {
        IsFlagged = false;
        displayedImg.sprite = hiddenSprite;
    }

    public void MakeMine()
    {
        isMine = true;
        revealedSprite = mineSprite;

        numAdjacentMines = 0;
    }

    public void IncrementNumAdjacentMines()
    {
        if (!isMine)
        {
            numAdjacentMines++;
            revealedSprite = numberSprites[numAdjacentMines];
        }
    }

    public void Explode()
    {
        revealedSprite = mineExplodedSprite;
    }

    /*
     *  If this Tile is a mine and game is won, defuse and reveal
     *  else if this Tile is flagged reveal that it was incorrectly flagged
     *      (does not reveal a non-mine number Tile)
     */
    public void EndGame(bool gameWon)
    {
        DisableInteraction();

        if (isMine)
        {
            if (gameWon)
            {
                revealedSprite = mineDefusedSprite;
            }

            Reveal();
        }
        else
        {
            if (IsFlagged)
            {
                revealedSprite = incorrectlyFlaggedSprite;
                Reveal();
            }
        }
    }

    private void DisableInteraction()
    {
        GetComponent<Button>().interactable = false;
        GetComponent<RightClick>().interactable = false;
    }

    private void EnableInteraction()
    {
        GetComponent<Button>().interactable = true;
        GetComponent<RightClick>().interactable = true;
    }
}
