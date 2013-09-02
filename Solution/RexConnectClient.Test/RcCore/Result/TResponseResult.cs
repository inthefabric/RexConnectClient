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
using RexConnectClient.Core.Cache;

namespace RexConnectClient.Test.RcCore.Result {

	/*================================================================================================*/
	[TestFixture]
	public class TResponseResult : TestBase {

		private Mock<IRexConnContext> vMockCtx;
		private TestResponseResult vResult;
		private Response vTestResponse;
		private string vMapResultJson;
		private string vTextResultJson;


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

			rc = new ResponseCmd();
			rc.Results = null;
			vTestResponse.CmdList.Add(rc);

			////

			vMapResultJson =
				@"{
					'i':'MyReqId',
					'c':[
						{'r':{'MyInt':123456, 'MyLong':1234567890123456, 'MyStr':'testing'}},
						{'r':[{'MyByte':2, 'MyFloat':987.654},{'MyStr':'second map'}]},
						{'r':[{'MyBool1':true},{'MyBool2':false},{},{'MyBool3':true}]},
						{'r':{}},
						{'r':null},
						{}
					]
				}";

			vTextResultJson =
				@"{
					'i':'MyReqId',
					'c':[
						{'r':[123456, 1234567890123456, 'testing']},
						{'r':[2, 987.654]},
						{'r':[true]},
						{'r':[]},
						{'r':null},
						{}
					]
				}";

			vMapResultJson = vMapResultJson.Replace("'", "\"");
			vTextResultJson = vTextResultJson.Replace("'", "\"");
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
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void NewWithCachedQueries() {
			const int cacheKey = 234;
			const string cachedScript = "this is my cached script";
			const string expectParamsJson = "{\"A\":\"123\"}";
			var querycParams = new Dictionary<string, string>();
			querycParams.Add("A", "123");
			
			var req = new Request();
			req.AddConfigSetting(RexConn.ConfigSetting.Pretty, "0");
			req.AddQuery("not cached");
			req.AddQuery(cachedScript, querycParams, true);
			req.AddQuery(cachedScript, null, true);
			
			var mockCache = new Mock<IRexConnCache>();
			mockCache.Setup(x => x.FindScriptKey(cachedScript)).Returns(cacheKey);
			
			var mockCtx = new Mock<IRexConnContext>();
			mockCtx.SetupGet(x => x.Request).Returns(req);
			mockCtx.SetupGet(x => x.Cache).Returns(mockCache.Object);
			
			var rr = new ResponseResult(mockCtx.Object);
			
			Assert.AreEqual(4, rr.Request.CmdList.Count, "Incorrect CmdList.Count.");
			
			RequestCmd rc0 = rr.Request.CmdList[0];
			RequestCmd rc1 = rr.Request.CmdList[1];
			RequestCmd rc2 = rr.Request.CmdList[2];
			RequestCmd rc3 = rr.Request.CmdList[3];
			
			Assert.AreEqual("config", rc0.Cmd, "Incorrect Cmd0.Cmd.");
			Assert.AreEqual("query", rc1.Cmd, "Incorrect Cmd1.Cmd.");
			
			Assert.AreEqual("queryc", rc2.Cmd, "Incorrect Cmd2.Cmd.");
			Assert.NotNull(rc2.Args, "Cmd2.Args should be filled.");
			Assert.AreEqual(2, rc2.Args.Count, "Incorrect Cmd2.Args.Count");
			Assert.AreEqual(cacheKey+"", rc2.Args[0], "Incorrect Cmd2.Args[0].");
			Assert.AreEqual(expectParamsJson, rc2.Args[1], "Incorrect Cmd2.Args[1].");
			
			Assert.AreEqual("queryc", rc3.Cmd, "Incorrect Cmd3.Cmd.");
			Assert.NotNull(rc3.Args, "Cmd3.Args should be filled.");
			Assert.AreEqual(1, rc3.Args.Count, "Incorrect Cmd3.Args.Count");
			Assert.AreEqual(cacheKey+"", rc3.Args[0], "Incorrect Cmd3.Args[0].");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void SetResponseJson() {
			const string reqId = "test123";
			const string json = "{\"i\":\""+reqId+"\",\"c\":[{},{\"e\":\"bad!\"}]}";

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
		public void SetResponseJsonWithCached() {
			const int cacheKey = 123;
			const string cacheScript = "this is my script";
			
			var req = new Request();
			req.ReqId = "testReqId";
			req.AddQuery(cacheScript, null, true);
			req.AddQuery("script2");
			
			var resp = new Response();
			resp.ReqId = req.ReqId;
			resp.CmdList = new List<ResponseCmd>();
			resp.CmdList.Add(new ResponseCmd { CacheKey=cacheKey });
			resp.CmdList.Add(new ResponseCmd { CacheKey=null });
			
			var mockCache = new Mock<IRexConnCache>();
			
			var mockCtx = new Mock<IRexConnContext>();
			mockCtx.SetupGet(x => x.Request).Returns(req);
			mockCtx.SetupGet(x => x.Cache).Returns(mockCache.Object);
			
			var rr = new ResponseResult(mockCtx.Object);
			rr.SetResponseJson(JsonSerializer.SerializeToString(resp));
			
			Assert.NotNull(rr.Response, "Response should be filled.");
			Assert.AreEqual(req.ReqId, rr.Response.ReqId, "Incorrect Response.ReqId.");
			Assert.AreEqual(2, rr.Response.CmdList.Count, "Incorrect Response.CmdList.Count.");
			Assert.False(rr.IsError, "Incorrect IsError.");
			
			mockCache.Verify(x => x.AddCachedScript(cacheKey, cacheScript), Times.Once());
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
			const string json = "{\"i\":\"test\",\"e\":\""+err+"\"}";

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
		public void GetMapResults(bool pTwice) {
			vResult.SetResponseJson(vMapResultJson);
			IList<IList<IDictionary<string, string>>> lists = vResult.GetMapResults();

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(6, lists.Count, "Incorrect lists length.");

			if ( pTwice ) {
				Assert.AreEqual(lists, vResult.GetMapResults(), "Incorrect cached result.");
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(0, new[] {
			"MyInt", "123456",
			"MyLong", "1234567890123456",
			"MyStr", "testing"
		})]
		[TestCase(1, new[] {
			"MyByte", "2",
			"MyFloat", "987.654",
			"#END", null,
			"MyStr", "second map"
		})]
		[TestCase(2, new[] {
			"MyBool1", "true",
			"#END", null,
			"MyBool2", "false",
			"#END", null,
			"#END", null,
			"MyBool3", "true"
		})]
		[TestCase(3, new string[] {})]
		public void GetMapResultsAt(int pIndex, string[] pExpectPairs) {
			vResult.SetResponseJson(vMapResultJson);
			IList<IDictionary<string, string>> list = vResult.GetMapResultsAt(pIndex);

			Assert.NotNull(list, "Result should not be null.");
			int cmdMapI = 0;

			for ( int i = 0 ; i < pExpectPairs.Length ; i += 2 ) {
				string key = pExpectPairs[i];
				string val = pExpectPairs[i+1];

				if ( key == "#END" ) {
					//Console.WriteLine("cmd "+pIndex+", map "+cmdMapI+": #END");
					cmdMapI++;
					continue;
				}

				Assert.LessOrEqual(cmdMapI, list.Count, "Incorrect list length.");

				IDictionary<string, string> map = list[cmdMapI];
				Assert.NotNull(map, "Map at "+cmdMapI+" should be filled.");
				Assert.True(map.ContainsKey(key), "Map at "+cmdMapI+" is missing key '"+key+"'.");
				Assert.AreEqual(val, map[key], "Map at "+cmdMapI+": incorrect value for '"+key+"'.");
				//Console.WriteLine("cmd "+pIndex+", map "+cmdMapI+": "+key+" = "+val);
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(4)]
		[TestCase(5)]
		public void GetMapResultsAtNull(int pIndex) {
			vResult.SetResponseJson(vMapResultJson);
			IList<IDictionary<string, string>> list = vResult.GetMapResultsAt(pIndex);
			Assert.Null(list, "Result should be null.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		public void GetGraphElements(bool pTwice) {
			vResult.SetTestResponse(vTestResponse);
			IList<IList<IGraphElement>> lists = vResult.GetGraphElements();

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(4, lists.Count, "Incorrect lists length.");

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

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetGraphElementsAt3() {
			vResult.SetTestResponse(vTestResponse);
			IList<IGraphElement> list = vResult.GetGraphElementsAt(3);
			Assert.Null(list, "Result should be null.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[TestCase(true)]
		[TestCase(false)]
		public void GetTextResults(bool pTwice) {
			vResult.SetResponseJson(vTextResultJson);
			IList<ITextResultList> lists = vResult.GetTextResults();

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(6, lists.Count, "Incorrect lists length.");

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
			vResult.SetResponseJson(vTextResultJson);
			ITextResultList list = vResult.GetTextResultsAt(pIndex);

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(pExpect, list.Values, "Incorrect list Values.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(4)]
		[TestCase(5)]
		public void GetTextResultsAtNull(int pIndex) {
			vResult.SetResponseJson(vTextResultJson);
			ITextResultList list = vResult.GetTextResultsAt(pIndex);
			Assert.Null(list, "Result should be null.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCustomResults() {
			vResult.SetResponseJson(vMapResultJson);
			IList<IList<CustomRes>> lists = vResult.GetCustomResults((c,m) => CustomRes.Convert(m));

			Assert.NotNull(lists, "Result should not be null.");
			Assert.AreEqual(6, lists.Count, "Incorrect lists length.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCustomResultsAt0() {
			IList<CustomRes> list = GetCustomResultsAt(0, 1);

			CustomRes cr = list[0];
			Assert.AreEqual(123456, cr.MyInt, "Incorrect MyInt.");
			Assert.AreEqual(1234567890123456, cr.MyLong, "Incorrect MyLong.");
			Assert.AreEqual("testing", cr.MyStr, "Incorrect MyStr.");
			Assert.Null(cr.MyByte, "Incorrect MyByte.");
			Assert.Null(cr.MyFloat, "Incorrect MyFloat.");
			Assert.Null(cr.MyBool1, "Incorrect MyBool1.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCustomResultsAt1() {
			IList<CustomRes> list = GetCustomResultsAt(1, 2);

			CustomRes cr = list[0];
			Assert.Null(cr.MyInt, "Incorrect MyInt.");
			Assert.Null(cr.MyLong, "Incorrect MyLong.");
			Assert.Null(cr.MyStr, "Incorrect MyStr.");
			Assert.AreEqual(2, cr.MyByte, "Incorrect MyByte.");
			Assert.AreEqual(987654, (int)(cr.MyFloat*1000), "Incorrect MyFloat.");
			Assert.Null(cr.MyBool1, "Incorrect MyBool1.");

			cr = list[1];
			Assert.Null(cr.MyInt, "Incorrect MyInt.");
			Assert.Null(cr.MyLong, "Incorrect MyLong.");
			Assert.AreEqual("second map", cr.MyStr, "Incorrect MyStr.");
			Assert.Null(cr.MyByte, "Incorrect MyByte.");
			Assert.Null(cr.MyFloat, "Incorrect MyFloat.");
			Assert.Null(cr.MyBool1, "Incorrect MyBool1.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCustomResultsAt2() {
			IList<CustomRes> list = GetCustomResultsAt(2, 4);

			CustomRes cr = list[0];
			Assert.Null(cr.MyInt, "Incorrect MyInt.");
			Assert.Null(cr.MyLong, "Incorrect MyLong.");
			Assert.Null(cr.MyStr, "Incorrect MyStr.");
			Assert.Null(cr.MyByte, "Incorrect MyByte.");
			Assert.Null(cr.MyFloat, "Incorrect MyFloat.");
			Assert.AreEqual(true, cr.MyBool1, "Incorrect MyBool1.");

			CheckAllNull(list[1]);
			CheckAllNull(list[2]);
			CheckAllNull(list[3]);
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCustomResultsAt3() {
			IList<CustomRes> list = GetCustomResultsAt(3, 1);
			CheckAllNull(list[0]);
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(4)]
		[TestCase(5)]
		public void GetCustomResultsAtNull(int pIndex) {
			vResult.SetResponseJson(vMapResultJson);
			IList<CustomRes> list = vResult.GetCustomResultsAt(pIndex, (c, m) => CustomRes.Convert(m));
			Assert.Null(list, "List should be null.");
		}

		/*--------------------------------------------------------------------------------------------*/
		private IList<CustomRes> GetCustomResultsAt(int pIndex, int pExpectLen) {
			vResult.SetResponseJson(vMapResultJson);
			IList<CustomRes> list = vResult.GetCustomResultsAt(pIndex, (c, m) => CustomRes.Convert(m));

			Assert.NotNull(list, "Result should not be null.");
			Assert.AreEqual(pExpectLen, list.Count, "Incorrect list count.");
			return list;
		}

		/*--------------------------------------------------------------------------------------------*/
		private void CheckAllNull(CustomRes pRes) {
			Assert.Null(pRes.MyInt, "Incorrect MyInt.");
			Assert.Null(pRes.MyLong, "Incorrect MyLong.");
			Assert.Null(pRes.MyStr, "Incorrect MyStr.");
			Assert.Null(pRes.MyByte, "Incorrect MyByte.");
			Assert.Null(pRes.MyFloat, "Incorrect MyFloat.");
			Assert.Null(pRes.MyBool1, "Incorrect MyBool1.");
		}
	}


	/*================================================================================================*/
	internal class CustomRes {

		public int? MyInt { get; set; }
		public long? MyLong { get; set; }
		public string MyStr { get; set; }
		public byte? MyByte { get; set; }
		public float? MyFloat { get; set; }
		public bool? MyBool1 { get; set; }
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public static CustomRes Convert(IDictionary<string, string> pMap) {
			if ( pMap == null ) {
				return null;
			}

			var cr = new CustomRes();
			cr.MyInt = (pMap.ContainsKey("MyInt") ? int.Parse(pMap["MyInt"]) : (int?)null);
			cr.MyLong = (pMap.ContainsKey("MyLong") ? long.Parse(pMap["MyLong"]) : (long?)null);
			cr.MyStr = (pMap.ContainsKey("MyStr") ? pMap["MyStr"] : null);
			cr.MyByte = (pMap.ContainsKey("MyByte") ? byte.Parse(pMap["MyByte"]) : (byte?)null);
			cr.MyFloat = (pMap.ContainsKey("MyFloat") ? float.Parse(pMap["MyFloat"]) : (float?)null);
			cr.MyBool1 = (pMap.ContainsKey("MyBool1") ? bool.Parse(pMap["MyBool1"]) : (bool?)null);
			return cr;
		}

	}

}