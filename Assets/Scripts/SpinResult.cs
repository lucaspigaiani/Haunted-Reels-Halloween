public class SpinResult
{
    public SymbolSystem[,] Grid;

    public SpinResult(int reels, int rows)
    {
        Grid = new SymbolSystem[reels, rows];
    }
}