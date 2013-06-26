using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RexConnectClient.Core.Result;

namespace RexConnectClient.Core {

	/*================================================================================================*/
	public class RexConnDataAccess : IRexConnDataAccess {

		public IRexConnContext Context { get; private set; }
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RexConnDataAccess(IRexConnContext pContext) {
			Context = pContext;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public IResponseResult Execute() {
			var sw = Stopwatch.StartNew();
			Exception unhandled = null;

			IResponseResult result = Context.CreateResponseResult();

			try {
				Context.Log("Debug", "Request", result.RequestJson);
				GetRawResult(result);
			}
			catch ( WebException we ) {
				unhandled = we;
				result.SetResponseError(we+"");
				Stream s = (we.Response == null ? null : we.Response.GetResponseStream());

				if ( s != null ) {
					var sr = new StreamReader(s);
					Context.Log("Error", "WebException", sr.ReadToEnd());
				}
			}
			catch ( Exception e ) {
				unhandled = e;
				Context.Log("Error", "Unhandled", "Raw result: "+result.ResponseJson);
				result.SetResponseError(e+"");
			}

			result.ExecutionMilliseconds = (int)sw.ElapsedMilliseconds;

			if ( unhandled != null ) {
				throw new Exception("Unhandled exception:\nRequestJson = "+
					result.RequestJson+"\nResponseJson = "+result.ResponseJson, unhandled);
			}

			return result;
		}

		/*--------------------------------------------------------------------------------------------*/
		protected virtual void GetRawResult(IResponseResult pResult) {
			IRexConnTcp tcp = pResult.Context.CreateTcpClient();

			int len = IPAddress.HostToNetworkOrder(pResult.RequestJson.Length);
			byte[] dataLen = BitConverter.GetBytes(len);
			byte[] data = Encoding.ASCII.GetBytes(pResult.RequestJson);

			//stream the request's string length, then the string itself

			NetworkStream stream = tcp.GetStream();
			stream.Write(dataLen, 0, dataLen.Length);
			stream.Write(data, 0, data.Length);

			//Get string length from the first four bytes

			data = new byte[4];
			stream.Read(data, 0, data.Length);
			Array.Reverse(data);
			int respLen = BitConverter.ToInt32(data, 0);

			//Get response string using the string length

			var sb = new StringBuilder(respLen);

			while ( sb.Length < respLen ) {
				data = new byte[respLen];
				int bytes = stream.Read(data, 0, data.Length);
				sb.Append(Encoding.ASCII.GetString(data, 0, bytes));
			}

			string resp = sb.ToString();
			pResult.Context.Log("Debug", "Result", resp);
			pResult.SetResponseJson(resp);
		}

	}

}