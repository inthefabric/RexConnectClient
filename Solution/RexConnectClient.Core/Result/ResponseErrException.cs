using System;

namespace RexConnectClient.Core.Result {

	/*================================================================================================*/
	public class ResponseErrException : Exception {
	
		public ResponseResult Result { get; private set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public ResponseErrException(ResponseResult pResult) : base(pResult.Response.Err) {
			Result = pResult;
		}

	}

}