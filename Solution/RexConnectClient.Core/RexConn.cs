namespace RexConnectClient.Core {

	/*================================================================================================*/
	public static class RexConn {

		public enum Command {
			Session = 1,
			Query,
			QueryC,
			Config
		}

		public enum SessionAction {
			Start = 1,
			Close,
			Commit,
			Rollback
		}

		public enum ConfigSetting {
			Debug = 1,
			Pretty
		}

		public enum GraphElementType {
			Unspecified = -1,
			Vertex = 1,
			Edge
		}

	}

}