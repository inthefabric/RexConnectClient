using System.Collections.Generic;

namespace RexConnectClient.Core.Cache {

	/*================================================================================================*/
	public class RexConnCacheProvider : IRexConnCacheProvider {
	
		private IDictionary<string, IRexConnCache> vCacheMap;
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RexConnCacheProvider() {
			vCacheMap = new Dictionary<string, IRexConnCache>();
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public IRexConnCache GetCache(string pHostName, int pPort) {
			string key = pHostName+":"+pPort;
			
			if ( !vCacheMap.ContainsKey(key) ) {
				vCacheMap.Add(key, new RexConnCache(pHostName, pPort));
			}
			
			return vCacheMap[key];
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public int GetCacheCount() {
			return vCacheMap.Count;
		}

	}

}