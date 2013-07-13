using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RexConnectClient.Core;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;
using RexConnectClient.Test.Common;
using RexConnectClient.Test.Utils;
using ServiceStack.Text;

namespace RexConnectClient.Test.RcCore.Result {

	/*================================================================================================*/
	[TestFixture]
	public class TResponseResult : TestBase {

		private Mock<IRexConnContext> vMockCtx;
		private TestResponseResult vResult;
		private Response vTestResponse;
		private string vTestResultJson;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		protected override void  SetUp() {
			base.SetUp();

			var mockReq = new Mock<Request>();
			vMockCtx = new Mock<IRexConnContext>();
			vMockCtx.SetupGet(x => x.Request).Returns(mockReq.Object);

			vResult = new TestResponseResult(vMockCtx.Object);

			////

			vTestResponse = new Response();
			vTestResponse.CmdList = new List<ResponseCmd>();

			var joVert = new JsonObject();
			joVert.Add("_type", "vertex");

			var joEdge = new JsonObject();
			joEdge.Add("_type", "edge");

			var rc = new ResponseCmd();
			rc.Results = new List<JsonObject>();
			vTestResponse.CmdList.Add(rc);

			rc.Results.Add(joVert);
			rc.Results.Add(joEdge);
			rc.Results.Add(joVert);
			rc.Results.Add(joEdge);

			rc = new ResponseCmd();
			rc.Results = new List<JsonObject>();
			vTestResponse.CmdList.Add(rc);

			for ( int i = 0 ; i < 10 ; ++i ) {
				rc.Results.Add(joVert);
			}

			rc = new ResponseCmd();
			rc.Results = new List<JsonObject>();
			vTestResponse.CmdList.Add(rc);

			for ( int i = 0 ; i < 20 ; ++i ) {
				rc.Results.Add(joEdge);
			}

			////

			vTestResultJson =
				@"{
					'reqId':'MyReqId',
					'cmdList':[
						{'results':[123456, 1234567890123456, 'testing']},
						{'results':[2, 987.654]},
						{'results':[true]},
						{'results':[]}
					]
				}";

			vTestResultJson = vTestResultJson.Replace("'", "\"");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			const string json = "testJson";
			var mockReq = new Mock<Request>();
			mockReq.Setup(x => x.ToRequestJson()).Returns(json);

			var mockCtx = new Mock<IRexConnContext>();
			mockCtx.SetupGet(x => x.Request).Returns(mockReq.Object);

			var rr = new ResponseResult(mockCtx.Object);

			Assert.AreEqual(mockCtx.Object, rr.Context, "Incorrect Context.");
			Assert.AreEqual(mockReq.Object, rr.Request, "Incorrect Request.");
			Assert.AreEqual(json, rr.RequestJson, "Incorrect RequestJson.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseJson() {
			const string reqId = "test123";
			const string json = "{\"reqId\":\""+reqId+"\",\"cmdList\":[{},{\"err\":\"bad!\"}]}";

			vResult.SetResponseJson(json);

			Assert.AreEqual(json, vResult.ResponseJson, "Incorrect ResponseJson.");
			Assert.NotNull(vResult.Response, "Response should be filled.");
			Assert.AreEqual(reqId, vResult.Response.ReqId, "Incorrect Response.ReqId.");
			Assert.AreEqual(2, vResult.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");
			Assert.False(vResult.IsError, "Incorrect IsError.");

			vMockCtx.Verify(x => x.Log("Warn", "Data", It.IsAny<string>(), null), Times.Once());
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseJsonNull() {
			Exception e = TestUtil.CheckThrows<Exception>(true, () => vResult.SetResponseJson(null));
			Assert.True(vResult.IsError, "Incorrect IsError.");
			Assert.True(e.Message.Contains("is null"), "Incorrect exception.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseJsonError() {
			const string err = "something went horribly askew";
			const string json = "{\"reqId\":\"test\",\"err\":\""+err+"\"}";

			Exception e = TestUtil.CheckThrows<ResponseErrException>(
				true, () => vResult.SetResponseJson(json));
			Assert.True(vResult.IsError, "Incorrect IsError.");
			Assert.True(e.Message.Contains(err), "Incorrect exception.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseJsonDuplicate() {
			vResult.SetResponseError(null);
			Exception e = TestUtil.CheckThrows<Exception>(true, () => vResult.SetResponseJson(null));
			Assert.True(e.Message.Contains("already set"), "Incorrect exception.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseError() {
			const string err = "something went horribly askew";

			vResult.SetResponseError(err);

			Assert.NotNull(vResult.Response, "Response should be filled.");
			Assert.NotNull(vResult.Response.CmdList, "Response.CmdList should be filled.");
			Assert.AreEqual(err, vResult.Response.Err, "Incorrect Response.Err.");
			Assert.True(vResult.IsError, "Incorrect IsError.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase("something", "duplicate", "something|duplicate")]
		[TestCase(null, "duplicate", "duplicate")]
		public void SetResponseErrorDuplicate(string pErr1, string pErr2, string pExpect) {
			vResult.SetResponseError(pErr1);
			vResult.SetResponseError(pErr2);
			
			Assert.NotNull(vResult.Response, "Response should be filled.");
			Assert.NotNull(vResult.Response.CmdList, "Response.CmdList should be filled.");
			Assert.AreEqual(pExpect, vResult.Response.Err, "Incorrect Response.Err.");
			Assert.True(vResult.IsError, "Incorrect IsError.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		public void GetGraphElements(bool pTwice) {
			vResult.SetTestResponse(vTestResponse);
			IList<IList<IGraphElement>> lists = vResult.GetGraphElements();

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(3, lists.Count, "Incorrect lists length.");

			if ( pTwice ) {
				Assert.AreEqual(lists, vResult.GetGraphElements(), "Incorrect cached result.");
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetGraphElementsAt0() {
			vResult.SetTestResponse(vTestResponse);
			IList<IGraphElement> list = vResult.GetGraphElementsAt(0);

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(4, list.Count, "Incorrect list length.");
			Assert.AreEqual(RexConn.GraphElementType.Vertex, list[0].Type, "Incorrect Type at 0.");
			Assert.AreEqual(RexConn.GraphElementType.Edge, list[1].Type, "Incorrect Type at 1.");
			Assert.AreEqual(RexConn.GraphElementType.Vertex, list[2].Type, "Incorrect Type at 2.");
			Assert.AreEqual(RexConn.GraphElementType.Edge, list[3].Type, "Incorrect Type at 3.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetGraphElementsAt1() {
			vResult.SetTestResponse(vTestResponse);
			IList<IGraphElement> list = vResult.GetGraphElementsAt(1);

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(10, list.Count, "Incorrect list length.");

			for ( int i = 0 ; i < 10 ; ++i ) {
				Assert.AreEqual(RexConn.GraphElementType.Vertex, list[i].Type,
					"Incorrect Type at "+i+".");
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetGraphElementsAt2() {
			vResult.SetTestResponse(vTestResponse);
			IList<IGraphElement> list = vResult.GetGraphElementsAt(2);

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(20, list.Count, "Incorrect list length.");

			for ( int i = 0 ; i < 20 ; ++i ) {
				Assert.AreEqual(RexConn.GraphElementType.Edge, list[i].Type,
					"Incorrect Type at "+i+".");
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		public void GetTextResults(bool pTwice) {
			vResult.SetResponseJson(vTestResultJson);
			IList<ITextResultList> lists = vResult.GetTextResults();

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(4, lists.Count, "Incorrect lists length.");

			if ( pTwice ) {
				Assert.AreEqual(lists, vResult.GetTextResults(), "Incorrect cached result.");
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(0, new [] { "123456", "1234567890123456", "testing" })]
		[TestCase(1, new [] { "2", "987.654" })]
		[TestCase(2, new [] { "true" })]
		[TestCase(3, new string[] {})]
		public void GetTextResultsAt(int pIndex, string[] pExpect) {
			vResult.SetResponseJson(vTestResultJson);
			ITextResultList list = vResult.GetTextResultsAt(pIndex);

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(pExpect, list.Values, "Incorrect list Values.");
		}

	}

}