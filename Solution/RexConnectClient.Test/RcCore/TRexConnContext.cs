using NUnit.Framework;
using RexConnectClient.Core;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;
using Moq;
using RexConnectClient.Core.Cache;
using RexConnectClient.Test.Utils;
using System;

namespace RexConnectClient.Test.RcCore {

	/*================================================================================================*/
	[TestFixture]
	public class TRexConnContext : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			var r = new Request();
			const string host = "local";
			const int port = 1234;

			var ctx = new RexConnContext(r, host, port);

			Assert.AreEqual(r, ctx.Request, "Incorrect Request.");
			Assert.AreEqual(host, ctx.HostName, "Incorrect HostName.");
			Assert.AreEqual(port, ctx.Port, "Incorrect Port.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void CreateResponseResult() {
			var r = new Request();
			var ctx = new RexConnContext(r, null, 0);
			IResponseResult rr = ctx.CreateResponseResult();

			Assert.NotNull(rr, "Result should not be null.");
			Assert.AreEqual(ctx, rr.Context, "Incorrect result Context.");
			Assert.AreEqual(r, rr.Request, "Incorrect result Request.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void CreateTcpClient() {
			var r = new Request();
			var ctx = new RexConnContext(r, RexConnHost, RexConnPort);
			IRexConnTcp tcp = ctx.CreateTcpClient();

			Assert.NotNull(tcp, "Result should not be null.");
			Assert.AreEqual(1<<16, tcp.SendBufferSize, "Incorrect result SendBufferSize.");
			Assert.AreEqual(1<<16, tcp.ReceiveBufferSize, "Incorrect result ReceiveBufferSize.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCacheWithProvider() {
			const string host = "local";
			const int port = 1234;
			
			var r = new Request();
			var mockProv = new Mock<IRexConnCacheProvider>();
			var mockCache = new Mock<IRexConnCache>();
			mockProv.Setup(x => x.GetCache(host, port)).Returns(mockCache.Object);
			
			var ctx = new RexConnContext(r, host, port);
			ctx.SetCacheProvider(mockProv.Object);
			
			Assert.AreEqual(mockCache.Object, ctx.Cache, "Incorrect result.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void GetCacheWithNoProvider() {
			var r = new Request();
			var ctx = new RexConnContext(r, "x", 0);
			TestUtil.CheckThrows<NullReferenceException>(true, () => { var c = ctx.Cache; });
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void Log() {
			var r = new Request();
			var ctx = new RexConnContext(r, null, 0);
			ctx.Log("Type", "Category", "Text");
		}
		
	}

}