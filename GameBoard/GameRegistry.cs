namespace GameBoard;

public static class GameRegistry
{
    public static void RegisterAllGames()
    {
        // register only concrete game boards
        var boardGameTypes = typeof(GameBoard).Assembly.GetTypes()
            .Where(t => typeof(GameBoard).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in boardGameTypes)
        {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
    
    
}
