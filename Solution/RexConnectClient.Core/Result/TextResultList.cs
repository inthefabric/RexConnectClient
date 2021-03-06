﻿using System.Collections.Generic;

namespace RexConnectClient.Core.Result {

	/*================================================================================================*/
	public class TextResultList : ITextResultList {

		public IList<string> Values { get; private set; }
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public TextResultList(IList<string> pValues) {
			Values = pValues;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public string ToString(int pIndex) {
			return Values[pIndex];
		}

		/*--------------------------------------------------------------------------------------------*/
		public bool ToBool(int pIndex) {
			return bool.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public byte ToByte(int pIndex) {
			return byte.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public short ToShort(int pIndex) {
			return short.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public int ToInt(int pIndex) {
			return int.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public long ToLong(int pIndex) {
			return long.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public float ToFloat(int pIndex) {
			return float.Parse(ToString(pIndex));
		}

		/*--------------------------------------------------------------------------------------------*/
		public double ToDouble(int pIndex) {
			return double.Parse(ToString(pIndex));
		}

	}

}