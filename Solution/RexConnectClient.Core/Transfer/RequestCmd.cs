using System.Collections.Generic;
using System.Text;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	public class RequestCmd {

		public string Cmd { get; set; }
		public IList<string> Args { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RequestCmd(string pCommand, params string[] pArgs) {
			Cmd = pCommand;
			Args = new List<string>(pArgs);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public static RequestCmd CreateQueryCommand(string pScript) {
			return new RequestCmd(RexConn.Command.Query.ToString().ToLower(), JsonUnquote(pScript));
		}

		/*--------------------------------------------------------------------------------------------*/
		public static RequestCmd CreateQueryCommand(string pScript, IDictionary<string,string> pParams){
			string q = JsonUnquote(pScript);
			var sb = new StringBuilder();

			foreach ( string key in pParams.Keys ) {
				sb.Append(
					(sb.Length > 0 ? "," : "")+
					"\""+JsonUnquote(key)+"\":"+
					"\""+JsonUnquote(pParams[key])+"\""
				);
			}

			return new RequestCmd(RexConn.Command.Query.ToString().ToLower(), q, "{"+sb+"}");
		}

		/*--------------------------------------------------------------------------------------------*/
		public static RequestCmd CreateSessionCommand(RexConn.SessionAction pAction) {
			return new RequestCmd(RexConn.Command.Session.ToString().ToLower(),
				pAction.ToString().ToLower());
		}

		/*--------------------------------------------------------------------------------------------*/
		public static RequestCmd CreateConfigCommand(RexConn.ConfigSetting pSetting, string pValue) {
			return new RequestCmd(RexConn.Command.Session.ToString().ToLower(),
				pSetting.ToString().ToLower(), pValue);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private static string JsonUnquote(string pText) {
			return pText.Replace("\"", "\\\"");
		}

	}

}