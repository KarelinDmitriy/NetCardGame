using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Core;
using Core.CardClasses;

namespace CardGame.Net
{
    public class PlayerData
    {
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
        public Card CardInGame { get; set; }
        public bool IsReady { get; set; }
    }

    public class Client
    {
        private Socket serverSocket;
        private readonly string name;
        private List<PlayerData> players;
        private readonly Painter painter;
        private readonly Action<string> showMessage;
        private readonly object locker = new object();
        private readonly Queue<byte[]> commands = new Queue<byte[]>();
        private bool isLose = true;
        private Action<string> log;

        private const int BufferSize = 1024;

        public Client(string name, Painter painter, Action<string> showMessage, Action<string> log)
        {
            this.name = name;
            this.painter = painter;
            this.showMessage = showMessage;
            this.log = log;
            players = new List<PlayerData>();
        }

        public void Start(IPEndPoint endPoint)
        {
            try
            {
                log("Попытка подключения к серверу");
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(endPoint);
                log("Подключение выполнено");
                //сначала логинимся
                var request = new[] {(byte) ServerCommands.Login};
                var bName = Encoding.UTF8.GetBytes(name);
                request = request.Concat(bName).ToArray();
                log("Отправлена команда LogIn");
                serverSocket.Send(request);
                var thread = new Thread(CommandsReader) {IsBackground = true};
                thread.Start();
                var thread2 = new Thread(Listen) {IsBackground = true};
                thread2.Start();
            }
            catch (Exception)
            {
                showMessage("Не удалось подключиться к серверу");
            }
        }

        public void Ready()
        {
            if (isLose) return;
            try
            {
                log("Отправка ReadyRequest");
                var request = new[] { (byte)ServerCommands.Ready };
                serverSocket.Send(request);
            }
            catch (Exception)
            {
                showMessage("Ошибка подключения");
            }
        }

        public void Put(int i)
        {
            if (isLose) return;
            var data = players.First(x => x.Name == name);
            if (data.Cards.Count < i)
                return;
            log("Отправка PutRequest");
            var requers = new[] { (byte)ServerCommands.Put, (byte)i };
            serverSocket.Send(requers);
        }

        private void CommandsReader()
        {
            try
            {
                var buffer = new byte[BufferSize];
                log("Запущен CommandReader");
                while (true)
                {
                    var msgLength = serverSocket.Receive(buffer);
                    log("Получена команда с сервера");
                    lock (commands)
                    {
                        commands.Enqueue(buffer.Take(msgLength).ToArray());
                    }
                }
            }
            catch (SocketException ex)
            {
                //todo подумать, что тут сделать
            }
            finally
            {
                serverSocket.Close();
            }
        }

        private void Listen()
        {
            while (commands != null)
            {
                byte[] command = null;
                lock (commands)
                {
                    if (commands.Any())
                        command = commands.Dequeue();
                }
                if (command == null)
                    continue;
                try
                {
                    var type = (ClientCommands)command[0];
                    switch (type)
                    {
                        case ClientCommands.Lose:
                            Lose(command);
                            break;
                        case ClientCommands.RefreshBoard:
                            RefreshBoard(command);
                            break;
                        case ClientCommands.NewGame:
                            NewGame(command);
                            break;
                        case ClientCommands.Win:
                            Win(command);
                            break;
                    }
                }
                catch (InvalidCastException)
                {
                    showMessage("Получена не верная команда с сервера.\nВозможна попытка взлома.\nСоеденение прервано");
                    serverSocket.Disconnect(false); //todo проверить, как работает
                    return;
                }
            }
            log("CommandReady остановлен");
        }

        private void Lose(byte[] command)
        {
            log("Получена сообщение о проигрыше ");
            var loserName = Encoding.UTF8.GetString(command, 1, command.Length - 1);
            showMessage(name == loserName ? "Вы проиграли" : String.Format("{0} проиграл", loserName));
        }

        private void RefreshBoard(byte[] command)
        {
            log("Получены команда обновления доски");
            players = command.UnpackPlayerInfo();
            painter.SetData(players, name);
        }

        private void NewGame(byte[] command)
        {
            log("Получена команда о начале новой игры");
            players = command.UnpackPlayerInfo();
            isLose = false;
            painter.SetData(players, name);
        }

        private void Win(byte[] command)
        {
            log("Сообщение о победе");
            var winName = Encoding.UTF8.GetString(command, 1, command.Length - 1);
            showMessage(name == winName ? "Вы победили" : String.Format("{0} победил", winName));
        }
    }
}