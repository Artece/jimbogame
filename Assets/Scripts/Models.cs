using System.Collections;
using System.Collections.Generic;
using System;

namespace Models
{
    // you can if you want, but I'm not introducing an entirely new library just to parse a string into an enum, I prefer to KISS and just do a strcmp later
    public enum ButtonType
    {
        Default,
        End // instant game over
    }

    // used to deserialize from json
    [Serializable]
    public class ScenarioSchema
    {
        public string Title;
        public string Subtitle;
        public string Description;
        public ScenarioSettings Settings;
    }

    [Serializable]
    public class ScenarioSettings
    {
        public StoryPath StartPath;
        public ScoreAdjustment StartScore;
        //public int StartingHP;
        //public int StartingPoints;
        //public string StartingBranch;
        //public int StartingPathPosition;
    }

    // used to deserialize from json
    [Serializable]
    public class ButtonSchema
    {
        public string ButtonType;
        public string Label;
        public string VideoFilename;
        public StoryPath Path;
        public ScoreAdjustment ScoreAdjustment;
        public List<Ending> Endings;
        public string EndScreenMessage;
    }

    [Serializable]
    public class Ending
    {
        public List<int> WhenPointsAreBetween;
        public string VideoFilename;
    }

    [Serializable]
    public class StoryPath
    {
        public string Branch;
        public int StartPosition;
        public override string ToString() => Branch + StartPosition + "/";
    }

    [Serializable]
    public class ScoreAdjustment
    {
        public int HP;
        public int Points;
        public ScoreAdjustment(int hp, int points)
        {
            HP = hp;
            Points = points;
        }
        public static ScoreAdjustment operator +(ScoreAdjustment a, ScoreAdjustment b) => new ScoreAdjustment(a.HP + b.HP, a.Points + b.Points);
    }

    [Serializable]
    public class ButtonData
    {
        public string ButtonType;
        public StoryPath StoryPath;
        public string VideoFileLocation;
        public ScoreAdjustment ScoreAdjustment;
        public List<Ending> Endings;
        public string EndScreenMessage;
    }

    [Serializable]
    public class GameSettings
    {
        // not used yet
    }
}