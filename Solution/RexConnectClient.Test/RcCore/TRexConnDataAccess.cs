using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void Execute(bool pUseHttp) {
			const string reqId = "1234";

			var r = new Request(reqId);
			r.AddQuery("g");
			r.AddConfigSetting(RexConn.ConfigSetting.Pretty, "1");
			r.AddSessionAction(RexConn.SessionAction.Start);
			r.AddQuery("x = 5");
			r.AddQuery("x+2");
			r.AddSessionAction(RexConn.SessionAction.Rollback);
			r.AddSessionAction(RexConn.SessionAction.Close);

			IResponseResult result = ExecuteRequest(r, pUseHttp);

			Assert.NotNull(result, "Result should not be null.");
			Assert.NotNull(result.Response, "Result.Response should not be null.");
			Assert.NotNull(result.ResponseJson, "Result.ResponseJson should not be null.");

			Assert.False(result.IsError, "Incorrect IsError.");
			Assert.AreEqual(reqId, result.Response.ReqId, "Incorrect Response.ReqId.");
			Assert.Null(result.Response.SessId, "Response.SessId should be null.");
			Assert.AreEqual(7, result.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteRaw(bool pUseHttp) {
			const string reqId = "1234";

			var r = new Request(reqId);
			r.AddQuery("999");

			string result = BuildDataAccess(r, pUseHttp).ExecuteRaw();

			const string expectPattern = 
				@"\{""i"":""1234"",""t"":\d+,""c"":\["+
					@"\{""t"":\d+,""r"":\[999\]\}"+
				@"\]\}";

			StringAssert.IsMatch(expectPattern, result, "Incorrect result: "+result);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteSessions(bool pUseHttp) {
			var r = new Request("1");
			r.AddSessionAction(RexConn.SessionAction.Start);
			IResponseResult result = ExecuteRequest(r, pUseHttp);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			string sessId = result.Response.SessId;

			////

			r = new Request("2", sessId);
			r.AddQuery("x = 5");
			result = ExecuteRequest(r, pUseHttp);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(5, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("3", sessId);
			r.AddQuery("x += 15");
			result = ExecuteRequest(r, pUseHttp);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(20, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("4", sessId);
			r.AddQuery("x");
			result = ExecuteRequest(r, pUseHttp);
			Assert.NotNull(result.Response.SessId, "SessId should be filled.");
			Assert.AreEqual(20, result.GetTextResultsAt(0).ToInt(0));

			////

			r = new Request("5", sessId);
			r.AddSessionAction(RexConn.SessionAction.Rollback);
			r.AddSessionAction(RexConn.SessionAction.Close);
			result = ExecuteRequest(r, pUseHttp);
			Assert.Null(result.Response.SessId, "SessId should be null.");

			////

			r = new Request("6");
			r.AddQuery("x"); //not available outside of session
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r, pUseHttp));
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteConditional(bool pUseHttp) {
			const string reqId = "1234";

			var r = new Request(reqId);
			//r.AddConfigSetting(RexConn.ConfigSetting.Debug, "1");
			r.AddSessionAction(RexConn.SessionAction.Start);

			RequestCmd none = r.AddQuery(" ");
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

			IResponseResult result = ExecuteRequest(r, pUseHttp);

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
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteRequestOptions(bool pUseHttp) {
			var r = new Request();
			r.EnableOption(Request.Option.OmitTimer);
			r.AddQuery("1");

			IResponseResult result = ExecuteRequest(r, pUseHttp);

			Assert.NotNull(result, "Result should not be null.");
			Assert.NotNull(result.Response, "Result.Response should not be null.");
			Assert.NotNull(result.ResponseJson, "Result.ResponseJson should not be null.");

			Assert.False(result.IsError, "Incorrect IsError.");
			Assert.AreEqual(1, result.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");

			Assert.Null(result.Response.Timer, "Timer should be null.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteCommandOptions(bool pUseHttp) {
			var r = new Request();

			RequestCmd c = r.AddQuery("1");
			c.EnableOption(RequestCmd.Option.OmitTimer);

			c = r.AddQuery("1");
			c.EnableOption(RequestCmd.Option.OmitResults);

			c = r.AddQuery("1");
			c.EnableOption(RequestCmd.Option.OmitTimer);
			c.EnableOption(RequestCmd.Option.OmitResults);

			IResponseResult result = ExecuteRequest(r, pUseHttp);

			Assert.NotNull(result, "Result should not be null.");
			Assert.NotNull(result.Response, "Result.Response should not be null.");
			Assert.NotNull(result.ResponseJson, "Result.ResponseJson should not be null.");

			Assert.False(result.IsError, "Incorrect IsError.");
			Assert.AreEqual(3, result.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");

			IList<ResponseCmd> cmdList = result.Response.CmdList;
			ResponseCmd cmd0 = cmdList[0];
			ResponseCmd cmd1 = cmdList[1];
			ResponseCmd cmd2 = cmdList[2];

			Assert.Null(cmd0.Timer, "Incorrect Cmd[0].Timer.");
			Assert.NotNull(cmd0.Results, "Incorrect Cmd[0].Results.");

			Assert.NotNull(cmd1.Timer, "Incorrect Cmd[1].Timer.");
			Assert.Null(cmd1.Results, "Incorrect Cmd[1].Results.");

			Assert.Null(cmd2.Timer, "Incorrect Cmd[2].Timer.");
			Assert.Null(cmd2.Results, "Incorrect Cmd[2].Results.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(200, false)]
		[TestCase(200, true)]
		[Category(Integration)]
		public void ExecuteMany(int pQueryCount, bool pUseHttp) {
			var r = new Request("x");

			for ( int i = 0 ; i < pQueryCount ; ++i ) {
				r.AddQuery("[val:"+i+"]");
			}

			Console.WriteLine("Execute...");
			IResponseResult result = ExecuteRequest(r, pUseHttp);

			Console.WriteLine("GetTextResults...");
			IList<ITextResultList> trList = result.GetTextResults();
			Assert.AreEqual(pQueryCount, trList.Count, "Incorrect GetTextResults() count.");

			Console.WriteLine("GetMapResults...");
			IList<IList<IDictionary<string, string>>> mapList = result.GetMapResults();
			Assert.AreEqual(pQueryCount, mapList.Count, "Incorrect GetMapResults() count.");

			for ( int i = 0 ; i < pQueryCount ; ++i ) {
				Assert.AreEqual("{val:"+i+"}", trList[i].ToString(0),
					"Incorrect text result at "+i+".");
				Assert.AreEqual(i+"", mapList[i][0]["val"], "Incorrect map result at "+i+".");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase("x", true)]
		[TestCase("^invalid-gremlin/script!", true)]
		[TestCase("x", false)]
		[TestCase("^invalid-gremlin/script!", false)]
		[Category(Integration)]
		public void ExecuteErrQuery(string pScript, bool pUseHttp) {
			var r = new Request("1234");
			r.AddQuery(pScript);
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r, pUseHttp));
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteErrConfig(bool pUseHttp) {
			var r = new Request("1234");
			r.AddConfigSetting(RexConn.ConfigSetting.Pretty, "invalid");
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r, pUseHttp));
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteErrSession(bool pUseHttp) {
			var r = new Request("1234");
			r.AddSessionAction(RexConn.SessionAction.Close);
			TestUtil.CheckThrows<ResponseErrException>(true, () => ExecuteRequest(r, pUseHttp));
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(1, true)]
		[TestCase(2, true)]
		[TestCase(5, true)]
		[TestCase(10, true)]
		[TestCase(100, true)]
		[TestCase(1, false)]
		[TestCase(2, false)]
		[TestCase(5, false)]
		[TestCase(10, false)]
		[TestCase(100, false)]
		[Category(Integration)]
		public void ExecuteTiming(int pQueryCount, bool pUseHttp) {
			var r = new Request("x");
			r.AddSessionAction(RexConn.SessionAction.Start);
			r.AddQuery("g");
			r.AddSessionAction(RexConn.SessionAction.Close);
			ExecuteRequest(r, pUseHttp);

			// Execute separate requests
			
			var sw = Stopwatch.StartNew();
			long reqTime = 0;

			r = new Request("x");
			r.AddSessionAction(RexConn.SessionAction.Start);
			IResponseResult result = ExecuteRequest(r, pUseHttp);
			reqTime += (long)result.Response.Timer;

			string sessId = result.Response.SessId;

			for ( int i = 0 ; i < pQueryCount ; ++i ) {
				r = new Request("x", sessId);
				r.AddQuery("g");
				result = ExecuteRequest(r, pUseHttp);
				reqTime += (long)result.Response.Timer;
			}

			r = new Request("x", sessId);
			r.AddSessionAction(RexConn.SessionAction.Close);
			result = ExecuteRequest(r, pUseHttp);
			reqTime += (long)result.Response.Timer;

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
			result = ExecuteRequest(r, pUseHttp);
			reqTime2 += (long)result.Response.Timer;

			sw2.Stop();

			IList<ITextResultList> trList = result.GetTextResults();
			Assert.AreEqual(pQueryCount+2, trList.Count, "Incorrect query result count.");
			
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

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		[Category(Integration)]
		public void ExecuteSeveralRequests(bool pUseHttp) {
			var r = new Request("x");
			r.AddQuery("1+1");
			ExecuteRequest(r, pUseHttp);
			ExecuteRequest(r, pUseHttp);
			ExecuteRequest(r, pUseHttp);
			ExecuteRequest(r, pUseHttp);
			ExecuteRequest(r, pUseHttp);

			for ( int x = 0 ; x < 5 ; ++x ) {
				const int count = 400;

				var sw = Stopwatch.StartNew();
				long reqTime = 0;

				for ( int i = 0 ; i < count ; ++i ) {
					reqTime += (long)ExecuteRequest(r, pUseHttp).Response.Timer;
				}

				TimeSpan ts = sw.Elapsed;
				sw.Stop();

				Console.WriteLine("Avg: "+(ts.TotalMilliseconds/count)+"ms total, "+
					(reqTime/(double)count)+"ms RexConnect / Latency: "+
					((ts.TotalMilliseconds-reqTime)/count)+"ms");

				Thread.Sleep(100);
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private RexConnDataAccess BuildDataAccess(Request pReq, bool pUseHttp) {
			var ctx = new RexConnContext(pReq, RexConnHost, (pUseHttp ? 8182 : RexConnPort));
			ctx.SetHttpMode(pUseHttp, "graph");
			ctx.Logger = (level, category, text, ex) => {};
			return new RexConnDataAccess(ctx);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private IResponseResult ExecuteRequest(Request pReq, bool pUseHttp) {
			return BuildDataAccess(pReq, pUseHttp).Execute();
		}

	}

}