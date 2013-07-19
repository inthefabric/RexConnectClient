using System.Collections.Generic;
using ServiceStack.Text;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	public class ResponseCmd {

		public string CmdId { get; set; }
		public long Timer { get; set; }
		public IList<JsonObject> Results { get; set; }
		public string Err { get; set; }

	}

}