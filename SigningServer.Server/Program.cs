﻿using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace SigningServer.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            var server = new SigningServerService();
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-install":
                            Console.WriteLine("Installing Windows Service");
                            ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                            return;
                        case "-remove":
                            Console.WriteLine("Removing Windows Service");
                            ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                            return;
                        case "-encode":
                            Console.WriteLine("Original data: " + args[1]);
                            Console.WriteLine("Encrypted data: " + DataProtector.ProtectData(args[1]));
                            return;
                    }
                }

                server.ConsoleStart();
                Console.ReadLine();
            }
            else
            {
                ServiceBase.Run(server);
            }
        }
    }
}
