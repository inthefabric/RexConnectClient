using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack.Text;

namespace RexConnectClient.Core.Transfer {

	/*================================================================================================*/
	[DataContract]
	public class ResponseCmd {

		[DataMember(Name="i")]
		public string CmdId { get; set; }

		[DataMember(Name="t")]
		public long? Timer { get; set; }

		[DataMember(Name="r")]
		public IList<JsonObject> Results { get; set; }

		[DataMember(Name="e")]
		public string Err { get; set; }
		
		[DataMember(Name="k")]
		public string CacheKey { get; set; }

	}

}