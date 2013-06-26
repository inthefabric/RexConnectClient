using NUnit.Framework;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Test.RcCore.Transfer {

	/*================================================================================================*/
	[TestFixture]
	public class TRequestCmd : TestBase {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void New() {
			var rc = new RequestCmd();

			Assert.Null(rc.Cmd, "Cmd should be null.");
			Assert.Null(rc.Args, "Args should be null.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void NewCmdArgs() {
			const string cmd = "myCommand";
			var rc = new RequestCmd(cmd, "a1", "a2", "a3");

			Assert.AreEqual(cmd, rc.Cmd, "Incorrect Cmd.");
			Assert.AreEqual(new[] { "a1", "a2", "a3" }, rc.Args, "Incorrect Args.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void NewCmdArgsArray() {
			const string cmd = "myCommand";
			string[] args = new[] { "a1", "a2", "a3" };
			var rc = new RequestCmd(cmd, args);

			Assert.AreEqual(cmd, rc.Cmd, "Incorrect Cmd.");
			Assert.AreEqual(args, rc.Args, "Incorrect Args.");
		}
		
	}

}