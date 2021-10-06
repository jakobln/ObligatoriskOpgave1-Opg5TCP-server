using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using UnitTestsOblOpg;

namespace TCPServerOblOpg
{
    public class Program
    {
        public static int _nextId = 1;
        public static List<FootballPlayer> players = new List<FootballPlayer>()
        {
            new FootballPlayer(_nextId++, "Anders", 500, 1),
            new FootballPlayer(_nextId++, "Peter", 3000, 2)
        };

    static void Main(string[] args)
        {
            Console.WriteLine("Player server ready");
            TcpListener listener = new TcpListener(IPAddress.Any, 2121);
            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Console.WriteLine("New client");

                Task.Run(() =>
                {
                    HandleClient(socket);
                });
            }
        }

        private static void HandleClient(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);
            string message = reader.ReadLine();
            string message2 = reader.ReadLine();
            int Id;

            if (message == "HentAlle" && message2 == "")
            {
                string serializedPlayers = JsonSerializer.Serialize(players);
                writer.WriteLine(serializedPlayers);
                writer.Flush();
            }

            else if (message == "Hent" && int.TryParse(message2, out Id))
            {
                FootballPlayer myplayer = players.Find(FootballPlayer => FootballPlayer.Id == Id);
                string serializedPlayer = JsonSerializer.Serialize(myplayer);
                writer.WriteLine(serializedPlayer);
                writer.Flush();
            }

            else if (message == "Gem" && message2.StartsWith("{"))
            {
                FootballPlayer fromJson = JsonSerializer.Deserialize<FootballPlayer>(message2);
                Console.WriteLine("ID: " + fromJson.Id);
                Console.WriteLine("Name: " + fromJson.Name);
                Console.WriteLine("Price: " + fromJson.Price);
                Console.WriteLine("Shirt number: " + fromJson.ShirtNumber);
                players.Add(new FootballPlayer(fromJson.Id, fromJson.Name, fromJson.Price, fromJson.ShirtNumber));
                writer.Flush();
            }

            socket.Close();
        }
    }
}