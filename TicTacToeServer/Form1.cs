using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.DirectoryServices;
using System.Threading;
using System;
using System.ComponentModel;
using static TicTacToeServer.Form1;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace TicTacToeServer
{
    public partial class Form1 : Form
    {
        const int MAX_NUMBER_OF_PLAYERS = 3;

        static Mutex mutex = new Mutex();


        bool isServerListening;
        bool isGameNotFinished;
        bool isXTurn;
        bool gameIsPending;


        Socket serverSocket;
        List<Socket> clientSocketArray;
        List<Player> activePlayers;
        Queue<Player> gameQueue;
        Dictionary<String, Player?> sides;
        String[,] gameBoard = new String[3, 3];
        List<List<Button>> buttonsMatrix = new List<List<Button>>();

        Player x_player, o_player;

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
            public string side;
            public bool isReceivedRequest;
            public bool isAccept;

            public Player(int gamesPlayed)
            {
                this.username = "";
                this.gamesPlayed = -1;
                this.win = 0;
                this.draw = 0;
                this.loss = 0;
                this.points = 0;
                this.socket = null;
                this.IPAddress = "";
                this.side = "";
                this.isReceivedRequest = false;
                this.isAccept = false;
            }
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
                this.side = "";
                this.isReceivedRequest = false;
                isAccept = false;

            }

        }


        public Form1()
        {
            /*To access UI elements in multi-thread level*/
            Control.CheckForIllegalCrossThreadCalls = false;

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            clientSocketArray = new List<Socket>();
            activePlayers = new List<Player>();
            gameQueue = new Queue<Player>();
            sides = new Dictionary<String, Player?>();
            sides.Add("X", null);
            sides.Add("O", null);


            x_player = new Player(-1);
            o_player = new Player(-1);

            isServerListening = false;
            isGameNotFinished = false;
            isXTurn = true;
            gameIsPending = false;
            InitializeComponent();
            initializeLeaderBoard();
            initializeButtonMatrix();
            initializeGameBoard();


        }


        private string getClientIPAddress(Socket server)
        {
            IPAddress clientIP = ((IPEndPoint)server.RemoteEndPoint).Address;
            string clientIPString = clientIP.ToString();
            return clientIPString;
        }

        void initializeButtonMatrix()
        {
            List<Button> row;

            row = new List<Button>();
            row.Add(btn_00);
            row.Add(btn_01);
            row.Add(btn_02);
            buttonsMatrix.Add(row);

            row = new List<Button>();
            row.Add(btn_10);
            row.Add(btn_11);
            row.Add(btn_12);
            buttonsMatrix.Add(row);

            row = new List<Button>();
            row.Add(btn_20);
            row.Add(btn_21);
            row.Add(btn_22);
            buttonsMatrix.Add(row);
        }

        void initializeGameBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameBoard[i, j] = "";
                }
            }
        }

        void initializeLeaderBoard()
        {
            dataGridView_learderboard.RowHeadersVisible = false;
            dataGridView_learderboard.ColumnCount = 6;

            dataGridView_learderboard.Columns[0].Name = "Name";
            dataGridView_learderboard.Columns[1].Name = "P";
            dataGridView_learderboard.Columns[2].Name = "W";
            dataGridView_learderboard.Columns[3].Name = "D";
            dataGridView_learderboard.Columns[4].Name = "L";
            dataGridView_learderboard.Columns[5].Name = "Points";


            dataGridView_learderboard.Columns[0].Width = 90;
            dataGridView_learderboard.Columns[1].Width = 30;
            dataGridView_learderboard.Columns[2].Width = 30;
            dataGridView_learderboard.Columns[3].Width = 30;
            dataGridView_learderboard.Columns[4].Width = 30;
            dataGridView_learderboard.Columns[5].Width = 60;

            foreach (DataGridViewColumn column in dataGridView_learderboard.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

        }

        void sortLeaderBoardByPoints()
        {
            dataGridView_learderboard.Sort(dataGridView_learderboard.Columns["Points"], ListSortDirection.Descending);
        }


        void updateServerStatus()
        {
            foreach (Socket clientSocket in clientSocketArray.ToList())
            {
                if (!clientSocket.Connected)
                {
                    Player player = findPlayerBySocket(clientSocket);
                    if (player.gamesPlayed != -1)
                    {
                        handleLeave(player);
                    }
                    else
                    {
                        clientSocketArray.Remove(clientSocket);
                        log_textbox.AppendText(getClientIPAddress(clientSocket) + " has disconnected from the server!\n");
                        clientSocket.Close();
                        clientSocket.Dispose();
                    }
                    updateLeaderBoard();
                }
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
                    String clientIPString = getClientIPAddress(newClient);
                    if (clientSocketArray.Count >= MAX_NUMBER_OF_PLAYERS)
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
            catch (Exception e)
            {
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
                updateLeaderBoard();
            }
            else
            {
                sendMessageToClientSocket(clientSocket, "401:Username is already taken!\n");
                log_textbox.AppendText(getClientIPAddress(clientSocket) + " has tried to take a username that is already exist!\n");
            }
        }

        void handleLeave(Player player)
        {

            if (player.username == x_player.username)
            {
                x_player = new Player(-1);
            }
            else if (player.username == o_player.username)
            {
                o_player = new Player(-1);
            }
            activePlayers.Remove(player);
            sendMessageToAllPlayers("info:" + player.username + " has left the game!\n");
            log_textbox.AppendText(player.username + " has left the game!\n");

            if (isGameNotFinished) dequeuePlayers();
            updateLeaderBoard();
        }


        void dequeuePlayers()
        {
            try
            {
                if (x_player.gamesPlayed == -1)
                {
                    x_player = gameQueue.Dequeue();
                }
                else if (o_player.gamesPlayed == -1)
                {
                    o_player = gameQueue.Dequeue();
                }

                if (x_player.gamesPlayed != -1 && o_player.gamesPlayed != -1)
                {

                    if (!x_player.isReceivedRequest) {
                        sendMessageToClientSocket(x_player.socket, "startreq:X:Are you ready to play as an X?\n");
                        x_player.isReceivedRequest = true;
                        log_textbox.AppendText("Server sent a game request to " + x_player.username + "\n");
                    }
                    if (!o_player.isReceivedRequest) {
                        sendMessageToClientSocket(o_player.socket, "startreq:O:Are you ready to play as an O?\n");
                        o_player.isReceivedRequest = true;
                        log_textbox.AppendText("Server sent a game request to " + o_player.username + "\n");
                    }

                }


            }
            catch (Exception e)
            {
                log_textbox.AppendText(e.Message + "\n");
            }

        }

        void handleQueue(Player player)
        {
            gameQueue.Enqueue(player);

            log_textbox.AppendText(player.username + " has entered the game queue!\n");
            sendMessageToAllPlayers("info:" + player.username + " has entered the game queue!\n");
            dequeuePlayers();

        }

        Player findPlayerBySocket(Socket clientSocket)
        {
            foreach (Player player in activePlayers)
            {
                if (player.socket == clientSocket) return player;
            }
            return new Player(-1);
        }

        void makeMoveX(Player player)
        {
            sendMessageToClientSocket(player.socket, "make-move-x:");
        }

        void makeMoveO(Player player)
        {
            sendMessageToClientSocket(player.socket, "make-move-y:");
        }

        private bool checkDraw()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (gameBoard[i, j] == "")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        bool checkWin()
        {
            for (int i = 0; i < 3; i++)
            {
                if (gameBoard[i, 0] != "" && gameBoard[i, 0] == gameBoard[i, 1] && gameBoard[i, 0] == gameBoard[i, 2])
                {
                    return true;
                }

                if (gameBoard[0, i] != "" && gameBoard[0, i] == gameBoard[1, i] && gameBoard[0, i] == gameBoard[2, i])
                {
                    return true;
                }
            }

            // Check diagonals
            if (gameBoard[0, 0] != "" && gameBoard[0, 0] == gameBoard[1, 1] && gameBoard[0, 0] == gameBoard[2, 2])
            {
                return true;
            }

            if (gameBoard[0, 2] != "" && gameBoard[0, 2] == gameBoard[1, 1] && gameBoard[0, 2] == gameBoard[2, 0])
            {
                return true;
            }
            return false;
        }

        void startGame(Player pX, Player pO)
        {
            pX.gamesPlayed++; pO.gamesPlayed++;
            isGameNotFinished = true;
            while (isGameNotFinished)
            {
                makeMoveX(pX);
                while (isXTurn) ;
                if (checkWin())
                {
                    pX.win++;
                    pO.loss++;
                    isGameNotFinished = false;
                    gameIsPending = false;
                    dequeuePlayers();
                    break;
                }
                else if (checkDraw())
                {
                    pX.draw++;
                    pO.draw++;
                    isGameNotFinished = false;
                    gameIsPending = false;
                    dequeuePlayers();
                    break;
                }
                makeMoveO(pO);
                while (!isXTurn) ;
                if (checkWin())
                {
                    pO.win++;
                    pX.loss++;
                    isGameNotFinished = false;
                    gameIsPending = false;
                    dequeuePlayers();
                    break;
                }
                else if (checkDraw())
                {
                    pX.draw++;
                    pO.draw++;
                    isGameNotFinished = false;
                    gameIsPending = false;
                    dequeuePlayers();
                    break;
                }
            }
        }

        void resetGameBoard()
        {
            //reset all board
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameBoard[i, j] = "";
                }
            }
            foreach (List<Button> row in buttonsMatrix)
            {
                foreach (Button button in row)
                {
                    button.Text = "";
                }
            }
            foreach (Player player in activePlayers)
            {
                sendBoardStatus(player.socket);
            }
        }

        void updateLeaderBoard()
        {
            /*mutex.WaitOne();
            dataGridView_learderboard.Rows.Clear();
            foreach (Player player in activePlayers)
            {
                dataGridView_learderboard.Rows.Add(player.username, player.gamesPlayed, player.win, player.draw, player.loss, player.points);
            }
            mutex.ReleaseMutex();*/

        }

        void sendBoardStatus(Socket clientSocket)
        {
            string gameBoardJSON = $"status:{JsonConvert.SerializeObject(gameBoard)}";
            byte[] buffer = Encoding.Default.GetBytes(gameBoardJSON);
            clientSocket.Send(buffer);

        }

        void updateGameBoard(int row, int col, string side)
        {
            gameBoard[row, col] = side;
            foreach (Player player in activePlayers)
            {
                sendBoardStatus(player.socket);
            }

        }

        void ClientController(Socket clientSocket) //listener of each client socket
        {
            while (clientSocket.Connected)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);

                    string token = Encoding.Default.GetString(buffer).Trim('\0');
                    string[] request = token.Split(":");
                    string action = request[0];

                    if (action == "join")
                    {
                        string username = request[1];
                        Thread joinThread = new Thread(() => handleJoin(clientSocket, username));
                        joinThread.Start();
                    }
                    else if (action == "queue")
                    {
                        Player p = findPlayerBySocket(clientSocket);
                        handleQueue((Player)p);
                   
                    }
                    else if (action == "accept")
                    {
                        Player player = findPlayerBySocket(clientSocket);
                        if (player.gamesPlayed != -1)
                        {
                            if (player.username == x_player.username)
                            {
                                log_textbox.AppendText($"{player.username} (X) is accepted!\n");
                                x_player.isAccept = true;
                            }
                            else if(player.username == o_player.username) {
                                log_textbox.AppendText($"{player.username} (O) is accepted!\n");
                                o_player.isAccept = true;
                            }

                            if(x_player.isAccept && o_player.isAccept)
                            {
                                sendMessageToAllPlayers($"info:The game between {x_player.username} and {o_player.username} is starting");
                                log_textbox.AppendText($"The game between {x_player.username} and {o_player.username} is starting");
                                Thread gameThread = new Thread(() => startGame(x_player,o_player));
                            }
                        }
                        

                     

                    }
                    else if (action == "move") //move:1-3
                    {
                        Player? p = findPlayerBySocket(clientSocket);
                        if (p.HasValue)
                        {
                            Player player = (Player)p;
                            string[] row_col = request[1].Split("-");
                            int row = int.Parse(row_col[0]);
                            int col = int.Parse(row_col[1]);
                            //updateGameBoard(row, col, player.current_side);
                            //isXTurn = player.current_side == "X" ? false : true;
                        }


                    }
                    else if (action == "leave")
                    {
                        Player? p = findPlayerBySocket(clientSocket);
                        if (p.HasValue) handleLeave((Player)p);

                    }
                    else
                    {
                        log_textbox.AppendText($"Invalid request ({action}) has been sent from {getClientIPAddress(clientSocket)}");
                    }
                }
                catch
                { }
                try
                {
                    updateServerStatus();
                }
                catch (Exception e)
                {
                    log_textbox.AppendText(e.Message + "\n");
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
            activePlayers.Clear();
            clientSocketArray.Clear();
            log_textbox.AppendText("Server has stopped accepting new connections!\n");
        }


        /* Form Closing event handler*/
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serverSocket != null) serverSocket.Close();
            isServerListening = false;
            Environment.Exit(0);
        }

        private void clearlogs_btn_Click(object sender, EventArgs e)
        {
            log_textbox.Clear();
        }
    }
}