public class TQLState
{
    public enum TotalHealth { VeryLow, Low, Medium, High }
    public enum Mana { Low, Medium, High }
    public enum Time { Low, Medium, High }
    public enum Position { TopLeft, TopRight, BottomLeft, BottomRight }

    public TotalHealth CurrentTotalHealth { get; set; }
    public Mana CurrentMana { get; set; }
    public Time CurrentTime { get; set; }
    public Position CurrentPosition { get; set; }

    public TQLState(TotalHealth health, Mana mana, Time time, Position position)
    {
        CurrentTotalHealth = health;
        CurrentMana = mana;
        CurrentTime = time;
        CurrentPosition = position;
    }
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj is not TQLState) return false;

        TQLState other = (TQLState)obj;

        // Compare all properties for equality
        return this.CurrentTotalHealth == other.CurrentTotalHealth &&
               this.CurrentMana == other.CurrentMana &&
               this.CurrentTime == other.CurrentTime &&
               this.CurrentPosition == other.CurrentPosition;
    }

    // Override GetHashCode to generate a unique hash code for each TQLState
    public override int GetHashCode()
    {
        return System.HashCode.Combine(CurrentTotalHealth, CurrentMana, CurrentTime, CurrentPosition);
    }
}
