using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JMSProxyLib;
using System.IO;

namespace JMSProducer
{
	class Producer
	{
		static String BROKER_HOST = "hou-12e74d9341";
		static int BROKER_PORT = 7676;
		static String TOPIC = "example_producerconsumer_dest";
		static String BROKER_USERID = "admin";
		static String BROKER_PASSWORD = "admin";

		static void Main(string[] args)
		{
			try
			{
				Producer p = new Producer();
				JMSProxy jms = new JMSProxy();
				jms.CreateTopicConnection(BROKER_HOST, BROKER_PORT, TOPIC,
									 BROKER_USERID, BROKER_PASSWORD, false);
				jms.CreateTopicProducer();

				String msg = Console.ReadLine();
				while (!msg.Equals("END"))
				{
					if (msg.StartsWith("file:"))
					{
						 StreamReader rdr = new StreamReader(msg.Substring(5));
						 msg = rdr.ReadToEnd();
						 rdr.Close();						
					}
					jms.SendMessage(msg);
					msg = Console.ReadLine();
				}

				jms.SendMessage("END");
				jms.DestroyTopicProducer();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
