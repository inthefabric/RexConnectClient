using System;
using System.Collections.Generic;
using System.Linq;
using RexConnectClient.Core.Result.Strings;
using RexConnectClient.Core.Transfer;
using ServiceStack.Text;

namespace RexConnectClient.Core.Result {

	/*================================================================================================*/
	public class ResponseResult : IResponseResult {

		public IRexConnContext Context { get; protected set; }

		public Request Request { get; protected set; }
		public string RequestJson { get; protected set; }
		public Response Response { get; protected set; }
		public string ResponseJson { get; protected set; }

		public bool IsError { get; protected set; }
		public int ExecutionMilliseconds { get; set; }

		private IList<IList<IDictionary<string, string>>> vMapResults;
		private IList<IList<IGraphElement>> vElementResults;
		private IList<ITextResultList> vTextResults;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public ResponseResult(IRexConnContext pContext) {
			Context = pContext;
			Request = pContext.Request;
			RequestJson = Request.ToRequestJson();
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void SetResponseJson(string pResponseJson) {
			if ( Response != null ) {
				throw new Exception("Response is already set.");
			}
			
			ResponseJson = pResponseJson;
			Response = JsonSerializer.DeserializeFromString<Response>(ResponseJson);

			if ( Response == null ) {
				IsError = true;
				throw new Exception("Response is null.");
			}

			if ( Response.Err != null ) {
				IsError = true;
				throw new ResponseErrException(this);
			}

			for ( int i = 0 ; i < Response.CmdList.Count ; ++i ) {
				ResponseCmd rc = Response.CmdList[i];

				if ( rc.Err != null ) {
					Context.Log("Warn", "Data", "Response.CmdList["+i+"] error: "+rc.Err);
				}
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void SetResponseError(string pErr) {
			IsError = true;
			
			if ( Response == null ) {
				Response = new Response();
				Response.CmdList = new List<ResponseCmd>();
			}
			
			if ( string.IsNullOrEmpty(Response.Err) ) {
				Response.Err = pErr;
			}
			else {
				Response.Err += "|"+pErr;
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IList<IDictionary<string, string>>> GetMapResults() {
			if ( vMapResults != null ) {
				return vMapResults;
			}

			vMapResults = new List<IList<IDictionary<string, string>>>();

			foreach ( ResponseCmd cmd in Response.CmdList ) {
				if ( cmd.Results == null ) {
					vMapResults.Add(null);
					continue;
				}

				var cmdMaps = new List<IDictionary<string, string>>();

				foreach ( JsonObject jo in cmd.Results ) {
					cmdMaps.Add(jo.ToDictionary()); //create a copy
				}

				vMapResults.Add(cmdMaps);
			}

			return vMapResults;
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IDictionary<string, string>> GetMapResultsAt(int pCommandIndex) {
			GetMapResults();
			return vMapResults[pCommandIndex];
		}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IList<IGraphElement>> GetGraphElements() {
			if ( vElementResults != null ) {
				return vElementResults;
			}

			vElementResults = new List<IList<IGraphElement>>();

			foreach ( ResponseCmd cmd in Response.CmdList ) {
				if ( cmd.Results == null ) {
					vElementResults.Add(null);
					continue;
				}

				vElementResults.Add(
					cmd.Results.Select(GraphElement.Build).Cast<IGraphElement>().ToList()
				);
			}

			return vElementResults;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IGraphElement> GetGraphElementsAt(int pCommandIndex) {
			GetGraphElements();
			return vElementResults[pCommandIndex];
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<ITextResultList> GetTextResults() {
			if ( vTextResults != null ) {
				return vTextResults;
			}

			StringsResponse sr = JsonSerializer.DeserializeFromString<StringsResponse>(ResponseJson);
			vTextResults = new List<ITextResultList>();

			foreach ( StringsResponseCmd src in sr.CmdList ) {
				vTextResults.Add(src.Results == null ? null : new TextResultList(src.Results));
			}

			return vTextResults;
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual ITextResultList GetTextResultsAt(int pCommandIndex) {
			GetTextResults();
			return vTextResults[pCommandIndex];
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IList<T>> GetCustomResults<T>(
										Func<string, IDictionary<string, string>, T> pFromCmdIdAndMap) {
			return Response.CmdList
				.Select((t,i) => GetCustomResultsAt(i, pFromCmdIdAndMap))
				.ToList();
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<T> GetCustomResultsAt<T>(int pCommandIndex,
										Func<string, IDictionary<string, string>, T> pFromCmdIdAndMap) {
			ResponseCmd cmd = Response.CmdList[pCommandIndex];

			if ( cmd.Results == null ) {
				return null;
			}

			return cmd.Results
				.Select(jo => pFromCmdIdAndMap(cmd.CmdId, jo))
				.ToList();
		}

	}

}