using System;
using System.Collections.Generic;
using RexConnectClient.Core.Transfer;

namespace RexConnectClient.Core.Result {

	/*================================================================================================*/
	public interface IResponseResult {
		
		IRexConnContext Context { get; }

		Request Request { get; }
		string RequestJson { get; }
		Response Response { get; }
		string ResponseJson { get; }

		bool IsError { get; }
		int ExecutionMilliseconds { get; set; }


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		void SetResponseJson(string pResponseJson);

		/*--------------------------------------------------------------------------------------------*/
		void SetResponseError(string pErr);


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IList<IList<IDictionary<string, string>>> GetMapResults();

		/*--------------------------------------------------------------------------------------------*/
		IList<IDictionary<string, string>> GetMapResultsAt(int pCommandIndex);


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IList<IList<IGraphElement>> GetGraphElements();
		
		/*--------------------------------------------------------------------------------------------*/
		IList<IGraphElement> GetGraphElementsAt(int pCommandIndex);


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IList<ITextResultList> GetTextResults();

		/*--------------------------------------------------------------------------------------------*/
		ITextResultList GetTextResultsAt(int pCommandIndex);

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		IList<IList<T>> GetCustomResults<T>(Func<string, 
			IDictionary<string, string>, T> pFromCmdIdAndMap);

		/*--------------------------------------------------------------------------------------------*/
		IList<T> GetCustomResultsAt<T>(int pCommandIndex, 
			Func<string, IDictionary<string, string>, T> pFromCmdIdAndMap);

	}

}