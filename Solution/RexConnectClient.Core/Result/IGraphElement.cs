﻿using ServiceStack.Text;

namespace RexConnectClient.Core.Result {

	/*================================================================================================*/
	public interface IGraphElement {

		RexConn.GraphElementType Type { get; set; }
		string Id { get; set; }
		string Label { get; set; }
		string OutVertexId { get; set; }
		string InVertexId { get; set; }
		JsonObject Properties { get; set; }

	}

}