﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public partial class server : Form
    {
        Socket Socket;
        Socket Accept;
        public server()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _performReceiving = true;
            btnStart.Enabled = false;
            ServerBackgroundWorker.RunWorkerAsync();
        }

        string ShowMsg = "Stopped";
        private void ServerTimer_Tick_1(object sender, EventArgs e)
        {
            lblServerStatus.Text = ShowMsg;
        }

        private bool _performReceiving;
        private void ServerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_performReceiving)
                {
                    try
                    {
                         ShowMsg = "Starting";
                         Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                         Socket.Bind(new IPEndPoint(0, 5656));
                         Socket.Listen(0);

                         Accept = Socket.Accept();
                         Socket.Close();

                    new Thread(() =>
                    {
                        while (true)
                        {
                            byte[] sizeBuf = new byte[4];
                            Accept.Receive(sizeBuf, 0, sizeBuf.Length, 0);
                            int size = BitConverter.ToInt32(sizeBuf, 0);
                            MemoryStream ms = new MemoryStream();
                            while (size > 0)
                            {
                                byte[] buffer;
                                if (size < Accept.ReceiveBufferSize)
                                {
                                    buffer = new byte[size];
                                }
                                else
                                {
                                    buffer = new byte[Accept.ReceiveBufferSize];
                                }

                                int rec = Accept.Receive(buffer, 0, buffer.Length, 0);
                                size -= rec;

                                ms.Write(buffer, 0, buffer.Length);
                            }
                            byte[] data = ms.ToArray();
                            Image img = Image.FromStream(ms);  //exception catched here
                            ms.Close();
                            ms.Dispose();

                            Invoke((MethodInvoker)delegate
                            { 
                               
                                pictureBox1.Image = img;
                                this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                ms.Close();
                                ms.Dispose();
                            });
                        }
                    }).Start();
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _performReceiving = false;
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            _performReceiving = false;
            this.Close();
        }
    }
}