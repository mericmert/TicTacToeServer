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
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TicTacToeServer
{
    public partial class Form1 : Form
    {
        const int MAX_NUMBER_OF_PLAYERS = 4;

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

            initializeButtonMatrix();
            initializeGameBoard();
            updateLeaderBoard();
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
                    gameBoard[i, j] = " ";
                }
            }
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

                        sendMessageToClientSocket(newClient, "400:Server is at maximum capacity!");
                    }
                    else
                    {
                        /*Adding new client socket into client socket queue*/
                        clientSocketArray.Add(newClient);
                        log_textbox.AppendText(clientIPString + " has connected to the server!\n");

                        sendMessageToClientSocket(newClient, "200:Connection is OK!");

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
                Thread.Sleep(150);
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
                sendMessageToClientSocket(clientSocket, "201:You have succesfully joined the game room!\n");
                sendMessageToAllPlayers($"update:newplayer:{username}");
                log_textbox.AppendText(username + " has joined the game room!\n");
                updateLeaderBoard();
                sendBoardStatus(player.socket);
                sendMessageToClientSocket(clientSocket, $"update:currentplayers:{wrapUsernames()}");
                sendMessageToClientSocket(clientSocket, $"update:vs:{x_player.username}:{o_player.username}");

                if (isGameNotFinished)
                    sendMessageToClientSocket(clientSocket, $"info:The {x_player.username} and {o_player.username} is playing currently.\n");

            }
            else
            {
                sendMessageToClientSocket(clientSocket, "401:Username is already taken!\n");
                log_textbox.AppendText(getClientIPAddress(clientSocket) + " has tried to take a username that is already exist!\n");
            }
        }

        string wrapUsernames()
        {
            string msg = "";
            for (int i = 0; i < activePlayers.Count; i++)
            {
                msg += activePlayers[i].username;
                if (i < activePlayers.Count - 1)
                    msg += ",";
            }
            return msg;
        }

        void handleLeave(Player player)
        {
            activePlayers.Remove(player);

            string side = "";
            if (player.username == x_player.username)
                side = "X";
            else if (player.username == o_player.username)
                side = "O";
            else
                side = "-";

            sendMessageToAllPlayers($"update:left:{side}:{player.username} has left the game!\n");

            if (side != "-")
                sendMessageToAllPlayers($"info:Waiting for a player to play as {side}...\n");

            log_textbox.AppendText(player.username + " has left the game!\n");

            if (player.username == x_player.username)
            {
                x_player = new Player(-1);
            }
            else if (player.username == o_player.username)
            {
                o_player = new Player(-1);
            }


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

                    if (!x_player.isReceivedRequest)
                    {
                        sendMessageToClientSocket(x_player.socket, "startreq:X:Are you ready to play as X?\n");
                        x_player.isReceivedRequest = true;
                        log_textbox.AppendText("Server sent a game request to " + x_player.username + "\n");
                    }
                    if (!o_player.isReceivedRequest)
                    {
                        sendMessageToClientSocket(o_player.socket, "startreq:O:Are you ready to play as O?\n");
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

        void makeMoveX()
        {
            log_textbox.AppendText("move request is sent to X!\n");
            sendBoardStatusToAll();
            sendMessageToClientSocket(x_player.socket, "yourturn:X:");

        }

        void makeMoveO()
        {
            log_textbox.AppendText("move request is sent to O!\n");
            sendBoardStatusToAll();
            sendMessageToClientSocket(o_player.socket, "yourturn:O");
        }

        private bool checkDraw()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (gameBoard[i, j] == " ")
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
                if (gameBoard[i, 0] != " " && gameBoard[i, 0] == gameBoard[i, 1] && gameBoard[i, 0] == gameBoard[i, 2])
                {
                    return true;
                }

                if (gameBoard[0, i] != " " && gameBoard[0, i] == gameBoard[1, i] && gameBoard[0, i] == gameBoard[2, i])
                {
                    return true;
                }
            }

            // Check diagonals
            if (gameBoard[0, 0] != " " && gameBoard[0, 0] == gameBoard[1, 1] && gameBoard[0, 0] == gameBoard[2, 2])
            {
                return true;
            }

            if (gameBoard[0, 2] != " " && gameBoard[0, 2] == gameBoard[1, 1] && gameBoard[0, 2] == gameBoard[2, 0])
            {
                return true;
            }
            return false;
        }

        void startGame()
        {
            resetGameBoard();
            sendMessageToAllPlayers($"info:The game between {x_player.username} and {o_player.username} is starting\n");
            sendMessageToAllPlayers($"update:vs:{x_player.username}:{o_player.username}");
            log_textbox.AppendText($"The game between {x_player.username} and {o_player.username} is starting\n");
            currentPlayers_label.Text = $"{x_player.username} vs {o_player.username}";

            //x_player.gamesPlayed++; o_player.gamesPlayed++;

            isGameNotFinished = true;
            while (isGameNotFinished)
            {
                makeMoveX();
                while (isXTurn) ;
                if (checkWin())
                {
                    sendMessageToAllPlayers($"info:{x_player.username} (X) has won the game!\n");
                    sendMessageToClientSocket(x_player.socket, $"update:finish:win:{x_player.username}");
                    sendMessageToClientSocket(o_player.socket, $"update:finish:lose:{o_player.username}");


                    log_textbox.AppendText($"{x_player.username} (X) has won the game!\n");
                    //x_player.win++;
                    incrementField(x_player.username, 'W');
                    //o_player.loss++;
                    incrementField(o_player.username, 'L');
                    break;
                }
                else if (checkDraw())
                {
                    sendMessageToAllPlayers("info:Game is Tie!\n");
                    sendMessageToClientSocket(x_player.socket, $"update:finish:draw:{x_player.username}");
                    sendMessageToClientSocket(o_player.socket, $"update:finish:draw{o_player.username}");

                    log_textbox.AppendText("Game is Tie!\n");
                    //x_player.draw++;
                    incrementField(x_player.username, 'D');
                    //o_player.draw++;
                    incrementField(o_player.username, 'D');
                    break;
                }
                makeMoveO();
                while (!isXTurn) ;
                if (checkWin())
                {
                    sendMessageToAllPlayers($"info:{o_player.username} (O) has won the game!\n");
                    sendMessageToClientSocket(o_player.socket, $"update:finish:win:{o_player.username}");
                    sendMessageToClientSocket(x_player.socket, $"update:finish:lose:{x_player.username}");


                    log_textbox.AppendText($"{o_player.username} (O) has won the game!\n");
                    //o_player.win++;
                    incrementField(o_player.username, 'W');
                    //x_player.loss++;
                    incrementField(x_player.username, 'L');
                    break;
                }
                else if (checkDraw())
                {
                    sendMessageToAllPlayers("info:Game is Tie!\n");
                    sendMessageToClientSocket(x_player.socket, $"update:finish:draw:{x_player.username}");
                    sendMessageToClientSocket(o_player.socket, $"update:finish:draw:{o_player.username}");

                    log_textbox.AppendText("Game is Tie!\n");
                    //x_player.draw++;
                    incrementField(x_player.username, 'D');
                    //o_player.draw++;
                    incrementField(x_player.username, 'D');

                    break;
                }
            }
            x_player = new Player(-1);
            o_player = new Player(-1);
            isGameNotFinished = false;
            gameIsPending = false;
            isXTurn = true;


            int k = Math.Min(gameQueue.Count, 2);

            for (int i = 0; i < k; i++)
            {
                dequeuePlayers();
            }
        }

        void resetGameBoard()
        {
            //reset all board
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    gameBoard[i, j] = " ";
                }
            }
            /*foreach (List<Button> row in buttonsMatrix)
            {
                foreach (Button button in row)
                {
                    button.Text = " ";
                }
            }*/
            sendBoardStatusToAll();
            updateGameUI();
        }

        void updateLeaderBoard()
        {
            richTextBox_Leaderboard.Clear();

            string output = "";

            // Print the header
            output += printTableRow("Username", "P", "W", "D", "L", "Points");
            output += printTableRow("========", "=", "=", "=", "=", "======");

            if (activePlayers.Count > 0)
            {
                List<Player> tempList = activePlayers.ToList();
                tempList.Sort((p1, p2) => p1.points.CompareTo(p2.points));

                activePlayers.Sort((p1, p2) => p2.points.CompareTo(p1.points));

                // Print player information
                foreach (var player in activePlayers)
                {
                    output += printTableRow(formatTableUsername(player.username), player.gamesPlayed.ToString(), player.win.ToString(), player.draw.ToString(), player.loss.ToString(), player.points.ToString());
                }
            }


            richTextBox_Leaderboard.AppendText(output);

            sendMessageToAllPlayers($"update:leaderboard:{output}");
        }

        private string printTableRow(params string[] columns)
        {
            const int columnWidth = 12; // Adjust the column width as needed

            string formattedLine = string.Join("\t", columns.Select(column => string.Format("{0,-" + columnWidth + "}", column)));
            //richTextBox_Leaderboard.AppendText(formattedLine + "\n");

            return formattedLine + "\n";
        }

        string formatTableUsername(string name)
        {
            int maxlen = 14;
            int minlen = 4;

            if (name.Length < minlen)
                return name + "\t";

            if (name.Length == maxlen)
                return name;
            else if (name.Length < maxlen)
            {
                return name + String.Concat(Enumerable.Repeat(' ', maxlen - name.Length));
            }
            else
            {
                return name.Substring(0, maxlen - 3) + String.Concat(Enumerable.Repeat('.', 3));
            }
        }


        void sendBoardStatus(Socket clientSocket)
        {
            string board = "update:board:";
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    board += $"{gameBoard[i,j]},";
                }
            }
            board = board.Substring(0, board.Length - 1);

            sendMessageToClientSocket(clientSocket, board);
        }

        void sendBoardStatusToAll()
        {
            foreach (Player player in activePlayers)
            {
                sendBoardStatus(player.socket);
            }
        }

        void updateGameUI()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    buttonsMatrix[i][j].Text = gameBoard[i, j];
                }
            }
        }


        void updateGameBoard(int row, int col, string side)
        {
            gameBoard[row, col] = side;
            updateGameUI();
            sendBoardStatusToAll();

        }

        void updatePlayersLabel()
        {
            currentPlayers_label.Text = x_player.username + " vs " + o_player.username;
            sendMessageToAllPlayers($"update:label:{x_player.username}:{o_player.username}");
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
                        handleQueue(p);

                    }
                    else if (action == "accept")
                    {
                        Player player = findPlayerBySocket(clientSocket);
                        if (player.gamesPlayed != -1)
                        {
                            updatePlayersLabel();
                            if (player.username == x_player.username)
                            {
                                log_textbox.AppendText($"{player.username} (X) is accepted!\n");
                                x_player.isAccept = true;
                                if (isXTurn && isGameNotFinished)
                                    makeMoveX();

                            }
                            else if (player.username == o_player.username)
                            {
                                log_textbox.AppendText($"{player.username} (O) is accepted!\n");
                                o_player.isAccept = true;
                                if (!isXTurn && isGameNotFinished)
                                    makeMoveO();
                            }

                            if (x_player.isAccept && o_player.isAccept)
                            {
                                x_player.isAccept = false;
                                o_player.isAccept = false;
                                sendMessageToClientSocket(x_player.socket, $"startplay:{o_player.username}");
                                sendMessageToClientSocket(o_player.socket, $"startplay:{x_player.username}");
                                Thread gameThread = new Thread(() => startGame());
                                gameThread.Start();
                            }
                        }

                    }
                    else if (action == "move") //move:1-3
                    {
                        Player player = findPlayerBySocket(clientSocket);
                        if (player.gamesPlayed != -1)
                        {
                            string[] row_col = request[1].Split("-");
                            int row = int.Parse(row_col[0]);
                            int col = int.Parse(row_col[1]);
                            string side = isXTurn ? "X" : "O";
                                                     
                            isXTurn = !isXTurn;
                            updateGameBoard(row, col, side);
                        }



                    }
                    else if (action == "leave")
                    {
                        Player p = findPlayerBySocket(clientSocket);
                        if (p.gamesPlayed != -1) handleLeave(p);

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
            resetGameBoard();
            serverSocket.Close();
            serverSocket.Dispose();
            activePlayers.Clear();
            clientSocketArray.Clear();
            gameQueue.Clear();
            x_player = new Player(-1);
            o_player = new Player(-1);
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

        private void incrementField(string name, char field)
        {
            Player player = activePlayers.Find(p => p.username == name);

            Player player2 = new Player
            {
                username = player.username,
                gamesPlayed = player.gamesPlayed,
                win = player.win,
                draw = player.draw,
                loss = player.loss,
                points = player.points,
                socket = player.socket,
                IPAddress = player.IPAddress,
                side = player.side,
                isReceivedRequest = player.isReceivedRequest,
                isAccept = player.isAccept,
            };

            player2.gamesPlayed++;

            if (field == 'W')
            {
                player2.win++;
                player2.points += 3;
            }
            else if (field == 'L')
            {
                player2.loss++;
            }
            else if (field == 'D')
            {
                player2.draw++;
                player2.points++;
            }

            activePlayers.Remove(player);
            activePlayers.Add(player2);

            updateLeaderBoard();
        }

    }
}