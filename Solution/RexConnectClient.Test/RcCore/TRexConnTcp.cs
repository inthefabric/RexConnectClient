using NUnit.Framework;
using RexConnectClient.Core;

namespace RexConnectClient.Test.RcCore {

	/*================================================================================================*/
	[TestFixture]
	public class TRexConnTcp : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void New() {
			BuildTcp();
			Assert.Pass("Something failed.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void SendBufferSize() {
			RexConnTcp tcp = BuildTcp();
			tcp.SendBufferSize = 1234;
			Assert.AreEqual(1234, tcp.SendBufferSize, "Incorrect result.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void ReceiveBufferSize() {
			RexConnTcp tcp = BuildTcp();
			tcp.ReceiveBufferSize = 1234;
			Assert.AreEqual(1234, tcp.ReceiveBufferSize, "Incorrect result.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		[Category(Integration)]
		public void GetStream() {
			RexConnTcp tcp = BuildTcp();
			Assert.NotNull(tcp.GetStream(), "Result should not be null.");
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private RexConnTcp BuildTcp() {
			return new RexConnTcp("rexster", 8185);
		}
		
	}

}