namespace RexConnectClient.Core.Cache {

	/*================================================================================================*/
	public interface IRexConnCache {
	
	
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		string GetScriptByKey(int pCacheKey);
		int? FindScriptKey(string pScript);
		void AddCachedScript(int pCacheKey, string pScript);

	}

}