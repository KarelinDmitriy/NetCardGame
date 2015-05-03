using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using Core.CardClasses;
using Core;

namespace CardGame.Net
{
    class PlayerInfo
    {
        public Socket Socket { get; set; }
        public Player Player { get; set; }
        public Queue<byte[]> Commands { get; set; }
        public Stack<Card> CardInGame { get; set; }

        public PlayerInfo(Socket socket)
        {
            this.Socket = socket;
            Player = new Player();
            Commands = new Queue<byte[]>();
            CardInGame = new Stack<Card>();
        }
    }

    class Server
    {
        const int BufferSize = 1024;
        private Socket socket;
        private IDictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();

        private object locker = new object();
        //threads
        private Thread Accepter;
        private IDictionary<string, Thread> clientsThreads = new Dictionary<string, Thread>();

        public Server()
        {

        }

        public void Run()
        {

        }

        private void AcceptThread()
        {
            try
            {
                while (true)
                {
                    var clinet = socket.Accept();
                    var clThread = new Thread(ClinetThread) { IsBackground = true };
                    clThread.Start(clinet);
                }
            }
            finally
            {
                socket.Close();
            }
        }

        private void ClinetThread(object obj)
        {
            var sock = (Socket)obj;
            //ожидаем login
            var buffer = new byte[BufferSize];
            var msgLength = sock.Receive(buffer);
            var name = string.Empty;
            try
            {
                ServerCommands suit = (ServerCommands)buffer[0];
                if (suit != ServerCommands.Login)
                {
                    SayDisconect(sock);
                    return;
                }
                name = Encoding.UTF8.GetString(buffer, 1, msgLength - 1);
                lock (locker)
                {
                    if (players.Count == 3)
                    {
                        SayDisconect(sock);
                        return;
                    }
                    if (players.ContainsKey(name))
                    {
                        SayDisconect(sock);
                        return;
                    }
                    players.Add(name, new PlayerInfo(sock));
                    if (players.Count == 3)
                        StartGame();
                    var listener = new Thread(Listen) { IsBackground = true };
                    listener.Start(name);
                } //
                while (true)
                {
                    msgLength = sock.Receive(buffer);
                    lock (players[name])
                    {
                        players[name].Commands.Enqueue(buffer.Take(msgLength).ToArray());
                    }
                }
            }
            catch (InvalidCastException e)
            {
                SayDisconect(sock);
                return;
            }
            finally
            {
                lock (locker)
                {
                    players.Remove(name);
                }
                sock.Close();
            }
        }

        private void Listen(object obj)
        {
            var name = (string)obj;
            var info = players[name];
            var commands = players[name].Commands;
            while (players.ContainsKey(name)) 
            {
                byte[] command = null;
                lock (info)
                {
                    if (commands.Any())
                        command = commands.Dequeue();
                }
                if (command == null)
                    continue;
                try
                {
                    var type = (ServerCommands)command[0];
                    switch (type)
                    {
                        case ServerCommands.Login: break;
                        case ServerCommands.Logout :
                            break;
                        case ServerCommands.Put: Put(command, name);
                            break;
                        case ServerCommands.Ready: Ready(command, name);
                            break;
                    }
                }
                catch (InvalidCastException)
                {
                    SayDisconect(info.Socket);
                    return;
                }
            }
        }

        private void Put(byte[] arr, string name)
        {
            var card = players[name].Player.GetCard(arr[1]);
            players[name].CardInGame.Push(card);
            players[name].Player.IsReady = false;
            var answer = new byte[3];
            answer[0] = (byte)ClientCommands.PlayerStep;
            var packedCard = card.Pack();
            answer[1] = packedCard[0];
            answer[2] = packedCard[1];
            var bName = Encoding.UTF8.GetBytes(name);
            answer = answer.Concat(bName).ToArray();
            SendAll(answer, name);
        }

        private void Ready(byte[] command, string name)
        {
            players[name].Player.IsReady = true;
            if (players.All(x => x.Value.Player.IsReady))
            {
                FindWinner();
            }
        }

        private void FindWinner()
        {
            var winnerPrize = new Stack<Card>();
            var winValue = players.Max(x => x.Value.CardInGame.Peek().Value);
            var wins = players.Where(x => x.Value.CardInGame.Peek().Value == winValue).Select(x => x.Key).ToArray();
        }

        private void SayDisconect(Socket socket)
        {

        }

        private void StartGame()
        {

        }

        private void SendAll(byte[] info, string exception = null)
        {
            lock (locker)
            {
                var sockets = players.Where(p => p.Key != exception).Select(x => x.Value.Socket);
                foreach (var s in sockets)
                    s.Send(info);
            }
        }
    }
}
