using NUnit.Framework;

namespace RexConnectClient.Test {

	/*================================================================================================*/
	public abstract class TestBase {

		public const string Integration = "Integration";

		public const string RexConnHost = "rexster";
		public const int RexConnPort = 8185;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		[SetUp]
		protected virtual void SetUp() {}

		/*--------------------------------------------------------------------------------------------*/
		[TearDown]
		public virtual void TearDown() {}

	}

}