namespace Core
{
    public enum ServerCommands
    {
        Login = 1,
        Logout,
        Put,
        Ready
    }

    public enum ClientCommands
    {
        NewGame = 1,
        RefreshBoard,
        Win,
        Lose, 
    }
}