﻿namespace RexConnectClient.Core.Cache {

	/*================================================================================================*/
	public interface IRexConnCacheProvider {
	
	
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IRexConnCache GetCache(string pHostName, int pPort);
		
		/*--------------------------------------------------------------------------------------------*/
		int GetCacheCount();

	}

}