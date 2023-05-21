using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace TicTacToeServer
{
    public partial class Form1 : Form
    {
        const int MAX_NUMBER_OF_PLAYERS = 4;
        Socket serverSocket;
        Queue<Socket> clientSocketsQueue;

        bool isServerListening;


        public Form1()
        {
            /*To access UI elements in multi-thread level*/
            Control.CheckForIllegalCrossThreadCalls = false;

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            isServerListening = false;

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocketsQueue = new Queue<Socket>();

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

                    if (clientSocketsQueue.Count == MAX_NUMBER_OF_PLAYERS)
                    {
                        log_textbox.AppendText(clientIPString + " has tried to connect server but server is at maximum capacity!\n");
   
                        byte[] buffer = Encoding.Default.GetBytes("400:Server is at maximum capacity!");
                        newClient.Send(buffer);
                        newClient.Close();
                    }
                    else
                    {
                        /*Adding new client socket into client socket queue*/
                        clientSocketsQueue.Enqueue(newClient);

                        log_textbox.AppendText(clientIPString + " has connected to the server!\n");
                        byte[] buffer = Encoding.Default.GetBytes("200:Connection is OK!");
                        newClient.Send(buffer);

                        //Thread joinThread = new Thread();
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