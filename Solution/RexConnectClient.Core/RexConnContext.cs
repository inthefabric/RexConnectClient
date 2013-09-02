using System;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;
using RexConnectClient.Core.Cache;

namespace RexConnectClient.Core {
	
	/*================================================================================================*/
	public class RexConnContext : IRexConnContext {

		public Request Request { get; private set; }
		public string HostName { get; private set; }
		public int Port { get; private set; }
		public bool UseHttp { get; private set; }
		public string GraphName { get; private set; }
		public LogFunc Logger { get; set; }
		
		private IRexConnCacheProvider vCacheProv;
		

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

		/*--------------------------------------------------------------------------------------------*/
		public virtual void SetHttpMode(bool pUseHttp, string pGraphName) {
			UseHttp = pUseHttp;
			GraphName = pGraphName;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public virtual void SetCacheProvider(IRexConnCacheProvider pCacheProvider) {
			vCacheProv = pCacheProvider;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public IRexConnCache Cache {
			get {
				if ( vCacheProv == null ) {
					throw new NullReferenceException("The CacheProvider was not set.");
				}
				
				return vCacheProv.GetCache(HostName, Port);
			}
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