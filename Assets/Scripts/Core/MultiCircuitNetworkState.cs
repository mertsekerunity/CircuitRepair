namespace CircuitRepair.Core
{
    public enum MultiCircuitNetworkState
    {
        NoSource,
        MixedSources,
        MissingBulb,
        MultipleBulbs,
        MissingGround,
        MultipleGrounds,
        Underpowered,
        Overloaded,
        Valid
    }
}