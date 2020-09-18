namespace Unified
{
    /// <summary>
    /// Partitioning size tiers.
    /// </summary>
    public enum Tier
    {
        /// <summary>
        /// Basic tier for 16 partitions 0-F.
        /// </summary>
        Basic = 1,
        /// <summary>
        /// Standard tier for 512 partitions 00-FV.
        /// </summary>
        Standard = 2,
        /// <summary>
        /// Premium tier for 16384 partitions 000-FVV.
        /// </summary>
        Premium = 3
    }
}
