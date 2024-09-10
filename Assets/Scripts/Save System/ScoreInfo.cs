using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public struct ScoreInfo
{
    public int scoreAmount;
    public int playerCount;
    public string displayName;
}

public class ScoreSystem
{
    public List<ScoreInfo> GenerateDefaultScores(int numberOfScores = 10, int maxNumberOfPlayers = 2, int minScore = 1000, int maxScore = 10000)
    { 
        var defaultScores = new List<ScoreInfo>();
        for (int scoresIndex = 0; scoresIndex < numberOfScores; scoresIndex++) 
        {
            for (int playerIndex = 1; playerIndex <= maxNumberOfPlayers; playerIndex++)
            {
                //https://stackoverflow.com/questions/50797116/equally-spaced-elements-between-two-given-number
                int step = (maxScore - minScore) / (numberOfScores - 1);
                defaultScores.Add(new ScoreInfo
                {
                    scoreAmount = minScore + step * scoresIndex,
                    playerCount = playerIndex,
                    displayName = RandomString(3)
                });
            }
        }

        return defaultScores;
    }

    //https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[new Random().Next(0, s.Length)]).ToArray());
    }
}
