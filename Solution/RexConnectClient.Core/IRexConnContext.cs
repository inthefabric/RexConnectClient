using System;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Core {
	
	/*================================================================================================*/
	public interface IRexConnContext {

		Request Request { get; }
		string HostName { get; }
		int Port { get; }
		bool UseHttp { get; set; }
		RexConnContext.LogFunc Logger { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IResponseResult CreateResponseResult();

		/*--------------------------------------------------------------------------------------------*/
		IRexConnTcp CreateTcpClient();


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		void Log(string pType, string pCategory, string pText, Exception pException=null);

	}

}