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

        Socket serverSocket;
        List<Socket> clientSocketArray;
        List<Player> activePlayers;
        Queue<Player> XO_Queue; //first element represent X; second O



        bool isServerListening;
        int numberOfClientsWaitingToJoin = 0;
        int numberOfPlayersWaitingToQueue = 0;

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
            }

        }


        public Form1()
        {
            /*To access UI elements in multi-thread level*/
            Control.CheckForIllegalCrossThreadCalls = false;

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            isServerListening = false;

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocketArray = new List<Socket>();
            activePlayers = new List<Player>();
            XO_Queue = new Queue<Player>();

            InitializeComponent();
        }

        private void listen_button_Click(object sender, EventArgs e)
        {
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

        private void WaitPlayerToJoin(Socket clientSocket)
        {
            IPAddress clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;
            string clientIPString = clientIP.ToString();

            Byte[] buffer = new Byte[64];
            while (numberOfClientsWaitingToJoin > 0) {
                try
                {
                    clientSocket.Receive(buffer);
                    string messageFromClient = Encoding.Default.GetString(buffer);
                    messageFromClient = messageFromClient.Trim('\0');
                    string[] token = messageFromClient.Split(':'); //format is "action:payload"
                    
                    if (token[0] == "join")
                    {
                        /* Atomic Operation */
                        mutex.WaitOne();
                        numberOfClientsWaitingToJoin--;
                        mutex.ReleaseMutex();
                        /********************/
                        Thread joinThread = new Thread(() => JoinPlayer(token[1], clientSocket));
                        joinThread.Start();
                        break;
                    }
                    else if (token[0] == "leave")
                    {
                        mutex.WaitOne();
                        numberOfClientsWaitingToJoin--;
                        mutex.ReleaseMutex();
                        clientSocket.Close();
                        clientSocketArray.Remove(clientSocket);
                        log_textbox.AppendText(clientIPString + " has disconnected from server!\n");
                    }
                }
                catch (Exception e)
                {
                    log_textbox.AppendText(clientIPString +" could not join to the server!\n");
                }

            }
            
        }
        private void QueuePlayer(Player player)
        {
            /*if (XO_Queue.Count < 2)
            {
                XO_Queue.Enqueue(player);
            }
            else
            {

            }*/
        }

        void updateSocketStatus()
        {
            foreach(Socket socket in clientSocketArray)
            {
                if (!socket.Connected)
                {
                    clientSocketArray.Remove(socket);
                }
            }
        }

        private void WaitPlayerToQueue(Player player)
        {
            Byte[] buffer = new Byte[64];
            while (numberOfPlayersWaitingToQueue > 0)
            {
                try
                {
                    player.socket.Receive(buffer);
                    string messageFromClient = Encoding.Default.GetString(buffer);
                    messageFromClient = messageFromClient.Trim('\0');
                    string[] token = messageFromClient.Split(':'); //format is "action:payload"

                    if (token[0] == "queue")
                    {
                        /* Atomic Operation */
                        mutex.WaitOne();
                        numberOfPlayersWaitingToQueue--;
                        mutex.ReleaseMutex();
                        /********************/
                        Thread queueThread = new Thread(() => QueuePlayer(player));
                        queueThread.Start();
                        break;
                    }
                    else if (token[0] == "leave")
                    {
                        mutex.WaitOne();
                        numberOfClientsWaitingToJoin--;
                        mutex.ReleaseMutex();
                        player.socket.Close();
                        clientSocketArray.Remove(player.socket);
                        log_textbox.AppendText(player.IPAddress + " has disconnected from server!\n");
                    }
                }
                catch (Exception e)
                {
                    log_textbox.AppendText(player.IPAddress + " could not join to the server!\n");
                }
            }
        }



        private bool checkUsername(String username)
        {
            foreach (Player player in activePlayers)
            {
                if (player.username == username) return false;
            }
            return true;
        }

        private void sendToAllClientSockets(String message)
        {
            foreach(Socket client in clientSocketArray)
            {
                byte[] buffer = Encoding.Default.GetBytes(message);
                client.Send(buffer);
            }
        }

        private void sendOneClient(Socket socket, String message)
        {
            byte[] buffer = Encoding.Default.GetBytes(message);
            socket.Send(buffer);
        }

        private void JoinPlayer(String username, Socket clientSocket)
        {
            if (checkUsername(username))
            {
                Player new_player = new Player(username, clientSocket);
                activePlayers.Add(new_player);
                sendOneClient(clientSocket,"201:You have joined the server!\n");
                sendToAllClientSockets("info:" + username + " has connnected to the server!\n");
                log_textbox.AppendText($"{username} has connected to the server!\n");

                /* Atomic Operation */
                mutex.WaitOne();
                numberOfPlayersWaitingToQueue += 1;
                mutex.ReleaseMutex();
                /********************/
                Thread waitThread = new Thread(() => WaitPlayerToQueue(new_player));
                waitThread.Start();

            }
            else
            {

                byte[] buffer = Encoding.Default.GetBytes("401:The username is not unique!\n");
                clientSocket.Send(buffer);
                String IPAdress = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                log_textbox.AppendText($"{IPAdress} has tried to take a name that is already exist!\n");


            }

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
                    IPAddress clientIP = ((IPEndPoint)newClient.RemoteEndPoint).Address;
                    string clientIPString = clientIP.ToString();
                    updateSocketStatus();
                    if (clientSocketArray.Count == MAX_NUMBER_OF_PLAYERS)
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

                        /* Atomic Operation */
                        mutex.WaitOne();
                        numberOfClientsWaitingToJoin +=1;
                        mutex.ReleaseMutex();
                        /********************/

                        Thread waitThread = new Thread(() => WaitPlayerToJoin(newClient));
                        waitThread.Start();
                    }
                }
                catch{}
            }
        }

        private void disconnect_button_Click(object sender, EventArgs e)
        {
            isServerListening = false;

            disconnect_button.Enabled = false;
            inputBox_port.Enabled = true;
            listen_button.Enabled = true;

            serverSocket.Close();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            log_textbox.AppendText("Server has stopped accepting new connections!\n");
        }


        /* Form Closing event handler*/
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            isServerListening = false;
            serverSocket.Close();
        }
    }
}