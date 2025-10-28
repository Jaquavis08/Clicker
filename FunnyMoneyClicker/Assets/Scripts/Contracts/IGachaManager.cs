namespace Game.Contracts
{
    // Contract for gacha managers so other code can depend on an interface
    public interface IGachaManager
    {
        // Called to attempt opening gacha based on a triggering level
        void TryOpenGacha(int level);
    }
}