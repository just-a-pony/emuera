﻿using MinorShift.Emuera.GameData.Expression;
using System;

namespace MinorShift.Emuera.GameProc.PluginSystem
{
	public class PluginMethodParameter
	{
		public PluginMethodParameter(string initialValue)
		{
			isString = true;
			strValue = initialValue;
		}

		public PluginMethodParameter(Int64 initialValue)
		{
			isString = false;
			intValue = initialValue;
		}

		public bool isString;
		public string strValue;
		public Int64 intValue;
	}

	internal static class PluginMethodParameterBuilder
	{
		internal static PluginMethodParameter ConvertTerm(AExpression term, ExpressionMediator exm)
		{
			if (term.IsString)
			{
				return new PluginMethodParameter(term.GetStrValue(exm));
			}
			else
			{
				return new PluginMethodParameter(term.GetIntValue(exm));

			}
		}
	}
}
