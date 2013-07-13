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
			
			if ( Response.Err == null || Response.Err.Length == 0 ) {
				Response.Err = pErr;
			}
			else {
				Response.Err += "|"+pErr;
			}
		}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public virtual IList<IList<IGraphElement>> GetGraphElements() {
			if ( vElementResults != null ) {
				return vElementResults;
			}

			vElementResults = new List<IList<IGraphElement>>();

			foreach ( ResponseCmd cmd in Response.CmdList ) {
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
				vTextResults.Add(new TextResultList(src.Results));
			}

			return vTextResults;
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual ITextResultList GetTextResultsAt(int pCommandIndex) {
			GetTextResults();
			return vTextResults[pCommandIndex];
		}

	}

}