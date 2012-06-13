using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenMQLib;
using System.Runtime.InteropServices;

namespace JMSProxyLib
{
	public class JMSProxy
	{
		static OpenMQNative.MQHandle propertiesHandle = new OpenMQNative.MQHandle();
		static OpenMQNative.MQHandle connectionHandle = new OpenMQNative.MQHandle();
		static OpenMQNative.MQHandle sessionHandle = new OpenMQNative.MQHandle();
		static OpenMQNative.MQHandle destinationHandle = new OpenMQNative.MQHandle();
		static OpenMQNative.MQHandle producer_consumer_Handle = new OpenMQNative.MQHandle();
		static OpenMQNative.MQHandle textMessageHandle = new OpenMQNative.MQHandle();

		// delegates for external class to use
		public delegate bool ProcessMessageDelegate(String message);
		public delegate bool ProcessMessageDelegateAsync(String message);

		// local instance vars for async delegate
		ProcessMessageDelegateAsync asyncDelegate = null;
		OpenMQProxy.MessageReceived msgDlg = null;

		OpenMQProxy mqProxy = new OpenMQProxy();

		public void CreateTopicConnection(String host, Int32 port,
			String destinationName, String userid, String password, bool isAsync)
		{
			propertiesHandle.init();
			connectionHandle.init();
			sessionHandle.init();
			destinationHandle.init();
			
			try
			{
				mqProxy.CreateProperties(ref propertiesHandle);

				mqProxy.SetBrokerHost(propertiesHandle, host);
				mqProxy.SetBrokerPort(propertiesHandle, port);
				mqProxy.SetConnectionType(propertiesHandle, "TCP");

			mqProxy.CreateConnection(propertiesHandle, userid, password,ref connectionHandle);

			if ( isAsync)
				mqProxy.CreateAsyncSession(connectionHandle, ref sessionHandle);
			else
				mqProxy.CreateSyncSession(connectionHandle, ref sessionHandle);

			mqProxy.CreateTopicDestination(sessionHandle, destinationName, ref destinationHandle);
			
			}
			catch (OpenMQException ex)
			{
				DestroyTopicConnection();
				Console.WriteLine(ex);
			}

		}

		public void CreateTopicConsumerSync()
		{
			try
			{
				producer_consumer_Handle.init();
				mqProxy.CreateSyncMessageConsumer(sessionHandle, destinationHandle, 
					ref producer_consumer_Handle);
				mqProxy.StartConnection(connectionHandle, destinationHandle);
			}
			catch (OpenMQException ex)
			{
				DestroyTopicConnection();
				Console.WriteLine(ex);
			}
		}


		public void ForwardMessageToClient(String textMessage)
		{
			// forward message to client
			asyncDelegate(textMessage);
		}
	

		public void CreateTopicConsumerAsync(ProcessMessageDelegateAsync msgDelegate)
		{
			try
			{
				producer_consumer_Handle.init();
				textMessageHandle.init();
				mqProxy.CreateTextMessageHandle(ref textMessageHandle);

				// setup local delegate back to C# client
				asyncDelegate = new ProcessMessageDelegateAsync(msgDelegate);

				// setup callback from proxy
				msgDlg = new OpenMQProxy.MessageReceived(ForwardMessageToClient);

				mqProxy.CreateAsyncMessageConsumer(sessionHandle, destinationHandle,
					msgDlg, ref producer_consumer_Handle);
				mqProxy.StartConnection(connectionHandle, destinationHandle);
			}
			catch (OpenMQException ex)
			{
				DestroyTopicConnection();
				Console.WriteLine(ex);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				DestroyTopicConnection();
			}
		}


		public void ConsumeTopicMessages(ProcessMessageDelegate msgHandler)
		{
			try
			{
				bool keepReading = true;
				do
				{
					StringBuilder messageText = new StringBuilder();
					mqProxy.ReceiveMessageWithWait(producer_consumer_Handle, sessionHandle, 
						ref messageText);
					keepReading = msgHandler(messageText.ToString());
				} while (keepReading);								
			}
			catch (OpenMQException ex)
			{
				DestroyTopicConnection();
				Console.WriteLine(ex);
			}
		}//ConsumeTopicMessage

		public void CreateTopicProducer()
		{
			try
			{
				textMessageHandle.init();
				mqProxy.CreateMessageProducer(sessionHandle, destinationHandle, 
					ref producer_consumer_Handle);
				mqProxy.FreeDestination(destinationHandle);
				mqProxy.CreateTextMessageHandle(ref textMessageHandle);
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
			}
		}// CreateTopicProducer

		public void SendMessage(String messageText)
		{
			try
			{
				mqProxy.SendMessageText(textMessageHandle, producer_consumer_Handle, messageText);
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
			}
		}

		public void DestroyTopicConsumer()
		{
			try
			{
				mqProxy.DestroyMessageConsumer(producer_consumer_Handle);
				DestroyTopicConnection();
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
			}			
		}

		public void DestroyTopicProducer()
		{
			try
			{
				mqProxy.DestroyMessageHandle(textMessageHandle);
				mqProxy.DestroyMessageProducer(producer_consumer_Handle);				
				DestroyTopicConnection();
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
			}
		}


		public void DestroyTopicConnection()
		{
			try
			{
				mqProxy.DestroySyncSession(sessionHandle);
				mqProxy.StopConnection(connectionHandle);
				mqProxy.DestroyConnection(connectionHandle);
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
			}			
		}// DestroyTopicConnection
	}
}
