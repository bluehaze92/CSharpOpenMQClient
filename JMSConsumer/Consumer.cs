using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JMSProxyLib;
using System.IO;
using System.Threading;
namespace JMSClient
{
	class Consumer
	{
		static String BROKER_HOST = "bvlap108.conoco.net";//"hou-12e74d9341";
		static int BROKER_PORT = 7676;
		static String TOPIC = "example_producerconsumer_dest";
		static String BROKER_USERID = "admin";
		static String BROKER_PASSWORD = "admin";

		public bool MessageHandler(String msg)
		{
			Console.WriteLine("[C# Received]: " + msg.Substring(0, 75));

			StreamWriter wtr = new StreamWriter(BASE_PATH + "Msg_Num_" + (cnt++) + ".xml");
			wtr.Write(msg);
			wtr.Flush();
			wtr.Close();

			return !msg.Equals("END");
		}

		static String BASE_PATH = @".\";
		static int cnt = 1;
		public bool ProcessMessageDelegateAsync(String msg)
		{
			Console.WriteLine("[C# Received]: " + msg.Substring(0,75));

			StreamWriter wtr = new StreamWriter(BASE_PATH+"Msg_Num_"+(cnt++)+".xml");
			wtr.Write(msg);
			wtr.Flush();
			wtr.Close();

			return !msg.Equals("END");
		}

		static void Main(string[] args)
		{
			try
			{
				Consumer p = new Consumer();
				bool doAsync = false;

				if ( args.Length == 1 && !String.IsNullOrEmpty(args[0]) )
				{
					try
					{
						doAsync = Convert.ToBoolean(args[0]);
					}
					catch (FormatException fe)
					{
						Console.WriteLine("Unrecognized boolean value (" + fe.Message + ") : " + args[0]);
					}
				}

				JMSProxy jms = new JMSProxy();
				jms.CreateTopicConnection(BROKER_HOST, BROKER_PORT, TOPIC,
									 BROKER_USERID, BROKER_PASSWORD, doAsync);

				if (doAsync)
				{
					Console.WriteLine("Starting ASYNC Consumer");
					jms.CreateTopicConsumerAsync(p.ProcessMessageDelegateAsync);					
					bool stop = false;
					Console.WriteLine("==>Enter 'q' to quit");
					do
					{						
						stop = Console.Read() == 'q';
					} while (!stop);
				}
				else
				{
					Console.WriteLine("Starting SYNC Consumer");
					jms.CreateTopicConsumerSync();
					Console.WriteLine("==>Send (from producer) 'END' to quit");
					// this is a blocking call so it won't return until we are ready to shutdown
					jms.ConsumeTopicMessages(p.MessageHandler);
				}

				jms.DestroyTopicConsumer();

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
