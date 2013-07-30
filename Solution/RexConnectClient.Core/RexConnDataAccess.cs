using System;
using System.Collections.Specialized;
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
			ResponseErrException respErrEx = null;

			IResponseResult result = Context.CreateResponseResult();

			try {
				Context.Log("Debug", "Request", result.RequestJson);
				GetRawResult(result);
			}
			catch ( ResponseErrException re ) {
				respErrEx = re;
				Context.Log("Error", "ResponseErrException", re.Message);
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

			sw.Stop();
			result.ExecutionMilliseconds = (int)sw.Elapsed.TotalMilliseconds;

			if ( respErrEx != null ) {
				throw respErrEx;
			}
			
			if ( unhandled != null ) {
				throw new Exception("Unhandled exception:\nRequestJson = "+
					result.RequestJson+"\nResponseJson = "+result.ResponseJson, unhandled);
			}

			return result;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public string ExecuteRaw() {
			IResponseResult result = Context.CreateResponseResult();
			Context.Log("Debug", "RequestRaw", result.RequestJson);
			return GetRawResult(result, false);
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		protected virtual string GetRawResult(IResponseResult pResult, bool pParse=true) {
			string resp = (pResult.Context.UseHttp ?
				GetRawResultHttp(pResult) : GetRawResultTcp(pResult));

			pResult.Context.Log("Debug", "Result", resp);

			if ( pParse ) {
				pResult.SetResponseJson(resp);
			}

			return resp;
		}

		/*--------------------------------------------------------------------------------------------*/
		protected virtual string GetRawResultTcp(IResponseResult pResult) {
			IRexConnTcp tcp = pResult.Context.CreateTcpClient();

			int len = IPAddress.HostToNetworkOrder(pResult.RequestJson.Length);
			byte[] dataLen = BitConverter.GetBytes(len);
			byte[] data = Encoding.UTF8.GetBytes(pResult.RequestJson);

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
			
			var buf = new char[respLen];
			new StreamReader(stream).Read(buf, 0, respLen);
			return new string(buf);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		protected virtual string GetRawResultHttp(IResponseResult pResult) {
			string url = "http://"+pResult.Context.HostName+":"+pResult.Context.Port+
				"/graphs/graph/fabric/rexconnect";
			
			using ( var wc = new WebClient() ) {
				var vals = new NameValueCollection();
				vals["req"] = pResult.RequestJson;

				byte[] bytes = wc.UploadValues(url, "POST", vals);
				return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
			}
		}

	}

}