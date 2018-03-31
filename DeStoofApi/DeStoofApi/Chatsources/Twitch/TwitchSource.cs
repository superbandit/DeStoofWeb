﻿using System;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;

namespace DeStoofApi.Chatsources
{
    public class TwitchSource
    {
        public string userName;
        private string channel;

        private TcpClient _tcpClient;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;

        public TwitchSource(string ip, int port, string userName, string password, string channel)
        {
            try
            {
                this.userName = userName;
                this.channel = channel;

                _tcpClient = new TcpClient(ip, port);
                _inputStream = new StreamReader(_tcpClient.GetStream());
                _outputStream = new StreamWriter(_tcpClient.GetStream());

                // Try to join the channel
                _outputStream.WriteLine("PASS " + password);
                _outputStream.WriteLine("NICK " + userName);
                _outputStream.WriteLine("USER " + userName + " 8 * :" + userName);
                _outputStream.WriteLine("JOIN #" + channel);
                _outputStream.Flush();
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException("Can't connect to: " + channel, ex);
            }
        }

        public void Disconnect()
        {
            _outputStream.WriteLine(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PART #" + channel);
        }

        public void SendIrcMessage(string message)
        {
            try
            {
                _outputStream.WriteLine(message);
                _outputStream.Flush();
            }
            catch (Exception ex)
            {
                //chatWriter.WriteText(ex.Message, true, Color.White);
            }
        }

        public void SendPublicChatMessage(string message)
        {
            try
            {
                SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
            }
            catch (Exception ex)
            {
                //chatWriter.WriteText(ex.Message, true, Color.White);
            }
        }

        public void GetMessages(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (!worker.CancellationPending)
            {
                string message = _inputStream.ReadLine();
                worker.ReportProgress(0, message);
            }
        }
    }
}