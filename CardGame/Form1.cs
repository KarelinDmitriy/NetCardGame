using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using CardGame.Net;

namespace CardGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Painter painter;
        private Socket udpSocket;
        private Server server;
        private Client clinet;

        private void Form1_Load(object sender, EventArgs e)
        {
            painter = new Painter(PlayersCard, Board);
        }

        private void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }

        private void LogForServer(string msg)
        {
            if (ServerLog.InvokeRequired)
                ServerLog.BeginInvoke(new Action<string>(x => ServerLog.AppendText(msg + "\n")), msg);
            else ServerLog.AppendText(msg + "\n");
        }

        private void LogForClient(string msg)
        {
            if (ClientLog.InvokeRequired)
                ClientLog.BeginInvoke(new Action<string>(x => ClientLog.AppendText(msg + "\n")), msg);
            else ClientLog.AppendText(msg + "\n");
        }

        private void ServerUdpListener()
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var serverUdpEp = new IPEndPoint(IPAddress.Any, Constants.ServerUdpPort);
            var buffer = new byte[Constants.BufferSize];
            try
            {
                udpSocket.Bind(serverUdpEp);
                while (true)
                {
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any,0);
                    var msgLength = udpSocket.ReceiveFrom(buffer, ref endPoint);
                    udpSocket.SendTo(new byte[1], endPoint);
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(@"Ошибка сети. Поиск серверов недоступен");
            }
            finally
            {
                udpSocket.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint ep = new IPEndPoint(IPAddress.Any, Constants.ClinetUdpPort);
            try
            {
                clientSocket.Bind(ep);
                clientSocket.ReceiveTimeout = 1000;
                EndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.255"), Constants.ServerUdpPort);
                clientSocket.SendTo(new byte[1], endPoint);
                var buffer = new byte[Constants.BufferSize];
                var msgLength = clientSocket.ReceiveFrom(buffer, ref endPoint);
                var ip = (endPoint as IPEndPoint).Address.ToString();
                AddToServerList(ip);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(@"Сервера не найдены");
            }
            finally
            {
                clientSocket.Close();
            }
        }

        private void AddToServerList(string ip)
        {
            if (ServerList.InvokeRequired)
            {
                ServerList.BeginInvoke(
                    new Action<string>(TryAddToList), ip);
            }
            else TryAddToList(ip);
        }

        private void TryAddToList(string x)
        {
            if (!ServerList.Items.Contains(x))
                ServerList.Items.Add(x);
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (ServerList.SelectedItem != null)
            {
                var ip = (string) ServerList.SelectedItem;
                var endPoint = new IPEndPoint(IPAddress.Parse(ip), Constants.ServerGamePort);
                if (clinet != null)
                {
                    ShowMessage("Клиент уже создан");
                }
                else
                {
                    clinet = new Client(PlayerName.Text, painter, ShowMessage, LogForClient);
                    clinet.Start(endPoint);
                }
            }
            else ShowMessage("Не выбран сервер");
        }

        private void PlayersCard_MouseDown(object sender, MouseEventArgs e)
        {
            if (clinet == null) return;
            var x = e.X;
            if (x > 2 && x < 124 )
                clinet.Put(0);
            else if (x > 224 && x < 336)
                clinet.Put(1);
            else if (x > 448 && x < 560)
                clinet.Put(2);
        }

        private void Board_Click(object sender, EventArgs e)
        {
            if (clinet == null) return;
            clinet.Ready();
        }

        private void ServerStart_Click(object sender, EventArgs e)
        {
            server = new Server(ShowMessage, LogForServer);
            server.Run();
            var serverUdpListener = new Thread(ServerUdpListener) { IsBackground = true };
            serverUdpListener.Start();
        }
    }
}