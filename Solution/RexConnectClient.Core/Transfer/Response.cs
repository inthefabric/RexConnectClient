using System.Collections.Generic;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	public class Response {

		public string ReqId { get; set; }
		public string SessId { get; set; }
		public long Timer { get; set; }
		public string Err { get; set; }
		public IList<ResponseCmd> CmdList { get; set; }

	}

}