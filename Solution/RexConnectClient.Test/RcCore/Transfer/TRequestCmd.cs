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

			Assert.Null(rc.CmdId, "CmdId should be null.");
			Assert.Null(rc.Opt, "Opt should be null.");
			Assert.Null(rc.Cond, "Cond should be null.");
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

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddConditionalCommandId() {
			var rc = new RequestCmd();
			rc.AddConditionalCommandId("a0");

			Assert.NotNull(rc.Cond, "Cond should be filled.");
			Assert.AreEqual(1, rc.Cond.Count, "Incorrect Cond.Count.");
			Assert.AreEqual("a0", rc.Cond[0], "Cond[0] is incorrect.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void AddConditionalCommandIdMulti() {
			var rc = new RequestCmd();
			rc.AddConditionalCommandId("a0");
			rc.AddConditionalCommandId("b1");
			rc.AddConditionalCommandId("c2");

			Assert.NotNull(rc.Cond, "Cond should be filled.");
			Assert.AreEqual(3, rc.Cond.Count, "Incorrect Cond.Count.");
			Assert.AreEqual("a0", rc.Cond[0], "Cond[0] is incorrect.");
			Assert.AreEqual("b1", rc.Cond[1], "Cond[1] is incorrect.");
			Assert.AreEqual("c2", rc.Cond[2], "Cond[2] is incorrect.");
		}

		/*--------------------------------------------------------------------------------------------*/
		[TestCase(RequestCmd.Option.OmitTimer, 1)]
		[TestCase(RequestCmd.Option.OmitResults, 2)]
		public void EnableOption(RequestCmd.Option pOption, byte pExpectOpt) {
			var rc = new RequestCmd();
			rc.EnableOption(pOption);
			Assert.AreEqual(pExpectOpt, rc.Opt, "Incorrect Opt.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		[Test]
		public void EnableOptions() {
			var rc = new RequestCmd();
			rc.EnableOption(RequestCmd.Option.OmitTimer);
			rc.EnableOption(RequestCmd.Option.OmitResults);
			Assert.AreEqual(3, rc.Opt, "Incorrect Opt.");
		}

	}

}