using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RexConnectClient.Core.Cache {

	/*================================================================================================*/
	public class RexConnCacheProvider : IRexConnCacheProvider {

		private readonly ConcurrentDictionary<string, IRexConnCache> vCacheMap;
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public RexConnCacheProvider() {
			vCacheMap = new ConcurrentDictionary<string, IRexConnCache>();
		}

		/*--------------------------------------------------------------------------------------------*/
		public IRexConnCache GetCache(string pHostName, int pPort) {
			string key = pHostName+":"+pPort;

			if ( !vCacheMap.ContainsKey(key) ) {
				vCacheMap.TryAdd(key, new RexConnCache(pHostName, pPort));
			}

			return vCacheMap[key];
		}

		/*--------------------------------------------------------------------------------------------*/
		public int GetCacheCount() {
			return vCacheMap.Count;
		}

		/*--------------------------------------------------------------------------------------------*/
		public ICollection<string> GetCacheKeys() {
			return vCacheMap.Keys;
		}

	}

}