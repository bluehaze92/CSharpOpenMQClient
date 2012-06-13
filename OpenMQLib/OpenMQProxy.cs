using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace OpenMQLib
{
	public class OpenMQException : Exception
	{		
		public OpenMQException(String msg) : base(msg)
		{			
		}
	}

	public class OpenMQProxy
	{

		public delegate void MessageReceived(String textMessage);

		MessageReceived msgRecDelegate = null;

		#region C# API

		public void CreateProperties(ref OpenMQNative.MQHandle propertiesHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateProperties(ref propertiesHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating properties handle " + res.errorCode);
		}

		public void SetBrokerHost(OpenMQNative.MQHandle propertiesHandle, string host)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQSetStringProperty(propertiesHandle,
				   OpenMQNative.MQ_BROKER_HOST_PROPERTY, host);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error setting Broker Host property " + res.errorCode);
		}

		public void SetBrokerPort(OpenMQNative.MQHandle propertiesHandle, int port)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQSetInt32Property(propertiesHandle,
				   OpenMQNative.MQ_BROKER_PORT_PROPERTY, port);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error setting Broker Port property " + res.errorCode);
		}

		public void SetConnectionType(OpenMQNative.MQHandle propertiesHandle, string type)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQSetStringProperty(propertiesHandle,
				   OpenMQNative.MQ_CONNECTION_TYPE_PROPERTY, type);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error setting Connection type property " + res.errorCode);
		}

		public void AsyncConnectionCallback(
				  OpenMQNative.MQHandle connectionHandle,
				  OpenMQNative.MQStatus exception,
				  IntPtr callbackData)
		{
			Console.WriteLine("Connection Callback : " + exception.errorCode);
		}

		public void CreateConnection(OpenMQNative.MQHandle propertiesHandle, 
			                        string userid, string password,							    
							    ref OpenMQNative.MQHandle connectionHandle)
		{
			OpenMQNative.MQConnectionExceptionListenerDelegate connectionDelegate =
				new OpenMQNative.MQConnectionExceptionListenerDelegate(AsyncConnectionCallback);

			OpenMQNative.MQStatus res = OpenMQNative.MQCreateConnection(propertiesHandle,
				userid, password, null, connectionDelegate, IntPtr.Zero, ref connectionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating connection " + res.errorCode);
		}

		public void CreateSyncSession(OpenMQNative.MQHandle connectionHandle,
			ref OpenMQNative.MQHandle sessionHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateSession(connectionHandle, 
				(Int32) OpenMQNative.MQ_FALSE,
				OpenMQNative.MQ_CLIENT_ACKNOWLEDGE,
				OpenMQNative.MQ_SESSION_SYNC_RECEIVE, ref sessionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating session handle " + res.errorCode);
		}

		public void CreateAsyncSession(OpenMQNative.MQHandle connectionHandle,
			ref OpenMQNative.MQHandle sessionHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateSession(connectionHandle,
				(Int32)OpenMQNative.MQ_FALSE,
				OpenMQNative.MQ_CLIENT_ACKNOWLEDGE,
				OpenMQNative.MQ_SESSION_ASYNC_RECEIVE, ref sessionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating session handle " + res.errorCode);
		}


		public void CreateTopicDestination(OpenMQNative.MQHandle sessionHandle, string destinationName,
			ref OpenMQNative.MQHandle destinationHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateDestination(sessionHandle, destinationName,
						    OpenMQNative.MQ_TOPIC_DESTINATION, ref destinationHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating topic destination "+res.errorCode);
		}

		public void CreateSyncMessageConsumer(OpenMQNative.MQHandle sessionHandle,
			OpenMQNative.MQHandle destinationHandle, 
			    ref OpenMQNative.MQHandle consumerHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateMessageConsumer(sessionHandle, destinationHandle,
							   null, (Int32) OpenMQNative.MQ_TRUE, ref consumerHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating Sync Message Consumer " + res.errorCode);
		}

		public UInt32 AsyncMessageCallback(OpenMQNative.MQHandle sessionHandle,
						  OpenMQNative.MQHandle consumerHandle,
						  OpenMQNative.MQHandle messageHandle,
			IntPtr callbackData)
		{
			try
			{
				StringBuilder messageText = new StringBuilder();
				this.ReceiveMessage(consumerHandle, sessionHandle,
						messageHandle, ref messageText);
				msgRecDelegate(messageText.ToString());
				return OpenMQNative.MQ_SUCCESS;
			}
			catch (OpenMQException ex)
			{
				Console.WriteLine(ex);
				return OpenMQNative.MQ_CALLBACK_RUNTIME_ERROR;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return OpenMQNative.MQ_CALLBACK_RUNTIME_ERROR;
			}

		}

		public void CreateAsyncMessageConsumer(OpenMQNative.MQHandle sessionHandle,
			OpenMQNative.MQHandle destinationHandle,
			MessageReceived MsgRecCallback, ref OpenMQNative.MQHandle consumerHandle)
		{
			try
			{
				msgRecDelegate = new MessageReceived(MsgRecCallback);

				OpenMQNative.MQMessageListenerDelegate callback =
					new OpenMQNative.MQMessageListenerDelegate(AsyncMessageCallback);

				OpenMQNative.MQStatus res = OpenMQNative.MQCreateAsyncMessageConsumer(sessionHandle,
					destinationHandle, null, (Int32)OpenMQNative.MQ_FALSE, callback,
					IntPtr.Zero,
					ref consumerHandle);
				if (OpenMQNative.MQ_ERR_CHK(res))
					throw new OpenMQException("Error creating Async Message Consumer " + res.errorCode);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				throw new OpenMQException(e.Message);
			}
		}
		
		public void StartConnection(OpenMQNative.MQHandle connectionHandle,
			OpenMQNative.MQHandle destinationHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQFreeDestination(destinationHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error freeing destination " + res.errorCode);
			
			res = OpenMQNative.MQStartConnection(connectionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error starting connection " + res.errorCode);
		}


		public void ReceiveMessageWithWait(OpenMQNative.MQHandle consumerHandle, 
			OpenMQNative.MQHandle sessionHandle,
			ref StringBuilder messageText)
		{
			OpenMQNative.MQHandle messageHandle = new OpenMQNative.MQHandle();
			messageHandle.init();

			OpenMQNative.MQStatus res = OpenMQNative.MQReceiveMessageWait(consumerHandle, ref messageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error waitig for message " + res.errorCode);

			res = OpenMQNative.MQGetTextMessageText(messageHandle, ref messageText);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error retreiving message text " + res.errorCode);

			OpenMQNative.MQAcknowledgeMessages(sessionHandle, messageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error send message Ack" + res.errorCode);

			OpenMQNative.MQFreeMessage(messageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error freeing message handle" + res.errorCode);

		}

		public void ReceiveMessage(OpenMQNative.MQHandle consumerHandle, OpenMQNative.MQHandle sessionHandle, 
			OpenMQNative.MQHandle messageHandle, ref StringBuilder messageText)
		{
			try
			{

				OpenMQNative.MQStatus res = OpenMQNative.MQGetTextMessageText(messageHandle, ref messageText);
				if (OpenMQNative.MQ_ERR_CHK(res))
					throw new OpenMQException("Error retreiving message text " + res.errorCode);

				OpenMQNative.MQAcknowledgeMessages(sessionHandle, messageHandle);
				if (OpenMQNative.MQ_ERR_CHK(res))
					throw new OpenMQException("Error send message Ack" + res.errorCode);

				OpenMQNative.MQFreeMessage(messageHandle);
				if (OpenMQNative.MQ_ERR_CHK(res))
					throw new OpenMQException("Error freeing message handle" + res.errorCode);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				throw new OpenMQException(e.Message);
			}			
		}


		public void DestroyMessageConsumer(OpenMQNative.MQHandle consumerHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCloseMessageConsumer(consumerHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying message consumer " + res.errorCode);
		}

		public void DestroySyncSession(OpenMQNative.MQHandle sessionHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCloseSession(sessionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying session "+res.errorCode);
		}

		public void StopConnection(OpenMQNative.MQHandle connectionHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCloseConnection(connectionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error closing connection " + res.errorCode);
		}

		public void DestroyConnection(OpenMQNative.MQHandle connectionHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQFreeConnection(connectionHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying connection " + res.errorCode);
		}
		public void DestroyProperties(OpenMQNative.MQHandle propertiesHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQFreeProperties(propertiesHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying properties " + res.errorCode);
		}		

		public void CreateMessageProducer(OpenMQNative.MQHandle sessionHandle, 
			OpenMQNative.MQHandle destinationHandle, 
			ref OpenMQNative.MQHandle producer_consumer_Handle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateMessageProducerForDestination(sessionHandle,
						    destinationHandle, ref producer_consumer_Handle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating messate producer " + res.errorCode);
		}

		public void CreateTextMessageHandle(ref OpenMQNative.MQHandle textMessageHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCreateTextMessage(ref textMessageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error creating text message handler "+res.errorCode);
		}

		public void FreeDestination(OpenMQNative.MQHandle destinationHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQFreeDestination(destinationHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error freeing destination " + res.errorCode);
		}


		public void SendMessageText(OpenMQNative.MQHandle textMessageHandle, 
			OpenMQNative.MQHandle producer_consumer_Handle, 
			string messageText)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQSetTextMessageText(textMessageHandle, messageText);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error setting message text " + res.errorCode);

			res = OpenMQNative.MQSendMessage(producer_consumer_Handle, textMessageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error sending message " + res.errorCode);
		}

		public void DestroyMessageHandle(OpenMQNative.MQHandle textMessageHandle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQFreeMessage(textMessageHandle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying message handle " + res.errorCode);

		}

		public void DestroyMessageProducer(OpenMQNative.MQHandle producer_consumer_Handle)
		{
			OpenMQNative.MQStatus res = OpenMQNative.MQCloseMessageProducer(producer_consumer_Handle);
			if (OpenMQNative.MQ_ERR_CHK(res))
				throw new OpenMQException("Error destroying message producer " + res.errorCode);

		}
		#endregion

	}// class

}
