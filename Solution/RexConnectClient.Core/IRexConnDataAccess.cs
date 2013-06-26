using RexConnectClient.Core.Result;

namespace RexConnectClient.Core {

	/*================================================================================================*/
	public interface IRexConnDataAccess {

		IRexConnContext Context { get; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IResponseResult Execute();

	}

}