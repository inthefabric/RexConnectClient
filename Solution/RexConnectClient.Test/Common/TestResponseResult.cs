using RexConnectClient.Core;
using RexConnectClient.Core.Result;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Test.Common {

	/*================================================================================================*/
	public class TestResponseResult : ResponseResult {


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public TestResponseResult(IRexConnContext pContext) : base(pContext) {}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual void SetTestResponse(Response pResponse) {
			Response = pResponse;
		}

	}

}