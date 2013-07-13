using System;
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
		private IResponseResult ExecuteRequest(Request pReq) {
			var ctx = new RexConnContext(pReq, RexConnHost, RexConnPort);
			var da = new RexConnDataAccess(ctx);
			return da.Execute();
		}

	}

}