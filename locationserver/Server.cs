using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace locationserver
{
    class Server
    {
        public bool UIMode = false;

        public TcpListener listener;
        public Socket connection;
        protected Handler RequestHandler;
        public Dictionary<string, string> personLocation = new Dictionary<string, string>();

        private string logPath, dbPath;
        private bool debug = false;

        public void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-l":
                        logPath = args[i + 1];
                        break;
                    case "-f":
                        if (args.Length >= i + 1)
                        {
                            dbPath = args[i + 1];
                        }
                        break;
                    case "-d":
                        debug = true;
                        break;
                }
            }
            runServer();
        }
        private void runServer()
        {
            if (dbPath != null) { LoadDb(dbPath, personLocation); }
            try
            {
                listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();

                Console.WriteLine("Server started");
                if (debug) { Console.WriteLine("Debugging mode is enabled"); };
                while (true)
                {
                    if (!UIMode)
                    {
                        serverLoop();
                    }
                    else
                    {
                        break;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exeption: " + e.ToString());
            }
        }

        public void serverLoop()
        {
            try
            {
                connection = listener.AcceptSocket();
                RequestHandler = new Handler();
                string lg;
                new Thread(() => RequestHandler.doRequest(connection, out lg, personLocation)).Start();
            }
            catch (Exception)
            {
                throw;
            }
        }


        static void LoadDb(string path, Dictionary<string, string> db)
        {
            try
            {
                StreamReader sr = new StreamReader(path);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] arr = line.Split(" ");
                    db.Add(arr[0], arr[1]);
                }
            }
            catch
            {
            }
        }
        public class Handler: Server
        {
            private static readonly object locker = new object();
            public NetworkStream socketStream;
            public void doRequest(Socket connection, out string lg,Dictionary<string,string> personLocation)
            {
                lg = null;
                socketStream = new NetworkStream(connection);
                string ip = ((IPEndPoint)(connection.RemoteEndPoint)).Address.ToString();
                Console.WriteLine("New Connection");
                try
                {
                    int timeOut = 1000;
                    socketStream.ReadTimeout = timeOut;
                    socketStream.WriteTimeout = timeOut;
                    StreamReader sr = new StreamReader(socketStream);

                    string line = null;
                    while (line == null)
                    {
                        try
                        {
                            while (sr.Peek() > -1)
                            {
                                line += (char)sr.Read();
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (debug) { Console.WriteLine($"Server received:\"{line}\" request"); };

                    string response = null;
                    string log = null;
                    if (!UIMode)
                    {
                        RequestFormat(ip, line, out response, out log, personLocation);
                    }

                    Console.WriteLine(log);
                    if (logPath != null)
                    {
                        WriteLog(log, logPath);
                    }

                    lg = log;
                    if (debug) { Console.WriteLine($"Server sending back:\"{response}\""); };
                    lock (locker)
                    {
                        StreamWriter sw = new StreamWriter(socketStream);
                        sw.WriteLine(response);
                        sw.Flush();
                        if (dbPath != null) { SaveDictionary(personLocation, dbPath); }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Connection faild");
                    Console.WriteLine(e);
                }
                finally
                {
                    socketStream.Close();
                    connection.Close();
                    Console.WriteLine("[Disconnected]");
                }
            }
            /// <summary>
            /// Process the input, creates log and respond message.
            /// </summary>
            /// <param name="personLocation">The database (Dictionary)</param>
            /// <param name="ip">ip address of the client</param>
            /// <param name="line">The input what we want to process</param>
            /// <param name="response">The response what we sending back</param>
            /// <param name="log">The log message of the input</param>
            public void RequestFormat(string ip, string line, out string response, out string log,Dictionary<string,string> personLocation)
            {
                string name = null;
                string location = null;
                response = null;
                bool ched = true;
                log = ip + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'");
                string[] commands = line.Split(" ");
                //GET commands
                if (commands[0] == "GET")
                {
                    if (commands.Length > 2)
                    {
                        //GET HTTP/1.0
                        if (commands[2].Contains("HTTP/1.0"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 2);
                            location = GetLocation(name, personLocation);

                            if (location != null)
                            {
                                response = $"HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                                log += $" \" GET /{name} HTTP/1.0\" OK";
                            }
                            else
                            {
                                response = "HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                                log += $" \" GET /{name} HTTP/1.0\" UNKNOWN";

                            }
                        }
                        //GET HTTP/1.1
                        else if (commands[2].Contains("HTTP/1.1"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 7);
                            location = GetLocation(name, personLocation);

                            if (location != null)
                            {
                                response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                                log += $" \" GET /name={name} HTTP/1.1\" OK";
                            }
                            else
                            {
                                response = "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                                log += $" \" GET /name={name} HTTP/1.1\" UNKNOWN";
                            }
                        }
                    }
                    //GET HTTP/0.9
                    else if (commands.Length == 2)
                    {
                        ched = false;
                        //5th character is the name start
                        name = line.Remove(0, 5);
                        name = name.Remove(name.Length - 2);
                        location = GetLocation(name, personLocation);
                        if (location != null)
                        {
                            response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n";
                            log += $" \" GET /{name}\" OK";
                        }
                        else
                        {
                            response = "HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" GET /{name}\" UNKNOWN";
                        }
                    }
                }
                //PUT HTTP/0.9 
                else if (commands[0] == "PUT" && commands[1].IndexOf("/") == 0 && line.Contains("\r\n\r\n"))
                {
                    ched = false;
                    string[] array = line.Split("\r\n");
                    name = array[0].Remove(0, 5);
                    location = array[array.Length - 2];
                    Console.WriteLine("AddLocation " + location);
                    UpdateAndAdd(name, location, personLocation);

                    response = $"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                    log += $" \" PUT /{name}\" OK";

                }
                else if (commands[0] == "POST")
                {
                    Console.WriteLine("POST");
                    if (commands.Length > 2)
                    {
                        //"HTTP/1.0"
                        if (commands[2].Contains("HTTP/1.0"))
                        {
                            ched = false;
                            name = commands[1].Remove(0, 1);
                            string[] array = line.Split("\r\n");
                            location = array[array.Length - 1];

                            UpdateAndAdd(name, location, personLocation);
                            response = "HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" POST /{name} HTTP/1.0\" OK";
                        }
                        //"HTTP/1.1"
                        else if (commands[2].Contains("HTTP/1.1"))
                        {
                            ched = false;
                            int locationIndex = line.IndexOf("&location=");
                            int nameIndex = line.IndexOf("name=");
                            name = line.Remove(locationIndex);
                            name = name.Remove(0, (5 + nameIndex));

                            location = line.Remove(0, (10 + locationIndex));
                            UpdateAndAdd(name, location, personLocation);
                            response = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                            log += $" \" POST /name={name}&location={location}  HTTP/1.1\" OK";
                        }
                    }
                }
                //whois request GET
                if (commands.Length == 1)
                {
                    name = commands[0];
                    name = name.Remove(name.Length - 2);
                    location = GetLocation(name, personLocation);
                    if (location != null)
                    {
                        response = location;
                        log += $" \" GET {name}\" OK";
                    }
                    else
                    {
                        response = "ERROR: no entries found";
                        log += $" \" GET {name}\" UNKNOWN";
                    }
                }
                //whois request SET
                else if (commands.Length > 1 && ched)
                {
                    name = commands[0];
                    location = commands[1];
                    for (int i = 2; i < commands.Length; i++)
                    {
                        location += " " + commands[i];
                    }
                    location = location.Remove(location.Length - 2);
                    UpdateAndAdd(name, location, personLocation);
                    response = "OK";
                    log += $"\"{name} {location} WHOIS\" OK";
                }
            }
            /// <summary>
            /// If the user already in the database updates its location,
            /// otherwise adds it to the database.
            /// </summary>
            /// <param name="name">User's name</param>
            /// <param name="location">Location of the user</param>
            /// <param name="personLocation">Database</param>
            static void UpdateAndAdd(string name, string location, Dictionary<string, string> personLocation)
            {
                location = location.Trim(new Char[] { '\"', '\'', '`', '\\', '.' });
                if (personLocation.ContainsKey(name))
                {
                    personLocation[name] = location;
                }
                else
                {
                    personLocation.Add(name, location);
                }
            }
            /// <summary>
            /// If the database contains the name of the user, then returns its location,
            /// Otherwise return null,
            /// </summary>
            /// <param name="name">User name in the database</param>
            /// <param name="personLocation">Location of the user</param>
            /// <returns></returns>
            static string GetLocation(string name, Dictionary<string, string> personLocation)
            {

                if (personLocation.ContainsKey(name))
                {
                    string location = personLocation[name];
                    return location;
                }
                else
                {
                    return null;
                }
            }
            /// <summary>
            /// Saves the content of the database to the desired filepath
            /// </summary>
            /// <param name="database">Name of the database</param>
            /// <param name="path">Path of the file</param>
            static void SaveDictionary(Dictionary<string, string> database, string path)
            {
                lock (locker)
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(path);
                        foreach (var entry in database)
                        {
                            sw.WriteLine("{0} {1}", entry.Key, entry.Value);
                        }
                        sw.Close();
                    }
                    catch
                    {
                        Console.WriteLine("Unable to save the database");
                    }
                }
            }
            /// <summary>
            /// Writes the log messages to the file
            /// </summary>
            /// <param name="logMessage">the message</param>
            /// <param name="FilePath">the file path</param>
            static void WriteLog(string logMessage, string FilePath)
            {
                lock (locker)
                {
                    try
                    {
                        StreamWriter sw;
                        sw = File.AppendText(FilePath);
                        sw.WriteLine(logMessage);
                        sw.Close();
                    }
                    catch
                    {
                        Console.WriteLine("Unable to write the Log message");
                    }
                }
            }
        }

    }
}