using System;
using System.Net.Sockets;
using System.Text;

public class Client
{
    public static void CreateClient(int clientId)
    {
        TcpClient client = new TcpClient("localhost", 9099); //connecting to the server
        NetworkStream stream = client.GetStream();

        //preparing the request parameters
        string par1 = $"value1_client{clientId}".PadRight(30, '\0');  //ensuring the length of strings
        string par2 = $"value2_client{clientId}".PadRight(30, '\0');

        byte[] data = new byte[60];  //packing the data,30 bytes for each parameter
        Encoding.UTF8.GetBytes(par1, 0, par1.Length, data, 0);
        Encoding.UTF8.GetBytes(par2, 0, par2.Length, data, 30);

        stream.Write(data, 0, data.Length); //sending the data to the server

        //receiving response in multiple packets
        byte[] responseBuffer = new byte[1024];  //buffer to store the entire response
        int totalBytesRead = 0;

        while (true)
        {
            int bytesRead = stream.Read(responseBuffer, totalBytesRead, responseBuffer.Length - totalBytesRead);

            if (bytesRead == 0) break;  //to stop reading when no more data is available
            totalBytesRead += bytesRead;
        }

        //unpacking the response
        string response = Encoding.UTF8.GetString(responseBuffer, 0, totalBytesRead).Trim('\0');
        Console.WriteLine($"Client {clientId} - Received response: {response}");

        client.Close();
    }

    public static void Main(string[] args)
    {
        int numberOfClients = 10;

        for (int i = 0; i < numberOfClients; i++)
        {
            CreateClient(i);  //creating each client
        }
    }
}
