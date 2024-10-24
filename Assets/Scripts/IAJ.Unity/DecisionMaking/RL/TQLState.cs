public class TQLState
{
    public enum TotalHealth { VeryLow, Low, Medium, High }
    public enum Mana { Low, Medium, High }
    public enum Progress { Low, Medium, High }

    public TotalHealth CurrentTotalHealth { get; set; }
    public Mana CurrentMana { get; set; }
    public Progress CurrentProgress { get; set; }
    public int CurrentLevel { get; set; }

    public TQLState(TotalHealth health, Mana mana, Progress progress, int level)
    {
        CurrentTotalHealth = health;
        CurrentMana = mana;
        CurrentProgress = progress;
        CurrentLevel = level;
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj is not TQLState) return false;

        TQLState other = (TQLState)obj;

        // Compare all properties for equality
        return this.CurrentTotalHealth == other.CurrentTotalHealth &&
               this.CurrentMana == other.CurrentMana &&
               this.CurrentProgress == other.CurrentProgress &&
               this.CurrentLevel == other.CurrentLevel;
    }

    // Override GetHashCode to generate a unique hash code for each TQLState
    public override int GetHashCode()
    {
        return System.HashCode.Combine(CurrentTotalHealth, CurrentMana, CurrentProgress, CurrentLevel);
    }
}
