using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RexConnectClient.Core.Result.Strings {

	/*================================================================================================*/
	[DataContract]
	internal class StringsResponse {

		[DataMember(Name="c")]
		public IList<StringsResponseCmd> CmdList { get; set; }

	}

}