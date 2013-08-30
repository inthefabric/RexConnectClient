using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	[DataContract]
	public class Response {

		[DataMember(Name="i")]
		public string ReqId { get; set; }

		[DataMember(Name="s")]
		public string SessId { get; set; }

		[DataMember(Name="t")]
		public long? Timer { get; set; }

		[DataMember(Name="e")]
		public string Err { get; set; }

		[DataMember(Name="c")]
		public IList<ResponseCmd> CmdList { get; set; }

	}

}