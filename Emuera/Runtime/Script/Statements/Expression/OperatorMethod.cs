﻿using MinorShift.Emuera.Runtime.Script.Statements.Function;
using MinorShift.Emuera.Runtime.Script.Statements.Variable;
using MinorShift.Emuera.Runtime.Utils;
using MinorShift.Emuera.Runtime.Utils.EvilMask;
using System.Collections.Generic;
using System.Text;
using trerror = MinorShift.Emuera.Runtime.Utils.EvilMask.Lang.Error;

namespace MinorShift.Emuera.Runtime.Script.Statements.Expression;

/// <summary>
/// 引数のチェック、戻り値の型チェック等は全て呼び出し元が責任を負うこと。
/// </summary>
internal abstract class OperatorMethod : FunctionMethod
{
	public OperatorMethod()
	{
		argumentTypeArray = null;
	}
	public override string CheckArgumentType(string name, List<AExpression> arguments) { throw new ExeEE("型チェックは呼び出し元が行うこと"); }
}

internal static class OperatorMethodManager
{
	readonly static Dictionary<OperatorCode, OperatorMethod> unaryDic = [];
	readonly static Dictionary<OperatorCode, OperatorMethod> unaryAfterDic = [];
	readonly static Dictionary<OperatorCode, OperatorMethod> binaryIntIntDic = [];
	readonly static Dictionary<OperatorCode, OperatorMethod> binaryStrStrDic = [];
	readonly static OperatorMethod binaryMultIntStr;
	readonly static OperatorMethod ternaryIntIntInt;
	readonly static OperatorMethod ternaryIntStrStr;

	static OperatorMethodManager()
	{
		unaryDic[OperatorCode.Plus] = new PlusInt();
		unaryDic[OperatorCode.Minus] = new MinusInt();
		unaryDic[OperatorCode.Not] = new NotInt();
		unaryDic[OperatorCode.BitNot] = new BitNotInt();
		unaryDic[OperatorCode.Increment] = new IncrementInt();
		unaryDic[OperatorCode.Decrement] = new DecrementInt();

		unaryAfterDic[OperatorCode.Increment] = new IncrementAfterInt();
		unaryAfterDic[OperatorCode.Decrement] = new DecrementAfterInt();

		binaryIntIntDic[OperatorCode.Plus] = new PlusIntInt();
		binaryIntIntDic[OperatorCode.Minus] = new MinusIntInt();
		binaryIntIntDic[OperatorCode.Mult] = new MultIntInt();
		binaryIntIntDic[OperatorCode.Div] = new DivIntInt();
		binaryIntIntDic[OperatorCode.Mod] = new ModIntInt();
		binaryIntIntDic[OperatorCode.Equal] = new EqualIntInt();
		binaryIntIntDic[OperatorCode.Greater] = new GreaterIntInt();
		binaryIntIntDic[OperatorCode.Less] = new LessIntInt();
		binaryIntIntDic[OperatorCode.GreaterEqual] = new GreaterEqualIntInt();
		binaryIntIntDic[OperatorCode.LessEqual] = new LessEqualIntInt();
		binaryIntIntDic[OperatorCode.NotEqual] = new NotEqualIntInt();
		binaryIntIntDic[OperatorCode.And] = new AndIntInt();
		binaryIntIntDic[OperatorCode.Or] = new OrIntInt();
		binaryIntIntDic[OperatorCode.Xor] = new XorIntInt();
		binaryIntIntDic[OperatorCode.Nand] = new NandIntInt();
		binaryIntIntDic[OperatorCode.Nor] = new NorIntInt();
		binaryIntIntDic[OperatorCode.BitAnd] = new BitAndIntInt();
		binaryIntIntDic[OperatorCode.BitOr] = new BitOrIntInt();
		binaryIntIntDic[OperatorCode.BitXor] = new BitXorIntInt();
		binaryIntIntDic[OperatorCode.RightShift] = new RightShiftIntInt();
		binaryIntIntDic[OperatorCode.LeftShift] = new LeftShiftIntInt();

		binaryStrStrDic[OperatorCode.Plus] = new PlusStrStr();
		binaryStrStrDic[OperatorCode.Equal] = new EqualStrStr();
		binaryStrStrDic[OperatorCode.Greater] = new GreaterStrStr();
		binaryStrStrDic[OperatorCode.Less] = new LessStrStr();
		binaryStrStrDic[OperatorCode.GreaterEqual] = new GreaterEqualStrStr();
		binaryStrStrDic[OperatorCode.LessEqual] = new LessEqualStrStr();
		binaryStrStrDic[OperatorCode.NotEqual] = new NotEqualStrStr();

		binaryMultIntStr = new MultStrInt();
		ternaryIntIntInt = new TernaryIntIntInt();
		ternaryIntStrStr = new TernaryIntStrStr();
	}



	public static AExpression ReduceUnaryTerm(OperatorCode op, AExpression o1)
	{
		OperatorMethod method = null;
		if (op == OperatorCode.Increment || op == OperatorCode.Decrement)
		{
			if (!(o1 is VariableTerm var))
				throw new CodeEE(trerror.IncrementNonVar.Text);
			if (var.Identifier.IsConst)
				throw new CodeEE(trerror.IncrementConst.Text);
		}
		if (o1.GetOperandType() == typeof(long))
		{
			if (op == OperatorCode.Plus)
				return o1;
			if (unaryDic.TryGetValue(op, out OperatorMethod value))
				method = value;
		}
		if (method != null)
			return new FunctionMethodTerm(method, [o1]);
		string errMes;
		if (o1.GetOperandType() == typeof(long))
			errMes = trerror.NumericType.Text;
		else if (o1.GetOperandType() == typeof(string))
			errMes = trerror.StringType.Text;
		else
			errMes = trerror.UnknownType.Text;
		errMes = string.Format(trerror.CanNotAppliedUnaryOp.Text, errMes, OperatorManager.ToOperatorString(op));
		throw new CodeEE(errMes);
	}

	public static AExpression ReduceUnaryAfterTerm(OperatorCode op, AExpression o1)
	{
		OperatorMethod method = null;
		if (op == OperatorCode.Increment || op == OperatorCode.Decrement)
		{
			if (!(o1 is VariableTerm var))
				throw new CodeEE(trerror.IncrementNonVar.Text);
			if (var.Identifier.IsConst)
				throw new CodeEE(trerror.IncrementConst.Text);
		}
		if (o1.GetOperandType() == typeof(long))
		{
			if (unaryAfterDic.TryGetValue(op, out OperatorMethod value))
				method = value;
		}
		if (method != null)
			return new FunctionMethodTerm(method, [o1]);
		string errMes;
		if (o1.GetOperandType() == typeof(long))
			errMes = trerror.NumericType.Text;
		else if (o1.GetOperandType() == typeof(string))
			errMes = trerror.StringType.Text;
		else
			errMes = trerror.UnknownType.Text;
		errMes = string.Format(trerror.CanNotAppliedUnaryOp.Text, errMes, OperatorManager.ToOperatorString(op));
		throw new CodeEE(errMes);
	}

	public static AExpression ReduceBinaryTerm(OperatorCode op, AExpression left, AExpression right)
	{
		OperatorMethod method = null;
		if (left.GetOperandType() == typeof(long) && right.GetOperandType() == typeof(long))
		{
			if (binaryIntIntDic.TryGetValue(op, out OperatorMethod value))
				method = value;
		}
		else if (left.GetOperandType() == typeof(string) && right.GetOperandType() == typeof(string))
		{
			if (binaryStrStrDic.TryGetValue(op, out OperatorMethod value))
				method = value;
		}
		else if (left.GetOperandType() == typeof(long) && right.GetOperandType() == typeof(string)
			 || left.GetOperandType() == typeof(string) && right.GetOperandType() == typeof(long))
		{
			if (op == OperatorCode.Mult)
				method = binaryMultIntStr;
		}
		if (method != null)
			return new FunctionMethodTerm(method, [left, right]);
		string typeName1, typeName2, errMes;
		if (left.GetOperandType() == typeof(long))
			typeName1 = trerror.NumericType.Text;
		else if (left.GetOperandType() == typeof(string))
			typeName1 = trerror.StringType.Text;
		else
			typeName1 = trerror.UnknownType.Text;
		if (right.GetOperandType() == typeof(long))
			typeName2 = trerror.NumericType.Text;
		else if (right.GetOperandType() == typeof(string))
			typeName2 = trerror.StringType.Text;
		else
			typeName2 = trerror.UnknownType.Text;
		errMes = string.Format(trerror.CanNotAppliedBinaryOp.Text, typeName1, typeName2, OperatorManager.ToOperatorString(op));
		throw new CodeEE(errMes);
	}

	public static AExpression ReduceTernaryTerm(AExpression o1, AExpression o2, AExpression o3)
	{
		OperatorMethod method = null;
		if (o1.GetOperandType() == typeof(long) && o2.GetOperandType() == typeof(long) && o3.GetOperandType() == typeof(long))
			method = ternaryIntIntInt;
		else if (o1.GetOperandType() == typeof(long) && o2.GetOperandType() == typeof(string) && o3.GetOperandType() == typeof(string))
			method = ternaryIntStrStr;
		if (method != null)
			return new FunctionMethodTerm(method, [o1, o2, o3]);
		throw new CodeEE(trerror.InvalidTernaryOp.Text);

	}

	#region OperatorMethod SubClasses

	private sealed class PlusIntInt : OperatorMethod
	{
		public PlusIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) + arguments[1].GetIntValue(exm);
		}
	}

	private sealed class PlusStrStr : OperatorMethod
	{
		public PlusStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(string);
			argumentTypeArray = [typeof(string), typeof(string)];
		}

		public override string GetStrValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetStrValue(exm) + arguments[1].GetStrValue(exm);
		}
	}

	private sealed class MinusIntInt : OperatorMethod
	{
		public MinusIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) - arguments[1].GetIntValue(exm);
		}
	}

	private sealed class MultIntInt : OperatorMethod
	{
		public MultIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) * arguments[1].GetIntValue(exm);
		}
	}

	private sealed class MultStrInt : OperatorMethod
	{
		public MultStrInt()
		{
			CanRestructure = true;
			ReturnType = typeof(string);
		}
		public override string GetStrValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			string str;
			long value;
			if (arguments[0].GetOperandType() == typeof(long))
			{
				value = arguments[0].GetIntValue(exm);
				str = arguments[1].GetStrValue(exm);
			}
			else
			{
				str = arguments[0].GetStrValue(exm);
				value = arguments[1].GetIntValue(exm);
			}
			if (value < 0)
				throw new CodeEE(string.Format(trerror.MultiplyNegativeToStr.Text, value.ToString()));
			if (value >= 10000)
				throw new CodeEE(string.Format(trerror.Multiply10kToStr.Text, value.ToString()));
			if (string.IsNullOrEmpty(str) || value == 0)
				return "";
			StringBuilder builder = new()
			{
				Capacity = str.Length * (int)value
			};
			for (int i = 0; i < value; i++)
			{
				builder.Append(str);
			}
			return builder.ToString();
		}
	}

	private sealed class DivIntInt : OperatorMethod
	{
		public DivIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			long right = arguments[1].GetIntValue(exm);
			if (right == 0)
				throw new CodeEE(trerror.DivideByZero.Text);
			return arguments[0].GetIntValue(exm) / right;
		}
	}

	private sealed class ModIntInt : OperatorMethod
	{
		public ModIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			long right = arguments[1].GetIntValue(exm);
			if (right == 0)
				throw new CodeEE(trerror.DivideByZero.Text);
			return arguments[0].GetIntValue(exm) % right;
		}
	}


	private sealed class EqualIntInt : OperatorMethod
	{
		public EqualIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) == arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}

	}

	private sealed class EqualStrStr : OperatorMethod
	{
		public EqualStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetStrValue(exm) == arguments[1].GetStrValue(exm))
				return 1L;
			return 0L;
		}
	}

	private sealed class NotEqualIntInt : OperatorMethod
	{
		public NotEqualIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) != arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}
	}

	private sealed class NotEqualStrStr : OperatorMethod
	{
		public NotEqualStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}
		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetStrValue(exm) != arguments[1].GetStrValue(exm))
				return 1L;
			return 0L;
		}

	}

	private sealed class GreaterIntInt : OperatorMethod
	{
		public GreaterIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) > arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}
	}

	private sealed class GreaterStrStr : OperatorMethod
	{
		public GreaterStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}
		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.Config.SCExpression);
			if (c > 0)
				return 1L;
			return 0L;
		}
	}
	private sealed class LessIntInt : OperatorMethod
	{
		public LessIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) < arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}
	}
	private sealed class LessStrStr : OperatorMethod
	{
		public LessStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}
		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.Config.SCExpression);
			if (c < 0)
				return 1L;
			return 0L;
		}

	}

	private sealed class GreaterEqualIntInt : OperatorMethod
	{
		public GreaterEqualIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) >= arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}
	}

	private sealed class GreaterEqualStrStr : OperatorMethod
	{
		public GreaterEqualStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}
		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.Config.SCExpression);
			if (c < 0)
				return 1L;
			return 0L;
		}
	}
	private sealed class LessEqualIntInt : OperatorMethod
	{
		public LessEqualIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) <= arguments[1].GetIntValue(exm))
				return 1L;
			return 0L;
		}

	}
	private sealed class LessEqualStrStr : OperatorMethod
	{
		public LessEqualStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}
		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			int c = string.Compare(arguments[0].GetStrValue(exm), arguments[1].GetStrValue(exm), Config.Config.SCExpression);
			if (c < 0)
				return 1L;
			return 0L;
		}
	}

	private sealed class AndIntInt : OperatorMethod
	{
		public AndIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) != 0 && arguments[1].GetIntValue(exm) != 0)
				return 1L;
			return 0L;
		}

	}

	private sealed class OrIntInt : OperatorMethod
	{
		public OrIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) != 0 || arguments[1].GetIntValue(exm) != 0)
				return 1L;
			return 0L;
		}
	}

	private sealed class XorIntInt : OperatorMethod
	{
		public XorIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			long i1 = arguments[0].GetIntValue(exm);
			long i2 = arguments[1].GetIntValue(exm);
			if (i1 == 0 && i2 != 0 || i1 != 0 && i2 == 0)
				return 1L;
			return 0L;
		}

	}

	private sealed class NandIntInt : OperatorMethod
	{
		public NandIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) == 0 || arguments[1].GetIntValue(exm) == 0)
				return 1L;
			return 0L;
		}

	}

	private sealed class NorIntInt : OperatorMethod
	{
		public NorIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) == 0 && arguments[1].GetIntValue(exm) == 0)
				return 1L;
			return 0L;
		}
	}

	private sealed class BitAndIntInt : OperatorMethod
	{
		public BitAndIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) & arguments[1].GetIntValue(exm);
		}
	}

	private sealed class BitOrIntInt : OperatorMethod
	{
		public BitOrIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) | arguments[1].GetIntValue(exm);
		}
	}

	private sealed class BitXorIntInt : OperatorMethod
	{
		public BitXorIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) ^ arguments[1].GetIntValue(exm);
		}
	}

	private sealed class RightShiftIntInt : OperatorMethod
	{
		public RightShiftIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) >> (int)arguments[1].GetIntValue(exm);
		}
	}

	private sealed class LeftShiftIntInt : OperatorMethod
	{
		public LeftShiftIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) << (int)arguments[1].GetIntValue(exm);
		}
	}

	private sealed class PlusInt : OperatorMethod
	{
		public PlusInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm);
		}
	}

	private sealed class MinusInt : OperatorMethod
	{
		public MinusInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			long ret = arguments[0].GetIntValue(exm);
			if (ret == long.MinValue)
			{
				exm.Console.PrintSystemLine(string.Format(Lang.SystemLine.MinusWontWork.Text, long.MinValue));
			}
			return -arguments[0].GetIntValue(exm);
		}
	}

	private sealed class NotInt : OperatorMethod
	{
		public NotInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			if (arguments[0].GetIntValue(exm) == 0)
				return 1L;
			return 0L;
		}
	}
	private sealed class BitNotInt : OperatorMethod
	{
		public BitNotInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return ~arguments[0].GetIntValue(exm);
		}
	}

	private sealed class IncrementInt : OperatorMethod
	{
		public IncrementInt()
		{
			CanRestructure = false;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			VariableTerm var = (VariableTerm)arguments[0];
			return var.ChangeValue(1L, exm);
		}
	}
	private sealed class DecrementInt : OperatorMethod
	{
		public DecrementInt()
		{
			CanRestructure = false;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			VariableTerm var = (VariableTerm)arguments[0];
			return var.ChangeValue(-1L, exm);
		}
	}
	private sealed class IncrementAfterInt : OperatorMethod
	{
		public IncrementAfterInt()
		{
			CanRestructure = false;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			VariableTerm var = (VariableTerm)arguments[0];
			return var.ChangeValue(1L, exm) - 1;
		}
	}

	private sealed class DecrementAfterInt : OperatorMethod
	{
		public DecrementAfterInt()
		{
			CanRestructure = false;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			VariableTerm var = (VariableTerm)arguments[0];
			return var.ChangeValue(-1L, exm) + 1;
		}
	}


	private sealed class TernaryIntIntInt : OperatorMethod
	{
		public TernaryIntIntInt()
		{
			CanRestructure = true;
			ReturnType = typeof(long);
		}

		public override long GetIntValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) != 0 ? arguments[1].GetIntValue(exm) : arguments[2].GetIntValue(exm);
		}
	}

	private sealed class TernaryIntStrStr : OperatorMethod
	{
		public TernaryIntStrStr()
		{
			CanRestructure = true;
			ReturnType = typeof(string);
		}

		public override string GetStrValue(ExpressionMediator exm, List<AExpression> arguments)
		{
			return arguments[0].GetIntValue(exm) != 0 ? arguments[1].GetStrValue(exm) : arguments[2].GetStrValue(exm);
		}
	}

	#endregion
}
