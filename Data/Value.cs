namespace SimpleChecks.Data;

public struct Value
{
    public long ValueVal { get; set; }
    public List<long> RandomVals { get; set; }

    public Value(long val)
    {
        ValueVal = val;
        RandomVals = Enumerable.Range(0, 500).Select(x => x + val * 100).ToList();
    }

    public override string ToString() => ValueVal.ToString();
}