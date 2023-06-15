using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Threading;


namespace Server
{
    static class Program
    {
        public static ServerAction server;
        public static Form1 form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //use points for floats for easy compatibility with coordinates
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            server = new ServerAction();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);

        }

    }

    public class ServerAction
    {
        private int port = 6321;

        private List<ServerClient> clients = new List<ServerClient>();
        private List<ServerClient> disconnectList = new List<ServerClient>();
        private TcpListener server;
        private bool serverStarted;
        private List<Unit> units = new List<Unit>();
        private bool ResyncNeeded = false;

        //the constructor, adds the listener
        public ServerAction()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                StartListening();
                serverStarted = true;
            }
            catch (Exception e)
            {
                Program.form.DebugTextBox.Text += "\r\n" + e.Message;
            }
        }
      

        //called at every fixed time intervals => time can be adjusted at timer component's property
        //used to check if there's incoming data
        public void Update()
        {
            if (!serverStarted)
                return;

            foreach (ServerClient c in clients)
            {
                // Is the client still connected?
                if (!IsConnected(c.tcp))
                {
                    c.tcp.Close();
                    disconnectList.Add(c);
                    continue;
                }
                else
                {
                    NetworkStream s = c.tcp.GetStream();
                    if (s.DataAvailable)
                    {
                        StreamReader reader = new StreamReader(s, true);
                        string data = reader.ReadLine();

                        if (data != null)
                            OnIncomingData(c, data);
                    }
                }
            }

            //checking disconnected players
            for (int i = 0; i < disconnectList.Count - 1; i++)
            {
                for (int j = 0; j < units.Count; j++)
                {
                    if (units[j].clientName == disconnectList[i].clientName)
                    {
                        units.RemoveAt(j);
                    }
                }
                Program.form.DebugTextBox.Text += "\r\nUser disconnected:" + disconnectList[i].clientName;
                clients.Remove(disconnectList[i]);
                disconnectList.RemoveAt(i);
                ResyncNeeded = true;
            }
            //if some1 disconnected, tell it to other players as well.
            if (ResyncNeeded){
                SynchronizeUnits();
                ResyncNeeded = false;
            }
        }

        private void StartListening()
        {
            server.BeginAcceptTcpClient(AcceptTcpClient, server);
        }

        private void AcceptTcpClient(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            string allUsers = "";
            foreach (ServerClient i in clients)
            {
                allUsers += i.clientName + '|';
            }

            ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
            clients.Add(sc);

            StartListening();
            //request authentication from client
            Broadcast("WhoAreYou|", clients[clients.Count - 1]);
        }

        private bool IsConnected(TcpClient c)
        {
            try
            {
                if (c != null && c.Client != null && c.Client.Connected)
                {
                    if (c.Client.Poll(0, SelectMode.SelectRead))
                        return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        // Server Send
        private void Broadcast(string data, List<ServerClient> cl)
        {
            foreach (ServerClient sc in cl)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                    writer.WriteLine(data);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    Program.form.DebugTextBox.Text += "\r\n" + e.Message;
                }
            }
        }

        private void Broadcast(string data, ServerClient c)
        {
            List<ServerClient> sc = new List<ServerClient> { c };
            Broadcast(data, sc);
        }

        // Server Read
        private void OnIncomingData(ServerClient c, string data)
        {
            string[] aData = data.Split('|');

            //login
            if (c.clientName != null)
            {
                Program.form.DebugTextBox.Text += "\r\nClient '" + c.clientName + "' sent command: " + data;
            }
            else
            {
                Program.form.DebugTextBox.Text += "\r\nNew Client trying to join server. Requesting authentication.";
                if (aData[0] == "Iam")
                {
                    bool authenticated =  Database.AuthenticateUser(aData[1], aData[2]);
                    if (authenticated)
                    {
                        foreach (ServerClient client in clients)
                        {
                            if (aData[1] == client.clientName)
                            {
                                Program.form.DebugTextBox.Text += "\r\nThis user is already connected";
                                c.tcp.Close();
                                disconnectList.Add(c);
                                return;
                            }
                        }
                        c.clientName = aData[1];
                        Program.form.DebugTextBox.Text += "\r\nUser authenticated";
                        Broadcast("Authenticated|", c);
                    }
                    else
                    {
                        Program.form.DebugTextBox.Text += "\r\nUser authentication failed, client disconnected.";
                        c.tcp.Close();
                        disconnectList.Add(c);
                    }
                    return; 
                }
                
            }


            //gameplay commands
            switch (aData[0])
            {
                case "SynchronizeRequest":
                    SynchronizeUnits(c);
                    break;
                case "SpawnUnit":
                    Unit unit = new Unit();
                    unit.clientName = c.clientName;
                    //give a new ID to the new units
                    int newid = 0;
                    foreach (Unit u in units)
                    {
                        if (u.unitID >= newid) { newid = u.unitID + 1; }
                    }

                    unit.unitID = newid;
                    unit.unitPositionX = 0.1f;
                    unit.unitPositionY = -6.09f;
                    unit.unitPositionZ = 0.0f;
                    units.Add(unit);
                    
                    Broadcast("UnitSpawned|" + c.clientName + "|" + unit.unitID + "|" + unit.unitPositionX + "|" + unit.unitPositionY + "|" + unit.unitPositionZ, clients);
                    break;
                case "Moving":
                    Broadcast("UnitMoved|" + c.clientName + "|" + aData[1] + "|" + aData[2] + "|" + aData[3] + "|" + aData[4] + "|" + aData[5], clients);
                    int id;
                    Int32.TryParse(aData[1], out id);
                    float parsedX;
                    float parsedY;
                    int AnimtionState;
                    int ValueFlip;
                    float.TryParse(aData[2], out parsedX);
                    float.TryParse(aData[3], out parsedY);
                    Int32.TryParse(aData[4], out AnimtionState);
                    Int32.TryParse(aData[5], out ValueFlip);
                    foreach (Unit u in units)
                    {
                        if (u.unitID == id)
                        {
                            u.unitPositionX = parsedX;
                            u.unitPositionY = parsedY;
                            u.ValueStateAnim = AnimtionState;
                            u.IsFlipRight = ValueFlip;
                        }
                    }
                    Program.form.DebugTextBox.Text += "\r\n" + parsedX + "  " + parsedY + "  " + AnimtionState;
                    break;
                case "Chat":
                    Program.form.DebugTextBox.Text += "\r\nChat";
                    break;
                default:
                    Program.form.DebugTextBox.Text += "\r\nReceived unknown signal => skipping";
                    break;
            }
        }
        
        //syncing 1 client
        private void SynchronizeUnits(ServerClient c)
        {
            string dataToSend = "Synchronizing|" + units.Count;
            /*string dataToSendFlip = "SynchronizingFlip|" + units.Count;*/
            
            foreach (Unit u in units)
            {
                dataToSend += "|" + (u.unitID) + "|" + u.unitPositionX + "|" + u.unitPositionY + "|" + u.ValueStateAnim + "|" +u.IsFlipRight;
                /*dataToSendFlip += "|" + (u.unitID) + "|" + u.IsFlipRight;*/
                
            }
            
            Broadcast(dataToSend, c);
            /*Broadcast(dataToSendFlip, c);*/
            
            Program.form.DebugTextBox.Text += "\r\nSynchronization request sent: " + dataToSend;
           /* Program.form.DebugTextBox.Text += "\r\nSynchronization request sent FLIP: " + dataToSendFlip;*/
            
        }

        //syncing all clients
        private void SynchronizeUnits()
        {
            string dataToSend = "Synchronizing|" + units.Count;
            /*string dataToSendFlip = "SynchronizingFlip|" + units.Count;*/
            
            foreach (Unit u in units)
            {
                dataToSend += "|" + (u.unitID) + "|" + u.unitPositionX + "|" + u.unitPositionY + "|" + u.ValueStateAnim + "|" +u.IsFlipRight;
               /* dataToSendFlip += "|" + (u.unitID) + "|" + u.IsFlipRight;*/
                
            }
            
            Broadcast(dataToSend, clients);
            /*Broadcast(dataToSendFlip, clients);*/
           
            Program.form.DebugTextBox.Text += "\r\nSynchronization request sent: " + dataToSend;
           /* Program.form.DebugTextBox.Text += "\r\nSynchronization request sent FLIP: " + dataToSendFlip;*/
            
        }
    }
    

    public class ServerClient
    {
        public string clientName;
        public TcpClient tcp;
        public ServerClient(TcpClient tcp)

        {
            this.tcp = tcp;
        }
    }

    public class Unit
    {
        public string clientName;
        public int unitID;
        public float unitPositionX;
        public float unitPositionY;
        public float unitPositionZ;
        public int IsFlipRight;
        public int ValueStateAnim;
    }

    public static class HashMD5
    {
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Chuyển đổi chuỗi đầu vào thành một mảng byte và tính toán mã hash
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Tạo một StringBuilder để chứa các byte đã tính toán thành mã hash
                StringBuilder sBuilder = new StringBuilder();

                // Lặp qua mỗi byte trong mảng và định dạng chúng thành một chuỗi hex
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Trả về chuỗi hex đã được tạo thành từ mã hash
                return sBuilder.ToString();
            }
        }
    }


    public static class Database
    {
        public static bool AuthenticateUser(string username, string password)
        {
            int result;
            string PassAfterHash;
            PassAfterHash = HashMD5.GetMd5Hash(password);
            result = (int)Program.form.usersTableAdapter.Authenticate(username, PassAfterHash);
            if (result == 1)
            {
                return true;
            }
            else return false;
        }
    }
}
