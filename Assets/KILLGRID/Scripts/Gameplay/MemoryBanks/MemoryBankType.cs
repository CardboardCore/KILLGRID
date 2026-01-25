namespace KILLGRID.Gameplay.MemoryBanks
{
    public enum MemoryBankType
    {
        CoreHQ,
        Generator,
        Fabricator
    }

    // Extension to get random MemoryBankType but excluding CoreHQ
    public static class MemoryBankTypeExtensions
    {
        private static readonly MemoryBankType[] nonCoreTypes = new[]
        {
            MemoryBankType.Generator, MemoryBankType.Fabricator
        };

        public static MemoryBankType GetRandomNonCoreType()
        {
            int index = UnityEngine.Random.Range(0, nonCoreTypes.Length);

            return nonCoreTypes[index];
        }
    }
}
