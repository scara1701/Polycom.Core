//Tested with Polycom HDX6000 unit
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Polycom.Core
{
    //Args for response event
    public class ResponseEventArgs : EventArgs
    {
        public string Response { get; set; }
        public ResponseEventArgs(string response)
        {
            Response = response;
        }
    }

    //Handler for telnet repsonse
    public delegate void ResponseHandler(object sender, ResponseEventArgs e);

    public static class Communicator
    {

        private static string responseData;
        public static string ResponseData
        {
            get { return responseData; }
            set
            {
                responseData = value;
                Response(typeof(Communicator), new ResponseEventArgs(responseData));
            }
        }

        public static TcpClient TCPClient { get; set; }
        private static String Message { get; set; }
        private static Byte[] Data { get; set; }
        private static Int32 Bytes { get; set; }
        private static Int32 Timeout { get; set; }
        public static NetworkStream Stream { get; set; }
        public static event ResponseHandler Response;

        /// <summary>
        /// Initializes the TCPClient
        /// </summary>
        /// <param name="ipaddress">IP address of the Polycom device</param>
        /// <param name="port">Port number used by Polycom for commands. Usually TCP Port 24</param>
        /// <param name="timeoutInMiliseconds">Timeout of commands in miliseconds, default = 2 s,  2000 ms</param>
        public static void Init(string ipaddress, Int32 timeoutInMiliseconds = 2000, Int32 port = 24)
        {
            ResponseData = string.Empty;
            Message = string.Empty;
            TCPClient = new TcpClient(ipaddress, port);
            Timeout = timeoutInMiliseconds;
            ReadResponse();
        }

        /// <summary>
        /// Terminates the TCP client
        /// </summary>
        public static void TerminateConnection()
        {
            Stream.Close();
            TCPClient.Close();
            ResponseData = "Connection terminated";
        }

        /// <summary>
        /// Dials an adressbook item.
        /// </summary>
        /// <param name="addressBookItem">Must match an existing addressbook item on the Polycom device</param>
        public static void DialAddressBookItem(string addressBookItem)
        {
            DoCommand("dial addressbook \"" + addressBookItem + "\"" + Environment.NewLine);
        }



        /// <summary>
        /// Dials an adres
        /// </summary>
        /// <param name="address">IP-address or hostname of other device</param>
        /// <param name="speed">Requested speed in kbps</param>
        public static void DialAddress(int speed, string address)
        {
            DoCommand("dial auto " + speed + " \"" + address + "\"" + Environment.NewLine);
        }


        /// <summary>
        /// Gets the current callstate
        /// </summary>
        public static void GetCallState()
        {
            DoCommand("getcallstate" + Environment.NewLine);
        }



        /// <summary>
        /// Disconnects all calls
        /// </summary>
        public static void Hangup()
        {
            DoCommand("hangup all" + Environment.NewLine);
        }

        /// <summary>
        /// runs the command on the Polycom device
        /// </summary>
        /// <param name="command">Use a valid command from the polycom API</param>
        private static void DoCommand(string command)
        {
            Thread.Sleep(Timeout);
            Message = command + Environment.NewLine;
            Data = Encoding.ASCII.GetBytes(Message);
            Stream.Write(Data, 0, Data.Length);
            ReadResponse();
        }

        /// <summary>
        /// Sends the reboot command
        /// </summary>
        public static void Reboot()
        {
            DoCommand("reboot now");
        }

        /// <summary>
        /// reads the response of the Polycom device
        /// </summary>
        /// <returns>Console feedback of Polycom device</returns>
        private static void ReadResponse()
        {
            Thread.Sleep(Timeout);
            Stream = TCPClient.GetStream();
            Data = new byte[1024];
            Bytes = Stream.Read(Data, 0, Data.Length);
            ResponseData = Encoding.ASCII.GetString(Data, 0, Bytes);
        }
    }
}
