using NUnit.Framework;
using RexConnectClient.Core;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;

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
		public void Log() {
			var r = new Request();
			var ctx = new RexConnContext(r, null, 0);
			ctx.Log("Type", "Category", "Text");
		}
		
	}

}