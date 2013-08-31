﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateQuery(string pScript, bool pCacheScript=false) {
			string q = JsonUnquote(pScript);
			string c = (pCacheScript ? "1" : null);
			return new RequestCmd(RexConn.Command.Query.ToString().ToLower(), q, null, c);
		}

		/*--------------------------------------------------------------------------------------------*/
		internal static RequestCmd CreateQuery(string pScript, IDictionary<string, string> pParams, 
																			bool pCacheScript=false) {
			string q = JsonUnquote(pScript);
			string c = (pCacheScript ? "1" : null);
			var sb = new StringBuilder();

			foreach ( string key in pParams.Keys ) {
				sb.Append(
					(sb.Length > 0 ? "," : "")+
					"\""+JsonUnquote(key)+"\":"+
					"\""+JsonUnquote(pParams[key])+"\""
				);
			}

			return new RequestCmd(RexConn.Command.Query.ToString().ToLower(), q, "{"+sb+"}", c);
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

	}

}