[System.Serializable]
public class QEntry
{
    public TQLState state;
    public int actionID;
    public float qValue;

    public QEntry(TQLState state, int actionID, float qValue)
    {
        this.state = state;
        this.actionID = actionID;
        this.qValue = qValue;
    }
}
