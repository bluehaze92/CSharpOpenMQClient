using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace OpenMQLib
{
	public static class OpenMQNative
	{
		public static UInt32 MQ_INVALID_HANDLE = 0xFEEEFEEE;
		public static UInt32 MQ_TRUE = 1;
		public static UInt32 MQ_FALSE = 0;
		public static UInt32 MQ_SUCCESS = 0;
		public static UInt32 MQ_CALLBACK_RUNTIME_ERROR = 3600;
		public static UInt32 MQ_OK = MQ_SUCCESS;
		public static String MQ_BROKER_HOST_PROPERTY = "MQBrokerHostName";
		public static String MQ_BROKER_PORT_PROPERTY = "MQBrokerHostPort";
		public static String MQ_CONNECTION_TYPE_PROPERTY = "MQConnectionType";
		public static Int32 MQ_QUEUE_DESTINATION = 0;
		public static Int32 MQ_TOPIC_DESTINATION = 1;
		public static Int32 MQ_SESSION_SYNC_RECEIVE = 0;
		public static Int32 MQ_SESSION_ASYNC_RECEIVE = 1;
		public static Int32 MQ_AUTO_ACKNOWLEDGE = 1;
		public static Int32 MQ_CLIENT_ACKNOWLEDGE = 2;
		public static Int32 MQ_DUPS_OK_ACKNOWLEDGE = 3;
		public static Int32 MQ_SESSION_TRANSACTED = 0;
		
		// Without this attribute, the async consumer example will work once and then crash
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate UInt32 MQMessageListenerDelegate(MQHandle sessionHandle,
								 MQHandle consumerHandle,
								 MQHandle messageHandle,
								 IntPtr callbackData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void MQConnectionExceptionListenerDelegate(
						  MQHandle connectionHandle,
						  MQStatus exception,
						  IntPtr callbackData);

		public struct MQStatus
		{
			public UInt32 errorCode;
		}

		public struct MQHandle
		{
			public UInt32 handle;
			public void init()
			{
				this.handle = MQ_INVALID_HANDLE;
			}
		}
		internal static Boolean MQ_ERR_CHK(MQStatus mqCall)
		{
			if (mqCall.errorCode != MQ_SUCCESS)
			{
				String errMsg = MQGetStatusString(mqCall);
				String trace = MQGetErrorTrace();
				Console.WriteLine(trace);
				MQFreeString(trace);
				MQFreeString(errMsg);
			}

			return mqCall.errorCode != MQ_SUCCESS;
		}

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern String MQGetErrorTrace();

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQFreeString(String statusString);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateProperties(ref MQHandle propertiesHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQSetStringProperty(MQHandle propertiesHandle,
			String key, String value);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQSetInt32Property(MQHandle propertiesHandle,
						   String key, Int32 value);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateConnection(MQHandle propertiesHandle,
			String userid, String password, String clientID,
			MQConnectionExceptionListenerDelegate exceptionListener,
			IntPtr listenerCallBackData,
			ref MQHandle connectionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateSession(MQHandle connectionHandle,
			Int32 isTransacted, Int32 ackMode,
			Int32 recMode, ref MQHandle sessionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateDestination(MQHandle sessionHandle,
			String destinationName, Int32 destinationType, ref MQHandle destinationHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateMessageConsumer(MQHandle sessionHandle,
			MQHandle destinationHandle, String messageSelector, Int32 noLocal, ref MQHandle consumerHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQFreeDestination(MQHandle destinationHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQStartConnection(MQHandle connectionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQReceiveMessageWait(MQHandle consumerHandle,
			ref MQHandle messageHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQGetTextMessageText(MQHandle messageHandle,
				   ref StringBuilder messageText);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQAcknowledgeMessages(MQHandle sessionHandle,
				  MQHandle messageHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQFreeMessage(MQHandle messageHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCloseMessageConsumer(MQHandle consumerHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCloseSession(MQHandle sessionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCloseConnection(MQHandle connectionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQFreeConnection(MQHandle connectionHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQFreeProperties(MQHandle propertiesHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateMessageProducerForDestination(MQHandle sessionHandle,
			MQHandle destinationHandle, ref MQHandle producerHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateTextMessage(ref MQHandle messageHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQSetTextMessageText(MQHandle messageHandle, String messageText);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQSendMessage(MQHandle producerHandle, MQHandle messageHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCloseMessageProducer(MQHandle producerHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern MQStatus MQCreateAsyncMessageConsumer(MQHandle sessionHandle,
					    MQHandle destinationHandle,
					    String messageSelector,
					    Int32 noLocal,
					    MQMessageListenerDelegate messageListener,
					    IntPtr messageListenerCallbackData,
					    ref MQHandle consumerHandle);

		[DllImport(@"mqcrt1.dll", CallingConvention = CallingConvention.Cdecl)]
		internal static extern String MQGetStatusString(MQStatus status);


	}
}
