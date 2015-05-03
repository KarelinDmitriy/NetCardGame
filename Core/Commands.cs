using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public enum ServerCommands
    {
        Login,
        Logout,
        Put, 
        Ready
    }

    public enum ClientCommands
    {
        NewGame,
        NextStep,
        AddCard,
        Win, 
        Lose,
        NewPlayer,
        PlayerStep
    }
}
