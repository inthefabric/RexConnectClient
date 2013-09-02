using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	[DataContract]
	public class RequestCmd {

		public enum Option {
			OmitTimer = 1,
			OmitResults = 2
		}

		[DataMember(Name="i")]
		public string CmdId { get; set; }

		[DataMember(Name="o")]
		public byte? Opt { get; set; }

		[DataMember(Name="e")]
		public IList<string> Cond { get; set; }

		[DataMember(Name="c")]
		public string Cmd { get; set; }

		[DataMember(Name="a")]
		public IList<string> Args { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RequestCmd() {}
		
		/*--------------------------------------------------------------------------------------------*/
		public RequestCmd(string pCommand, params string[] pArgs) {
			Cmd = pCommand;
			Args = new List<string>(pArgs);
		}

		/*--------------------------------------------------------------------------------------------*/
		public void AddConditionalCommandId(string pCommandId) {
			if ( Cond == null ) {
				Cond = new List<string>();
			}

			Cond.Add(pCommandId);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public void EnableOption(Option pOption) {
			if ( Opt == null ) {
				Opt = 0;
			}

			Opt = (byte)(Opt | (byte)pOption);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public bool IsQueryToBeCached() {
			if ( Cmd != RexConn.Command.Query.ToString().ToLower() ) {
				return false;
			}
			
			return (Args.Count == 3 && Args[2] == "1");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateQuery(string pScript, IDictionary<string, string> pParams=null, 
																			bool pCacheScript=false) {
			int argN = (pCacheScript ? 3 : (pParams == null ? 1 : 2));
			var args = new string[argN];
			args[0] = JsonUnquote(pScript);
			
			if ( pParams != null ) {
				args[1] = GetParamsJson(pParams);
			}
			
			if ( pCacheScript ) {
				args[2] = "1";
			}

			return new RequestCmd(RexConn.Command.Query.ToString().ToLower(), args);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateQueryC(int pCacheKey, IDictionary<string,string> pParams=null){
			string queryc = RexConn.Command.QueryC.ToString().ToLower();
			string key = pCacheKey+"";
			
			if ( pParams == null ) {
				return new RequestCmd(queryc, key);
			}
			
			return new RequestCmd(queryc, key, GetParamsJson(pParams));
		}
		
		/*--------------------------------------------------------------------------------------------*/
		internal static void ConvertToQueryC(RequestCmd pQueryCmd, int pCacheKey) {
			if ( !pQueryCmd.IsQueryToBeCached() ) {
				throw new Exception("IsQueryToBeCached is false for this command.");
			}
			
			pQueryCmd.Cmd = RexConn.Command.QueryC.ToString().ToLower();
			
			if ( pQueryCmd.Args.Count == 1 ) {
				pQueryCmd.Args[0] = pCacheKey+"";
			}
			else {
				pQueryCmd.Args = new[] { pCacheKey+"", pQueryCmd.Args[1] };
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateSession(RexConn.SessionAction pAction) {
			return new RequestCmd(RexConn.Command.Session.ToString().ToLower(),
				pAction.ToString().ToLower());
		}

		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateConfig(RexConn.ConfigSetting pSetting, string pValue) {
			return new RequestCmd(RexConn.Command.Config.ToString().ToLower(),
				pSetting.ToString().ToLower(), pValue);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private static string JsonUnquote(string pText) {
			return pText.Replace("\"", "\\\"");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private static string GetParamsJson(IDictionary<string, string> pParams) {
			var sb = new StringBuilder();
			
			foreach ( string key in pParams.Keys ) {
				sb.Append(
					(sb.Length > 0 ? "," : "")+
					"\""+JsonUnquote(key)+"\":"+
					"\""+JsonUnquote(pParams[key])+"\""
					);
			}
			
			return "{"+sb+"}";
		}

	}

}