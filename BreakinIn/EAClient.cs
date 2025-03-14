﻿using BreakinIn.Messages;
using BreakinIn.Model;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BreakinIn
{
	public class EAClient
	{
		public AbstractEAServer Context;
		public User User;
		public string IP = "127.0.0.1";
		public string Port = null;
		public int SessionID;

		private int ExpectedBytes = -1;
		private bool InHeader;
		private TcpClient Client;
		private NetworkStream Stream;
		private Thread RecvThread;
		private byte[] TempData = null;
		private int TempDatOff;
		private string CommandName = "null";

		public long PingSendTick;
		public int Ping;

		private static int MAX_SIZE = 1024 * 1024 * 2;

		public EAClient(AbstractEAServer context, TcpClient client)
		{
			Context = context;
			Client = client;
			IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

			Console.WriteLine("New connection from " + IP + ".");

			RecvThread = new Thread(RunLoop);
			RecvThread.Start();
		}

		public void Close()
		{
			Client.Close();
		}

		private void RunLoop()
		{
			Stream = Client.GetStream();
			var bytes = new Byte[65536];

			int len;
			try
			{
				while ((len = Stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					var off = 0;
					while (len > 0)
					{
						//got some data
						if (ExpectedBytes == -1)
						{
							//new packet
							InHeader = true;
							ExpectedBytes = 12; //header
							TempData = new byte[12];
							TempDatOff = 0;
						}

						var copyLen = Math.Min(len, TempData.Length - TempDatOff);
						Array.Copy(bytes, off, TempData, TempDatOff, copyLen);
						off += copyLen;
						TempDatOff += copyLen;
						len -= copyLen;

						if (TempDatOff == TempData.Length)
						{
							if (InHeader)
							{
								//header complete.
								InHeader = false;
								var size = TempData[11] | (TempData[10] << 8) | (TempData[9] << 16) | (TempData[8] << 24);
								if (size > MAX_SIZE)
								{
									Client.Close(); //either something terrible happened or they're trying to mess with us
									break;
								}
								CommandName = ASCIIEncoding.ASCII.GetString(TempData).Substring(0, 4);

								TempData = new byte[size - 12];
								TempDatOff = 0;
							}
							else
							{
								//message complete
								GotMessage(CommandName, TempData);

								TempDatOff = 0;
								ExpectedBytes = -1;
								TempData = null;
							}
						}
					}
				}
			} catch (Exception)
			{
			}
			
			Stream.Dispose();
			Client.Dispose();
			Console.WriteLine("User "+IP+" Disconnected.");
			Context.RemoveClient(this);
		}

		private void GotMessage(string name, byte[] data)
		{
			Task.Run(() =>
			{
				Context.HandleMessage(name, data, this);
			});
		}

		public void SendMessage(byte[] data)
		{
			try
			{
				Stream.Write(data);
			}
			catch
			{
				//something bad happened :(
			}
		}

		public void SendMessage(AbstractMessage msg)
		{
			try
			{
				Stream.Write(msg.GetData());
			} catch
			{
				//something bad happened :(
			}
		}

		public bool HasAuth()
		{
			return User != null;
		}
	}
}