using System.Net.Sockets;

namespace RexConnectClient.Core {
	
	/*================================================================================================*/
	public interface IRexConnTcp {

		int SendBufferSize { get; set; }
		int ReceiveBufferSize { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		NetworkStream GetStream();
		void Close();

	}

}