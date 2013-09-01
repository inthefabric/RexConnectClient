using System;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;
using RexConnectClient.Core.Cache;

namespace RexConnectClient.Core {
	
	/*================================================================================================*/
	public interface IRexConnContext {

		Request Request { get; }
		string HostName { get; }
		int Port { get; }
		bool UseHttp { get; }
		string GraphName { get; }
		RexConnContext.LogFunc Logger { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IResponseResult CreateResponseResult();

		/*--------------------------------------------------------------------------------------------*/
		IRexConnTcp CreateTcpClient();
		
		/*--------------------------------------------------------------------------------------------*/
		void SetHttpMode(bool pUseHttp, string pGraphName);
		
		/*--------------------------------------------------------------------------------------------*/
		void SetCacheProvider(IRexConnCacheProvider pCacheProvider);
		
		/*--------------------------------------------------------------------------------------------*/
		IRexConnCache Cache { get; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		void Log(string pType, string pCategory, string pText, Exception pException=null);

	}

}