﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Security;
using Newtonsoft.Json;
using NLog;

namespace SigningServer.Client
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: SigningServer.Client.exe [--config File] Source1 [Source2 Source3 ...]");
                return;
            }

            string configFile = null;
            if (args.Length > 2)
            {
                if (args[0] == "--config")
                {
                    configFile = args[1];
                    args = args.Skip(2).ToArray();
                }
            }


            if (string.IsNullOrEmpty(configFile))
            {
                configFile = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName,
                    "config.json");
            }

            if (!File.Exists(configFile))
            {
                Log.Fatal("Could not find config.json beside executable");
                return;
            }

            SigningClientConfiguration configuration;
            try
            {
                Log.Info("Loading config");
                configuration = JsonConvert.DeserializeObject<SigningClientConfiguration>(File.ReadAllText(configFile));
                if (configuration.Retry == 0)
                {
                    configuration.Retry = 3;
                }
                Log.Info("Configuration loaded");
            }
            catch (Exception e)
            {
                Log.Error(e, "Config could not be loaded");
                Environment.ExitCode = ErrorCodes.InvalidConfiguration;
                return;
            }

            SigningClient client;
            try
            {
                Log.Info("Creating client");
                client = new SigningClient(configuration);
                Log.Info("connected to server");
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not create signing client");
                Environment.ExitCode = ErrorCodes.CommunicationError;
                return;
            }

            try
            {
                foreach (var arg in args)
                {
                    client.SignFile(arg);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Environment.ExitCode = ErrorCodes.Unauthorized;
            }
            catch (SecurityNegotiationException)
            {
                Environment.ExitCode = ErrorCodes.SecurityNegotiationFailed;
            }
            catch (UnsupportedFileFormatException)
            {
                Environment.ExitCode = ErrorCodes.UnsupportedFileFormat;
            }
            catch (FileAlreadySignedException)
            {
                Environment.ExitCode = ErrorCodes.FileAlreadySigned;
            }
            catch (FileNotFoundException)
            {
                Environment.ExitCode = ErrorCodes.FileNotFound;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Unexpected error happened");
                Environment.ExitCode = ErrorCodes.UnexpectedError;
            }
        }
    }

    internal static class ErrorCodes
    {
        public const int UnexpectedError = 1;
        public const int FileNotFound = 2;
        public const int FileAlreadySigned = 3;
        public const int UnsupportedFileFormat = 4;
        public const int Unauthorized = 5;
        public const int InvalidConfiguration = 6;
        public const int CommunicationError = 7;
        public const int SecurityNegotiationFailed = 8;
    }

    public class SigningClientConfiguration
    {
        public string SigningServer { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool OverwriteSignatures { get; set; }
        public bool IgnoreExistingSignatures { get; set; }
        public bool IgnoreUnsupportedFiles { get; set; }
        public int Timeout { get; set; }
        public string HashAlgorithm { get; set; }
        public int Retry { get; set; }
    }
}
