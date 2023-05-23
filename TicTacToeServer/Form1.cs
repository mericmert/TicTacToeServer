using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.DirectoryServices;
using System.Threading;
using System;
using static TicTacToeServer.Form1;

namespace TicTacToeServer
{
    public partial class Form1 : Form
    {
        const int MAX_NUMBER_OF_PLAYERS = 2;

        static Mutex mutex = new Mutex();


        bool isServerListening;

        Socket serverSocket;
        List<Socket> clientSocketArray;
        List<Player> activePlayers;


        public struct Player
        {
            public string username;
            public int gamesPlayed;
            public int win;
            public int draw;
            public int loss;
            public int points;
            public Socket socket;
            public string IPAddress;
            public char? current_side;
            public Player(string name, Socket socket)
            {
                this.username = name;
                this.gamesPlayed = 0;
                this.win = 0;
                this.draw = 0;
                this.loss = 0;
                this.points = 0;
                this.socket = socket;
                this.IPAddress = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                this.current_side = null;
            }

        }


        public Form1()
        {
            /*To access UI elements in multi-thread level*/
            Control.CheckForIllegalCrossThreadCalls = false;

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            isServerListening = false;

            clientSocketArray = new List<Socket>();
            activePlayers = new List<Player>();

            InitializeComponent();
        }


        private string getClientIPAddress(Socket server)
        {
            IPAddress clientIP = ((IPEndPoint)server.RemoteEndPoint).Address;
            string clientIPString = clientIP.ToString();
            return clientIPString;
        }


        private void Accept()
        {
            while (isServerListening)
            {
                try
                {
                    /*Server is waiting to accept new incoming client sockets*/
                    Socket newClient = serverSocket.Accept();

                    /*Getting IP adress of connected client socket*/
                    String clientIPString = getClientIPAddress(newClient);
                    if (clientSocketArray.Count > MAX_NUMBER_OF_PLAYERS)
                    {
                        log_textbox.AppendText(clientIPString + " has tried to connect server but server is at maximum capacity!\n");

                        byte[] buffer = Encoding.Default.GetBytes("400:Server is at maximum capacity!");
                        newClient.Send(buffer);
                        newClient.Close();
                    }
                    else
                    {
                        /*Adding new client socket into client socket queue*/
                        clientSocketArray.Add(newClient);
                        log_textbox.AppendText(clientIPString + " has connected to the server!\n");
                        byte[] buffer = Encoding.Default.GetBytes("200:Connection is OK!");
                        newClient.Send(buffer);


                        Thread controllerThread = new Thread(() => ClientController(newClient));
                        controllerThread.Start();
                    }
                }
                catch { }
            }
        }

        bool checkUserName(string username)
        {
            foreach (Player player in activePlayers)
            {
                if (player.username == username) return false;
            }
            return true;
        }

        private void sendMessageToClientSocket(Socket client, string message)
        {
            try
            {
                Byte[] buffer = Encoding.Default.GetBytes(message);
                client.Send(buffer);
            }
            catch (Exception e) {
                log_textbox.AppendText("Message couldn't be sent to the client!\n");
            };
            
        }

        private void sendMessageToAllPlayers(string message)
        {
            foreach (Player player in activePlayers)
            {
                sendMessageToClientSocket(player.socket, message);
            }
        }


        void handleJoin(Socket clientSocket, string username)
        {
            if (checkUserName(username))
            {
                Player player = new Player(username, clientSocket);
                activePlayers.Add(player);
                sendMessageToClientSocket(clientSocket, "201:You are succesfully joined the game!\n");
                sendMessageToAllPlayers("info:" + username + " has joined the game!\n");
                log_textbox.AppendText(username + " has joined the game!\n");
            }
            else
            {
                sendMessageToClientSocket(clientSocket, "401:Username is already taken!\n");
                log_textbox.AppendText(getClientIPAddress(clientSocket) + " has tried to take a username that is already exist!\n");
            }
        }

        void handleLeave(Player player)
        {
            sendMessageToClientSocket(player.socket, "info:" + "You've left the game!\n");

            player.socket.Close();
            player.socket.Dispose();
            activePlayers.Remove(player);

            sendMessageToAllPlayers("info:" + player.username + " has left the game!\n");
            log_textbox.AppendText(player.username + " has left the game!\n");
        }

        void handleQueue(Player player)
        {

        }

        Player? findPlayerBySocket(Socket clientSocket)
        {
            foreach(Player player in activePlayers)
            {
                if (player.socket == clientSocket) return player;
            }
            return null;
        }


        void ClientController(Socket clientSocket) //listener of each client socket
        {
            while(clientSocket.Connected)
            {
                Byte[] buffer = new Byte[64];
                clientSocket.Receive(buffer);

                string token = Encoding.Default.GetString(buffer).Trim('\0');
                string[] request = token.Split(":");
                string action = request[0];
                try
                {
                    if (action == "join")
                    {
                        string username = request[1];
                        handleJoin(clientSocket, username);
                    }
                    else if (action == "queue")
                    {
                        Player? player = findPlayerBySocket(clientSocket);
                        if (player != null) handleQueue((Player)player);
                    }

                    else if (action == "leave")
                    {
                        Player? player = findPlayerBySocket(clientSocket);
                        if (player != null) handleLeave((Player)player);

                    }
                    else
                    {
                        log_textbox.AppendText($"Invalid request ({action}) has been sent from {getClientIPAddress(clientSocket)}");
                    }
                }
                catch
                {
                    log_textbox.AppendText($"Error on {getClientIPAddress(clientSocket)}'s request : the request was {action}\n");
                }
                
            }
        }

        private void listen_button_Click(object sender, EventArgs e) // Client Socket connection is established
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int portNumber;
            //Parsing Port Number
            if (Int32.TryParse(inputBox_port.Text, out portNumber))
            {
                try
                {
                    /*Binding Endpoint to the server socket*/ 
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, portNumber);
                    serverSocket.Bind(endpoint);
                    serverSocket.Listen(MAX_NUMBER_OF_PLAYERS);

                    isServerListening = true;

                    disconnect_button.Enabled = true;
                    listen_button.Enabled = false;
                    inputBox_port.Enabled = false;

                    log_textbox.AppendText("Server has started to listen the port: " + portNumber + "\n");


                    /*Server Socket starts to listen client sockets in the endpoint provided above*/
                    Thread acceptThread = new Thread(Accept);
                    acceptThread.Start();

                }
                catch
                {
                    log_textbox.AppendText("Server cannot listen the endpoint provided!\n");
                }
            }
            else
            {
                log_textbox.AppendText("Please enter a valid port number!\n");
            }
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            isServerListening = false;

            disconnect_button.Enabled = false;
            inputBox_port.Enabled = true;
            listen_button.Enabled = true;
            serverSocket.Close();
            serverSocket.Dispose();

            log_textbox.AppendText("Server has stopped accepting new connections!\n");
        }


        /* Form Closing event handler*/
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isServerListening = false;
            serverSocket.Close();
            Environment.Exit(0);
        }
    }
}