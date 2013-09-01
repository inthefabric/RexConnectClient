using System.Collections.Generic;
using System;

namespace RexConnectClient.Core.Cache {

	/*================================================================================================*/
	//TEST: RexConnCache
	public class RexConnCache : IRexConnCache {
	
		public string HostName { get; private set; }
		public int Port { get; private set; }
		
		private IDictionary<int, string> vScriptMap;
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RexConnCache(string pHostName, int pPort) {
			HostName = pHostName;
			Port = pPort;
			vScriptMap = new Dictionary<int, string>();
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public string GetScriptByKey(int pCacheKey) {
			if ( vScriptMap.ContainsKey(pCacheKey) ) {
				return vScriptMap[pCacheKey];
			}
			
			throw new KeyNotFoundException("No script found for cache key '"+pCacheKey+"'.");
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public int? FindScriptKey(string pScript) {
			foreach ( KeyValuePair<int, string> pair in vScriptMap ) {
				if ( pair.Value == pScript ) {
					return pair.Key;
				}
			}
			
			return null;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public void AddCachedScript(int pCacheKey, string pScript) {
			if ( vScriptMap.ContainsKey(pCacheKey) ) {
				if ( vScriptMap[pCacheKey] == pScript ) {
					return;
				}
				
				throw new Exception("A different script is already cached for key '"+pCacheKey+"':\n"+
					"Current: "+vScriptMap[pCacheKey]+"\n"+
					"New:     "+pScript);
			}
			
			vScriptMap.Add(pCacheKey, pScript);
		}

	}

}