using System.Collections.Generic;
using ServiceStack.Text;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	public class Request {

		public string ReqId { get; set; }
		public string SessId { get; set; }
		public IList<RequestCmd> CmdList { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public Request() {
			CmdList = new List<RequestCmd>();
		}

		/*--------------------------------------------------------------------------------------------*/
		public Request(string pRequestId) : this() {
			ReqId = pRequestId;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public Request(string pRequestId, string pSessionId) : this(pRequestId) {
			SessId = pSessionId;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual RequestCmd AddQuery(string pScript) {
			return AddCmd(RequestCmd.CreateQuery(pScript));
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual RequestCmd AddQuery(string pScript, IDictionary<string, string> pParams) {
			return AddCmd(RequestCmd.CreateQuery(pScript, pParams));
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual RequestCmd AddSessionAction(RexConn.SessionAction pAction) {
			return AddCmd(RequestCmd.CreateSession(pAction));
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual RequestCmd AddConfigSetting(RexConn.ConfigSetting pSetting, string pValue) {
			return AddCmd(RequestCmd.CreateConfig(pSetting, pValue));
		}

		/*--------------------------------------------------------------------------------------------*/
		private RequestCmd AddCmd(RequestCmd pCmd) {
			CmdList.Add(pCmd);
			return pCmd;
		}

		
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