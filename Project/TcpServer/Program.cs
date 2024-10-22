using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server
{
    public static string currentState = "Idle";
    private static Dictionary<string, int> clientContactCounts = new Dictionary<string, int>();

    public static void HandleClient(TcpClient client)
    {
        Console.WriteLine("Connection created with client.");

        NetworkStream stream = client.GetStream();
        currentState = "Waiting for Request";

        byte[] buffer = new byte[60]; //creating a buffer to store the response from the client(30 bytes for each parameter)
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead > 0)
        {
            string par1 = Encoding.UTF8.GetString(buffer, 0, 30).Trim('\0'); //unpacking the data
            string par2 = Encoding.UTF8.GetString(buffer, 30, 30).Trim('\0');

            Console.WriteLine($"Received parameters: {par1}, {par2}");

            currentState = "Processing Request";

            string clientAddress = client.Client.RemoteEndPoint.ToString(); //to track contact counts

            if (!clientContactCounts.ContainsKey(clientAddress))
            {
                clientContactCounts[clientAddress] = 0;
            }

            clientContactCounts[clientAddress]++;
            int contactCount = clientContactCounts[clientAddress];

            //Below, splitting response into multiple packets
            string responseMessage = $"Request received: {par1}, {par2} - Contact count: {contactCount}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);

            int packetSize = 10;
            int totalPackets = (int)Math.Ceiling((double)responseBytes.Length / packetSize);

            currentState = "Sending Response";

            for (int i = 0; i < totalPackets; i++)
            {
                int offset = i * packetSize;
                int sizeToSend = Math.Min(packetSize, responseBytes.Length - offset); //calculating process - how much data to send in this packet

                byte[] packet = new byte[sizeToSend];
                Array.Copy(responseBytes, offset, packet, 0, sizeToSend);

                stream.Write(packet, 0, packet.Length);
                Console.WriteLine($"Sent packet {i + 1}/{totalPackets}: {Encoding.UTF8.GetString(packet)}");
            }

            currentState = "Idle";
        }

        client.Close();
    }

    public static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 9099);

        server.Start();
        Console.WriteLine("Server is listening on port 9099...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient(); 
            Thread clientThread = new Thread(() => HandleClient(client)); //handling client in each thread
            clientThread.Start();
        }
    }
}