using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core;
using Core.CardClasses;

namespace CardGame.Net
{
    internal class PlayerInfo
    {
        public Socket Socket { get; set; }
        public Player Player { get; set; }
        public Queue<byte[]> Commands { get; set; }
        public Stack<Card> CardInGame { get; set; }
        public bool IsLose { get; set; }
        public bool CanMove { get; set; }
        public bool InDispute { get; set; }
        public bool IsWinInStep { get; set; }

        public PlayerInfo(Socket socket)
        {
            Socket = socket;
            Player = new Player();
            Commands = new Queue<byte[]>();
            CardInGame = new Stack<Card>();
            IsLose = false;
            CanMove = true;
            InDispute = false;
            IsWinInStep = false;
        }
    }

    internal class Server
    {
        private readonly Action<string> showMessage;
        private const int BufferSize = 1024;
        private Socket socket;
        private readonly IDictionary<string, PlayerInfo> players = new Dictionary<string, PlayerInfo>();
        private readonly List<Card> prize = new List<Card>(); 

        private readonly object locker = new object();
        private readonly Action<string> log;

        public Server(Action<string> showMessage, Action<string> log)
        {
            this.showMessage = showMessage;
            this.log = log;
        }

        public void Reset()
        {
            foreach (var sock in players.Values.Select(x => x.Socket))
            {
                sock.Disconnect(false);
            }
            players.Clear();
            prize.Clear();
        }

        public void Run()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, Constants.ServerGamePort));
                socket.Listen(100);
                log("Сервер запущен");
                var thread = new Thread(AcceptThread) { IsBackground = true };
                thread.Start();
            }
            catch (SocketException ex)
            {
                showMessage("Не удалось создать сервер");
            }
        }

        private void AcceptThread()
        {
            try
            {
                log("Запущен прием сокетов");
                while (true)
                {
                    var clinet = socket.Accept();
                    log("Принято соеденение");
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
            var name = string.Empty;
            try
            {
                //ожидаем login
                var buffer = new byte[BufferSize];
                var msgLength = sock.Receive(buffer);
                var сmd = (ServerCommands)buffer[0];
                if (сmd != ServerCommands.Login)
                {
                    SayDisconect(sock);
                    return;
                }
                name = Encoding.UTF8.GetString(buffer, 1, msgLength - 1);
                log("Получена команда LogIn от " + name);
                lock (locker)
                {
                    if (players.Count == 3)
                    {
                        SayDisconect(sock);
                        return;
                    }
                    if (players.ContainsKey(name))
                    {
                        log("Такое имя уже используется");
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
            }
            catch (SocketException e)
            { }
            finally
            {
                lock (locker)
                {
                    players.Remove(name);
                }
                log("Игрок " + name + "отключен");
                sock.Close();
            }
        }

        private void Listen(object obj)
        {
            var name = (string)obj;
            log("Запущен сборщик комадн от " + name);
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
                        case ServerCommands.Login:
                            break;
                        case ServerCommands.Logout:
                            break;
                        case ServerCommands.Put:
                            Put(command, name);
                            break;
                        case ServerCommands.Ready:
                            Ready(command, name);
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
            log("Получет PutRequest от " + name);
            if (!players[name].CanMove) return;
            var card = players[name].Player.GetCard(arr[1]);
            if (card == null) return;
            players[name].CardInGame.Push(card);
            players[name].Player.IsReady = false;
            players[name].CanMove = false;
            lock (locker)
            {
                Check();
            }
            var answer = BuildRefreshCommand();
            SendAll(answer);
        }

        private void Check()
        {
            log("Проверка состояния после хода");
            //если никто не может ходить, вычисляем состояние
            if (!AnyCanMove())
            {
                //проверяем, находимся ли мы с стадии спора
                if (players.Any(x => x.Value.InDispute))
                {
                    var duplicatePlayers = players.Where(x => x.Value.InDispute).Select(x => x.Key);
                    var winValue = players.Where(x => duplicatePlayers.Contains(x.Key))
                        .Max(x => x.Value.CardInGame.Peek().Value);
                    var wins = players
                        .Where(x => duplicatePlayers.Contains(x.Key) 
                            && x.Value.CardInGame.Peek().Value == winValue)
                        .Select(x => x.Key).ToArray();
                    foreach (var pl in  players.Where(x=>x.Value.InDispute).Select(x=>x.Value))
                    {
                        pl.InDispute = false;
                    }
                    DoSomethingWithWinners(wins);
                } //если никто не спорит
                else
                {
                    var winValue = players.Where(x=>!x.Value.IsLose)
                        .Max(x => x.Value.CardInGame.Peek().Value);
                    var wins = players.Where(x=>!x.Value.IsLose)
                        .Where(x => x.Value.CardInGame.Peek().Value == winValue)
                        .Select(x => x.Key)
                        .ToArray();
                    DoSomethingWithWinners(wins);
                }
            }
        }

        private void DoSomethingWithWinners(string[] wins)
        {
            var localPrize = players.SelectMany(x => x.Value.CardInGame);
            prize.AddRange(localPrize);
            if (wins.Count() > 1)
            {
                log("Зафиксирован спор");
                foreach (var player in players.Where(x => wins.Contains(x.Key)))
                {
                    player.Value.InDispute = true;
                }
            }
            else
            {
                log("Победа в шаге: " + wins[0]);
                foreach (var card in prize)
                {
                    players[wins[0]].Player.AddCard(card);
                }
                prize.Clear();
                players[wins[0]].IsWinInStep = true;
            }
            var losers = players
                .Where(x => x.Value.Player.IsLose())
                .Select(x => x.Key)
                .ToArray();
            foreach (var loser in losers.Where(loser => !players[loser].IsLose))
            {
                //проиграл только сейчас
                players[loser].IsLose = true;
                var msg = BuildLoseMessage(loser);
                log("Следующий игрок проиграл: " + loser);
                SendAll(msg);
            }
            if (players.Count(x => x.Value.IsLose) == 2)
            {
                var winnerName = players.Select(x => x.Key).Except(losers).Single();
                var msg = BuildWinMessage(winnerName);
                log("Победитель игры: " + winnerName);
                SendAll(msg);
            }
        }

        private bool AnyCanMove()
        {
            return players.Any(x => x.Value.CanMove);
        }

        private void Ready(byte[] command, string name)
        {
            //нужно ли проверять готовность или сразу ставить true
            log("Получет ReadyRequest от " + name);
            if (!players[name].CanMove)
            {
                players[name].Player.IsReady = true;
                lock (locker)
                {
                    NextStepChecker();
                } //нужна ли в этом месте синхронизация?
                var request = BuildRefreshCommand();
                SendAll(request);
            }
        }

        private void NextStepChecker()
        {
            log("Определяется победитель в текущем шаге");
            if (AllReady())
            {
                foreach (var playerInfo in players.Select(x => x.Value))
                {
                    playerInfo.CardInGame.Clear();
                }
                //если не в стадии спора, то очищаем стеки, иначе нет
                var inDispute = players.Any(x => x.Value.InDispute);
                foreach (var playerInfo in players.Where(x=>!x.Value.IsLose).Select(x=>x.Value))
                {
                    playerInfo.IsWinInStep = false;
                    if (inDispute)
                    {
                        if (playerInfo.InDispute)
                        {
                            playerInfo.CanMove = true;
                            playerInfo.Player.IsReady = false;
                        }
                    }
                    else
                    {
                        playerInfo.CardInGame.Clear();
                        playerInfo.CanMove = true;
                        playerInfo.Player.IsReady = false;
                    }
                }
                var answer = BuildRefreshCommand();
                SendAll(answer);
            }
        }

        private bool AllReady()
        {
            return players.Where(x => !x.Value.IsLose).All(x => x.Value.Player.IsReady);
        }

        private void SayDisconect(Socket socket)
        {
        }

        private void StartGame()
        {
            log("Запускаем игру");
            CreateCards();
            log("Карты перетасованы. Раздача");
            SendAll(BuildStartMessage());
        }

        private void CreateCards()
        {
            var cards = new List<Card>();
            for (var i = 1; i < 5; i++)
            {
                for (var j = 6; j < 15; j++)
                    cards.Add(new Card((Suit)i, j));
            }
            var rnd = new Random();
            for (var k = 0; k < 1000; k++)
            {
                var i = rnd.Next() % 36;
                var j = rnd.Next() % 36;
                var card = cards[i];
                cards[i] = cards[j];
                cards[j] = card;
            }
            var s = 0;
            foreach (var player in players.Select(x => x.Value.Player))
            {
                for (var i = 0; i < 12; i++, s++)
                    player.AddCard(cards[s]);
            }
        }

        private void SendAll(byte[] info, string exception = null)
        {
            lock (locker)
            {
                try
                {
                    log("Отправка общего сообщения");
                    var sockets = players.Where(p => p.Key != exception).Select(x => x.Value.Socket);
                    foreach (var s in sockets)
                        s.Send(info);
                }
                catch (SocketException ex)
                {
                    Reset();
                }

            }
        }

        private byte[] BuildRefreshCommand()
        {
            //будем отправлять всем одну и ту же информацию,
            //клиенты должны сами понять, какую инфу отображать, а какую нет 
            var commId = new[] { (byte)ClientCommands.RefreshBoard };
            return commId.Concat(players.PackGameToPlayers()).ToArray();
        }

        private byte[] BuildWinMessage(string name)
        {
            var res = new[] { (byte)ClientCommands.Win };
            var nameM = Encoding.UTF8.GetBytes(name);
            return res.Concat(nameM).ToArray();
        }

        private byte[] BuildLoseMessage(string name)
        {
            var res = new[] { (byte)ClientCommands.Lose };
            var nameM = Encoding.UTF8.GetBytes(name);
            return res.Concat(nameM).ToArray();
        }

        private byte[] BuildStartMessage()
        {
            var commId = new[] { (byte)ClientCommands.NewGame };
            return commId.Concat(players.PackGameToPlayers()).ToArray();
        }
    }
}