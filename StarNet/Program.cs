using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Linq;
using System.IO;
using StarNet.Database;
using Newtonsoft.Json;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using StarNet.Packets.StarNet;

namespace StarNet
{
    class Program
    {
        public const int Version = 1;

        private static AsymmetricCipherKeyPair ServerKey;

        public static void Main(string[] args)
        {
            if (!File.Exists("node.key") || !File.Exists("node.key.pub"))
            {
                Console.WriteLine("Error: Please generate an RSA keypair with an empty password and save it to node.key and node.key.pub\n" +
                    "$ openssl genrsa -out node.key && openssl rsa -pubout -in node.key -out node.key.pub\n" +
                    "Distribute node.key.pub to a network authority node and add it to the trusted nodes with:\n" +
                    "$ mono StarNet.exe add-node <ip address> <port> <path/to/key.pub>");
                return;
            }
            LoadKeys();
            LocalSettings settings = new LocalSettings();
            if (File.Exists("node.config"))
            {
                JsonConvert.PopulateObject(File.ReadAllText("node.config"), settings);
                File.WriteAllText("node.config", JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            else
                settings = FirstRun(settings);
            var sharedDatabase = new SharedDatabase(settings.ConnectionString);
            if (args.Length != 0)
                HandleArguments(args, settings);
            else
            {
                var localNode = new StarNetNode(sharedDatabase, settings, 
                    new CryptoProvider(ServerKey), new IPEndPoint(IPAddress.Any, settings.StarboundPort));
                Console.WriteLine("Starting node {0}", localNode.Settings.UUID);
                localNode.Start();
                while (true)
                    Thread.Sleep(10000);
            }
        }

        static void LoadKeys()
        {
            var reader = new PemReader(new StreamReader("node.key"));
            try
            {
                ServerKey = (AsymmetricCipherKeyPair)reader.ReadObject();
            }
            catch
            {
                Console.WriteLine("Error: Unable to read key file.");
                throw;
            }
            finally
            {
                reader.Reader.Close();
            }
        }

        static LocalSettings FirstRun(LocalSettings settings)
        {
            Console.WriteLine("StarNet Node Setup");
            GetValueFromUser("Starbound port (21025): ", v => settings.StarboundPort = ushort.Parse(v));
            GetValueFromUser("StarNet port (21024): ", v => settings.NetworkPort = ushort.Parse(v));
            GetValueFromUser("PostgreSQL Connection String: ", v => settings.ConnectionString = v, false);
            File.WriteAllText("node.config", JsonConvert.SerializeObject(settings, Formatting.Indented));
            return settings;
        }

        delegate void ApplyValue(string result);
        static void GetValueFromUser(string prompt, ApplyValue apply, bool allowDefaultValue = true)
        {
            while (true)
            {
                Console.Write(prompt);
                var response = Console.ReadLine();
                if (!string.IsNullOrEmpty(response))
                {
                    try
                    {
                        apply(response);
                        break;
                    }
                    catch
                    {
                    }
                }
                else if (allowDefaultValue)
                    break;
            }
        }

        static void HandleArguments(string[] args, LocalSettings settings)
        {
            var action = args[0];
            try
            {
                switch (action)
                {
                    case "ping":
                        PingServer(settings);
                        break;
                    case "shutdown":
                        Shutdown(settings);
                        break;
                    case "user":
                        HandleUserCommand(args.Skip(1).ToArray(), settings);
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Error: not enough parameters for {0}", action);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid arguments.");
            }
        }

        private static void SendPacket(StarNetPacket packet, IPEndPoint endPoint, EventHandler confirmation, Action timedOut)
        {
            var network = new InterNodeNetwork(new CryptoProvider(ServerKey));
            network.Start();
            var reset = new ManualResetEvent(false);
            DateTime sent = default(DateTime);
            packet.ConfirmationReceived += (sender, e) =>
            {
                if (confirmation != null)
                    confirmation(sender, e);
                reset.Set();
            };
            sent = DateTime.Now;
            network.Send(packet, endPoint);
            if (confirmation == null)
                return;
            if (!reset.WaitOne(10000))
                if (timedOut != null)
                    timedOut();
        }

        private static void PingServer(LocalSettings settings)
        {
            var packet = new PingPacket();
            DateTime sent = DateTime.Now;
            var endPoint = new IPEndPoint(IPAddress.Loopback, settings.NetworkPort);
            Console.WriteLine("-> PING {0}", endPoint);
            SendPacket(packet, endPoint, (sender, e) =>
            {
                Console.WriteLine("<- PONG {0}ms", (int)(DateTime.Now - sent).TotalMilliseconds);
            }, () => Console.WriteLine("Timed out."));
        }

        private static void Shutdown(LocalSettings settings)
        {
            var packet = new ShutdownPacket();
            var endPoint = new IPEndPoint(IPAddress.Loopback, settings.NetworkPort);
            Console.WriteLine("Instructing {0} to shut down.", endPoint);
            SendPacket(packet, endPoint, null, null);
        }

        static void HandleUserCommand(string[] args, LocalSettings settings)
        {
            var action = args[0];
            var endPoint = new IPEndPoint(IPAddress.Loopback, settings.NetworkPort);
            try
            {
                switch (action)
                {
                    case "add":
                        var addUserPacket = new AddNewUserPacket
                        {
                            AccountName = args[1],
                            Password = args[2],
                            NetworkAdmin = bool.Parse(args[3])
                        };
                        SendPacket(addUserPacket, endPoint, (s, e) => Console.WriteLine("Done."), () => Console.WriteLine("Timed out."));
                        break;
                    case "drop":
                        var dropUserPacket = new DropUserPacket
                        {
                            AccountName = args[1]
                        };
                        SendPacket(dropUserPacket, endPoint, (s, e) => Console.WriteLine("Done."), () => Console.WriteLine("Timed out."));
                        break;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Error: not enough parameters for {0}", action);
            }
        }
    }
}