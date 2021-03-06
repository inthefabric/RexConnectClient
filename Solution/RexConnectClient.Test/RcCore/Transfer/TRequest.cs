﻿using System.Collections.Generic;
using NUnit.Framework;
using RexConnectClient.Core;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Test.RcCore.Transfer {

	/*================================================================================================*/
	[TestFixture]
	public class TRequest : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			var r = new Request();

			Assert.Null(r.ReqId, "ReqId should be null.");
			Assert.Null(r.SessId, "SessId should be null.");
			Assert.Null(r.Opt, "Opt should be null.");
			AssertCmdList(r, 0);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void NewReq() {
			const string reqId = "myReqId";
			var r = new Request(reqId);

			Assert.AreEqual(reqId, r.ReqId, "Incorrect ReqId.");
			Assert.Null(r.SessId, "SessId should be null.");
			AssertCmdList(r, 0);
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void NewReqSess() {
			const string reqId = "myReqId";
			const string sessId = "mySessId";
			var r = new Request(reqId, sessId);

			Assert.AreEqual(reqId, r.ReqId, "Incorrect ReqId.");
			Assert.AreEqual(sessId, r.SessId, "Incorrect SessId.");
			AssertCmdList(r, 0);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(false)]
		[TestCase(true)]
		public void AddQuery(bool pCacheScript) {
			const string script = "testScript";
			var r = new Request();

			RequestCmd cmd = r.AddQuery(script, null, pCacheScript);

			AssertCmdList(r, 1);
			
			if ( pCacheScript ) {
				AssertCmd(cmd, "query", script, null, "1");
			}
			else {
				AssertCmd(cmd, "query", script);
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(false)]
		[TestCase(true)]
		public void AddQueryParams(bool pCacheScript) {
			const string script = "testScript";
			const string paramJson = "{\"a\":\"valueA\",\"B\":\"ValueB\"}";

			var param = new Dictionary<string, string>();
			param.Add("a", "valueA");
			param.Add("B", "ValueB");

			var r = new Request();

			RequestCmd cmd = r.AddQuery(script, param, pCacheScript);

			AssertCmdList(r, 1);
			
			if ( pCacheScript ) {
				AssertCmd(cmd, "query", script, paramJson, "1");
			}
			else {
				AssertCmd(cmd, "query", script, paramJson);
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddQueryC() {
			const int cacheKey = 1234;
			var r = new Request();
			
			RequestCmd cmd = r.AddQueryC(cacheKey, null);
			
			AssertCmdList(r, 1);
			AssertCmd(cmd, "queryc", cacheKey+"");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddQueryCParams() {
			const int cacheKey = 1234;
			const string paramJson = "{\"a\":\"valueA\",\"B\":\"ValueB\"}";
			
			var param = new Dictionary<string, string>();
			param.Add("a", "valueA");
			param.Add("B", "ValueB");
			
			var r = new Request();
			
			RequestCmd cmd = r.AddQueryC(cacheKey, param);
			
			AssertCmdList(r, 1);
			AssertCmd(cmd, "queryc", cacheKey+"", paramJson);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddSessionAction() {
			var r = new Request();

			RequestCmd cmd = r.AddSessionAction(RexConn.SessionAction.Rollback);

			AssertCmdList(r, 1);
			AssertCmd(cmd, "session", "rollback");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddConfigSetting() {
			var r = new Request();

			RequestCmd cmd = r.AddConfigSetting(RexConn.ConfigSetting.Debug, "1");

			AssertCmdList(r, 1);
			AssertCmd(cmd, "config", "debug","1");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(Request.Option.OmitTimer, 1)]
		public void EnableOption(Request.Option pOption, byte pExpectOpt) {
			var r = new Request();
			r.EnableOption(pOption);
			Assert.AreEqual(pExpectOpt, r.Opt, "Incorrect Opt.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void ToRequestJson() {
			const string expect = "{\"i\":\"testing\",\"c\":[]}";
			var r = new Request("testing");
			Assert.AreEqual(expect, r.ToRequestJson(), "Incorrect result.");
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void AssertCmdList(Request pReq, int pLen) {
			Assert.NotNull(pReq.CmdList, "CmdList should be filled.");
			Assert.AreEqual(pLen, pReq.CmdList.Count, "Incorrect CmdList.Count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void AssertCmd(RequestCmd pCmd, string pCmdText, params string[] pArgs) {
			Assert.AreEqual(pCmdText, pCmd.Cmd, "Incorrect Cmd.");
			Assert.NotNull(pCmd.Args, "Cmd.Args should be filled.");
			Assert.AreEqual(pArgs, pCmd.Args, "Incorrect Args.");
		}
		
	}

}