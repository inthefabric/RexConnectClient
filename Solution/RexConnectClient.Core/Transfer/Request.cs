using System.Collections.Generic;
using ServiceStack.Text;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	public class Request {

		public static bool CreateRequestsInDebugMode;

		public string ReqId { get; set; }
		public string SessId { get; set; }
		public IList<RequestCmd> CmdList { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public Request(string pRequestId) {
			ReqId = pRequestId;
			CmdList = new List<RequestCmd>();

			if ( CreateRequestsInDebugMode ) {
				CmdList.Add(RequestCmd.CreateConfigCommand(RexConn.ConfigSetting.Debug, "1"));
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public Request(string pRequestId, string pScript, IDictionary<string,string> pParams) :
																					this(pRequestId) {
			CmdList.Add(RequestCmd.CreateQueryCommand(pScript, pParams));
		}

		/*--------------------------------------------------------------------------------------------*/
		public Request(string pRequestId, string pScript) : this(pRequestId, pScript, null) {}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual string ToRequestJson() {
			JsConfig.EmitCamelCaseNames = true;
			string json = JsonSerializer.SerializeToString(this);
			JsConfig.EmitCamelCaseNames = false;
			return json;
		}

	}
	
}