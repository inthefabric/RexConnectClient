using System;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Core {
	
	/*================================================================================================*/
	public class RexConnContext : IRexConnContext {

		public Request Request { get; private set; }
		public string HostName { get; private set; }
		public int Port { get; private set; }
		public LogFunc Logger { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RexConnContext(Request pRequest, string pHostName, int pPort) {
			Request = pRequest;
			HostName = pHostName;
			Port = pPort;

			Logger = ((level, cat, text, ex) =>
				Console.WriteLine(level+" | "+cat+" | "+text+(ex == null ? "" : " | "+ex))
			);
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual IResponseResult CreateResponseResult() {
			return new ResponseResult(this);
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual IRexConnTcp CreateTcpClient() {
			var tcp = new RexConnTcp(HostName, Port);
			tcp.SendBufferSize = tcp.ReceiveBufferSize = 1<<16;
			return tcp;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public delegate void LogFunc(string pLevel, string pCategory, string pText, Exception pEx=null);

		/*--------------------------------------------------------------------------------------------*/
		public virtual void Log(string pLevel, string pCategory, string pText, Exception pEx=null) {
			Logger(pLevel, pCategory, pText, pEx);
		}

	}

}