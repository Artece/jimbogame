using System.Collections;
using System.Collections.Generic;
using System;

namespace Models
{

    [Serializable]
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
        public int StartingHP;
        public int StartingPoints;
        public string StartingBranch;
        public int StartingPathPosition;
    }

    // used to deserialize from json
    [Serializable]
    public class ButtonSchema
    {
        public ButtonType ButtonType;
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
    }

    [Serializable]
    public class ScoreAdjustment
    {
        public int HP;
        public int Points;
    }

    [Serializable]
    public class ButtonData
    {
        public ButtonType ButtonType;
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