using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeater : MonoBehaviour
{
    GameController gc;
    AIPlayer ai;

    public int numOfGames;

    private List<float> gamesWonDurations;
    private int gamesPlayed;
    private int gamesWon;
    private int flubGames;
    private bool newGame;
    private bool done;

    // Start is called before the first frame update
    void Start()
    {
        gc = FindObjectOfType<GameController>();
        ai = FindObjectOfType<AIPlayer>();

        gamesPlayed = 0;
        gamesWon = 0;
        flubGames = 0;

        newGame = true;
        done = false;

        gamesWonDurations = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!done)
        {
            if (newGame)
            {
                ai.ToggleRunAI();
                newGame = false;
                gamesPlayed++;

                print(gamesPlayed);
            }
            else
            {
                if (!gc.GameIsActive)
                {
                    if (gc.WonGame)
                    {
                        gamesWon++;
                        gamesWonDurations.Add(gc.DurationOfGame);
                    }
                    else if (ai.LogicFieldSize == gc.MinefieldSize)
                    {
                        gamesPlayed--;
                        flubGames++;
                    }


                    if (gamesPlayed < numOfGames)
                    {
                        gc.NewGame();
                        ai.InitializeAI();

                        newGame = true;
                    }
                    else
                    {
                        PrintStats();
                        done = true;
                    }
                }
            }
        }
    }

    private void PrintStats()
    {
        print("Number of games played: " + gamesPlayed);
        print("Number of games won: " + gamesWon + ", " + ((float) gamesWon / gamesPlayed) * 100 + "%");

        float avgDuration = 0;
        foreach (float time in gamesWonDurations)
        {
            avgDuration += time;
        }

        avgDuration /= gamesWonDurations.Count;

        print("Average duration of won games: " + avgDuration);
        print("Number of flub games: " + flubGames);
    }

}
