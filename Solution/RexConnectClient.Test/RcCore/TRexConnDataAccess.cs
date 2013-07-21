﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using RexConnectClient.Core;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;
using RexConnectClient.Test.Utils;

namespace RexConnectClient.Test.RcCore {

	/*================================================================================================*/
	[TestFixture]
	public class TRexConnDataAccess : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			var mockCtx = new Mock<IRexConnContext>();
			var da = new RexConnDataAccess(mockCtx.Object);
			Assert.AreEqual(mockCtx.Object, da.Context, "Incorrect Context.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void Execute() {
			const string reqId = "1234";

			var r = new Request(reqId);
			r.AddQuery("g");
			r.AddConfigSetting(RexConn.ConfigSetting.Pretty, "1");
			r.AddSessionAction(RexConn.SessionAction.Start);
			r.AddQuery("x = 5");
			r.AddQuery("x+2");
			r.AddSessionAction(RexConn.SessionAction.Rollback);
			r.AddSessionAction(RexConn.SessionAction.Close);

			IResponseResult result = ExecuteRequest(r);

			Assert.NotNull(result, "Result should not be null.");
			Assert.NotNull(result.Response, "Result.Response should not be null.");
			Assert.NotNull(result.ResponseJson, "Result.ResponseJson should not be null.");

			Assert.False(result.IsError, "Incorrect IsError.");
			Assert.AreEqual(reqId, result.Response.ReqId, "Incorrect Response.ReqId.");
			Assert.Null(result.Response.SessId, "Response.SessId should be null.");
			Assert.AreEqual(7, result.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void ExecuteSessions() {
			var r = new Request("1");
			r.AddSessionAction(RexConn.SessionAction.Start);
			IResponseResult result = ExecuteRequest(r);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			string sessId = result.Response.SessId;

			////

			r = new Request("2", sessId);
			r.AddQuery("x = 5");
			result = ExecuteRequest(r);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(5, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("3", sessId);
			r.AddQuery("x += 15");
			result = ExecuteRequest(r);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(20, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("4", sessId);
			r.AddQuery("x");
			result = ExecuteRequest(r);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(20, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("5", sessId);
			r.AddSessionAction(RexConn.SessionAction.Rollback);
			r.AddSessionAction(RexConn.SessionAction.Close);
			result = ExecuteRequest(r);
			Assert.Null(result.Response.SessId, "SessId should be null.");

			////

			r = new Request("6");
			r.AddQuery("x"); //not available outside of session
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r));
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void ExecuteConditional() {
			const string reqId = "1234";

			var r = new Request(reqId);
			//r.AddConfigSetting(RexConn.ConfigSetting.Debug, "1");
			r.AddSessionAction(RexConn.SessionAction.Start);

			RequestCmd none = r.AddQuery("");
			RequestCmd zero = r.AddQuery("0");
			RequestCmd nul = r.AddQuery("null");
			RequestCmd fals = r.AddQuery("false");
			RequestCmd empty = r.AddQuery("x=''");

			RequestCmd one = r.AddQuery("1");
			RequestCmd tru = r.AddQuery("true");
			RequestCmd g = r.AddQuery("g");

			none.CmdId = "none";
			zero.CmdId = "zero";
			nul.CmdId = "nul";
			fals.CmdId = "fals";
			empty.CmdId = "empty";

			one.CmdId = "one";
			tru.CmdId = "tru";
			g.CmdId = "g";

			// Expected to skip

			RequestCmd test = r.AddQuery("throw new Exception('noneTest')");
			test.AddConditionalCommandId(none.CmdId);

			RequestCmd zeroTest = r.AddQuery("throw new Exception('zeroTest')");
			zeroTest.CmdId = "zeroTest";
			zeroTest.AddConditionalCommandId(zero.CmdId);

			test = r.AddQuery("throw new Exception('nulTest')");
			test.AddConditionalCommandId(nul.CmdId);

			test = r.AddQuery("throw new Exception('falsTest')");
			test.AddConditionalCommandId(fals.CmdId);

			test = r.AddQuery("throw new Exception('emptyTest')");
			test.AddConditionalCommandId(empty.CmdId);

			RequestCmd zeroTestTest = r.AddQuery("throw new Exception('zeroTestTest')");
			zeroTestTest.AddConditionalCommandId(zeroTest.CmdId); //an unexecuted command

			test = r.AddQuery("throw new Exception('comboTest1')");
			test.AddConditionalCommandId(one.CmdId);
			test.AddConditionalCommandId(nul.CmdId);

			test = r.AddQuery("throw new Exception('comboTest2')");
			test.AddConditionalCommandId(tru.CmdId);
			test.AddConditionalCommandId(one.CmdId);
			test.AddConditionalCommandId(zero.CmdId);
			test.AddConditionalCommandId(g.CmdId);
			test.AddConditionalCommandId(fals.CmdId);

			// Expected to pass

			test = r.AddQuery("[val:'one']");
			test.CmdId = "oneTest";
			test.AddConditionalCommandId(one.CmdId);

			test = r.AddQuery("[val:'tru']");
			test.CmdId = "truTest";
			test.AddConditionalCommandId(tru.CmdId);

			test = r.AddQuery("[val:'g']");
			test.CmdId = "gTest";
			test.AddConditionalCommandId(g.CmdId);

			test = r.AddQuery("[val:'all']");
			test.CmdId = "allTest";
			test.AddConditionalCommandId(tru.CmdId);
			test.AddConditionalCommandId(one.CmdId);
			test.AddConditionalCommandId(g.CmdId);

			// Finish the session

			r.AddSessionAction(RexConn.SessionAction.Close);

			IResponseResult result = ExecuteRequest(r);

			Assert.NotNull(result, "Result should not be null.");
			Assert.NotNull(result.Response, "Result.Response should not be null.");
			Assert.NotNull(result.ResponseJson, "Result.ResponseJson should not be null.");

			Assert.False(result.IsError, "Incorrect IsError.");
			Assert.AreEqual(reqId, result.Response.ReqId, "Incorrect Response.ReqId.");
			Assert.Null(result.Response.SessId, "Response.SessId should be null.");
			Assert.AreEqual(22, result.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");

			var cmdMap = new Dictionary<string, ResponseCmd>();

			foreach ( ResponseCmd rc in result.Response.CmdList ) {
				if ( rc.CmdId == null ) {
					continue;
				}

				cmdMap.Add(rc.CmdId, rc);
			}

			var passChecks = new[] { "one", "tru", "g", "all" };

			foreach ( string c in passChecks ) {
				Assert.AreEqual(c, cmdMap[c+"Test"].Results[0]["val"],
					"Failed '"+c+"' command response.");
			}

			Console.WriteLine("TIME: "+result.Response.Timer+"ms");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase("x")]
		[TestCase("^invalid-gremlin/script!")]
		[Category(Integration)]
		public void ExecuteErrQuery(string pScript) {
			var r = new Request("1234");
			r.AddQuery(pScript);
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r));
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void ExecuteErrConfig() {
			var r = new Request("1234");
			r.AddConfigSetting(RexConn.ConfigSetting.Pretty, "invalid");
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r));
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void ExecuteErrSession() {
			var r = new Request("1234");
			r.AddSessionAction(RexConn.SessionAction.Close);
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r));
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(1)]
		[TestCase(2)]
		[TestCase(5)]
		[TestCase(10)]
		[TestCase(100)]
		//[TestCase(1000)]
		[Category(Integration)]
		public void ExecuteTiming(int pQueryCount) {
			var r = new Request("x");
			r.AddSessionAction(RexConn.SessionAction.Start);
			r.AddQuery("g");
			r.AddSessionAction(RexConn.SessionAction.Close);
			ExecuteRequest(r);

			// Execute separate requests
			
			var sw = Stopwatch.StartNew();
			long reqTime = 0;

			r = new Request("x");
			r.AddSessionAction(RexConn.SessionAction.Start);
			IResponseResult result = ExecuteRequest(r);
			reqTime += result.Response.Timer;

			string sessId = result.Response.SessId;

			for ( int i = 0 ; i < pQueryCount ; ++i ) {
				r = new Request("x", sessId);
				r.AddQuery("g");
				result = ExecuteRequest(r);
				reqTime += result.Response.Timer;
			}

			r = new Request("x", sessId);
			r.AddSessionAction(RexConn.SessionAction.Close);
			result = ExecuteRequest(r);
			reqTime += result.Response.Timer;

			sw.Stop();

			// Execute one combined request

			var sw2 = Stopwatch.StartNew();
			long reqTime2 = 0;

			r = new Request("x");
			r.AddSessionAction(RexConn.SessionAction.Start);

			for ( int i = 0 ; i < pQueryCount ; ++i ) {
				r.AddQuery("g");
			}

			r.AddSessionAction(RexConn.SessionAction.Close);
			result = ExecuteRequest(r);
			reqTime2 += result.Response.Timer;

			sw2.Stop();
			
			// Report results

			Console.WriteLine();
			Console.WriteLine("Query Count: "+pQueryCount);
			Console.WriteLine("Separate requests: "+
				sw.Elapsed.TotalMilliseconds+"ms total, "+reqTime+"ms RexConnect");
			Console.WriteLine("Combined requests: "+
				sw2.Elapsed.TotalMilliseconds+"ms total, "+reqTime2+"ms RexConnect");
			Console.WriteLine("Combined improvement:"+
				(sw.Elapsed.TotalMilliseconds/sw2.Elapsed.TotalMilliseconds).ToString("###.0")+
				"x (total), "+(reqTime/(double)reqTime2).ToString("###.0")+"x (RexConnect)"
			);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private IResponseResult ExecuteRequest(Request pReq) {
			var ctx = new RexConnContext(pReq, RexConnHost, RexConnPort);
			var da = new RexConnDataAccess(ctx);
			return da.Execute();
		}

	}

}