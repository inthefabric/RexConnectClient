using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RexConnectClient.Core.Result.Strings {

	/*================================================================================================*/
	[DataContract]
	internal class StringsResponseCmd {

		[DataMember(Name="r")]
		public IList<string> Results { get; set; }

	}

}