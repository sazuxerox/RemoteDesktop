using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public partial class client : Form
    {
        private static Bitmap _bmpScreenshot;
        private static Graphics _gfxScreenshot;
        Socket _sock;
        int _sent;


        public client()
        {
            InitializeComponent();
        }

        private bool _performCapturing;
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            StartCapturing();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopAllCapturing();
        }

        private void StartCapturing()
        {
            _showMsg = "Starting...";
            _performCapturing = true;
            ClientBackgroundWorker.RunWorkerAsync();
        }

        private void StopAllCapturing()
        {
            _showMsg = "Stopping...";
            _performCapturing = false;
            this.Close();
        }

        string _showMsg = "Idle";
        private void ClientTimer_Tick_1(object sender, EventArgs e)
        {
            label2.Text = _showMsg;
        }

        private void ClientBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (_performCapturing)
                {
                    _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        _sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5656));
                    }
                    catch
                    {
                        MessageBox.Show(@"Unable to connect");
                    }

                    _bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, 
                        Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                    _gfxScreenshot = Graphics.FromImage(_bmpScreenshot);
                    _gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                        Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size,
                        CopyPixelOperation.SourceCopy);

                    var ms = new MemoryStream();
                    _bmpScreenshot.Save(ms, ImageFormat.Png);
                    var bw = new BinaryWriter(ms);
                    var bmpbytes = ms.ToArray();
                    _bmpScreenshot.Dispose();
                    ms.Close();

                    _sent = SendData(_sock, bmpbytes);
                    _sock.Close();
                }
            }
         }

        private int SendData(Socket sock, byte[] bmpbytes)
        {
            int total = 0;
            int size = bmpbytes.Length;
            int dataLeft = size;
            int sent;

            byte[] dataSize = new byte[4];
            dataSize = BitConverter.GetBytes(size);
            sent = sock.Send(dataSize);
            while (total < size)
            {
                sent = sock.Send(bmpbytes, total, dataLeft, SocketFlags.None);
                total += sent;
                dataLeft -= sent;
            }
            return total;
        }
        }
    }