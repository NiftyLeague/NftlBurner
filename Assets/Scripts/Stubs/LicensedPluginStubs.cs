// The commercial Unity Asset Store plugins these APIs belong to are not
// redistributed in this repository:
//
//   - Beebyte Obfuscator (namespace Beebyte.Obfuscator)
//   - Anti-Cheat Toolkit by Code Stage (namespace CodeStage.AntiCheat)
//
// These no-op/passthrough stubs keep the marked call sites compiling so the
// project source stays browsable. They provide no obfuscation or anti-cheat
// behavior - re-add the real plugins to restore it.

using System;
using UnityEngine;

namespace Beebyte.Obfuscator
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class SkipRenameAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class SkipAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class DoNotFakeAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class RenameAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class SuppressLogAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class ObfuscateLiteralsAttribute : Attribute { }
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class ReplaceLiteralsWithNameAttribute : Attribute { }
}

namespace CodeStage.AntiCheat.ObscuredTypes
{
	// Passthrough stand-ins for the ACTk Obscured* wrappers. Fields serialize as
	// plain values, so Unity scenes/prefabs authored with real Obscured fields
	// lose their stored data - re-add ACTk before building.
	[Serializable]
	public struct ObscuredInt
	{
		[SerializeField] private int _value;
		public static implicit operator int(ObscuredInt v) => v._value;
		public static implicit operator ObscuredInt(int v) => new ObscuredInt { _value = v };
		public static ObscuredInt operator ++(ObscuredInt v) => v._value + 1;
		public static ObscuredInt operator --(ObscuredInt v) => v._value - 1;
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredUInt
	{
		[SerializeField] private uint _value;
		public static implicit operator uint(ObscuredUInt v) => v._value;
		public static implicit operator ObscuredUInt(uint v) => new ObscuredUInt { _value = v };
		public static ObscuredUInt operator ++(ObscuredUInt v) => v._value + 1;
		public static ObscuredUInt operator --(ObscuredUInt v) => v._value - 1;
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredFloat
	{
		[SerializeField] private float _value;
		public static implicit operator float(ObscuredFloat v) => v._value;
		public static implicit operator ObscuredFloat(float v) => new ObscuredFloat { _value = v };
		public static ObscuredFloat operator ++(ObscuredFloat v) => v._value + 1;
		public static ObscuredFloat operator --(ObscuredFloat v) => v._value - 1;
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredLong
	{
		[SerializeField] private long _value;
		public static implicit operator long(ObscuredLong v) => v._value;
		public static implicit operator ObscuredLong(long v) => new ObscuredLong { _value = v };
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredDouble
	{
		[SerializeField] private double _value;
		public static implicit operator double(ObscuredDouble v) => v._value;
		public static implicit operator ObscuredDouble(double v) => new ObscuredDouble { _value = v };
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredBool
	{
		[SerializeField] private bool _value;
		public static implicit operator bool(ObscuredBool v) => v._value;
		public static implicit operator ObscuredBool(bool v) => new ObscuredBool { _value = v };
		public static bool operator true(ObscuredBool v) => v._value;
		public static bool operator false(ObscuredBool v) => !v._value;
		public static bool operator !(ObscuredBool v) => !v._value;
		public override string ToString() => _value.ToString();
	}

	[Serializable]
	public struct ObscuredString : IEquatable<ObscuredString>
	{
		[SerializeField] private string _value;
		public static implicit operator string(ObscuredString v) => v._value;
		public static implicit operator ObscuredString(string v) => new ObscuredString { _value = v };
		public override string ToString() => _value;
		public bool Equals(ObscuredString other) => _value == other._value;
		public override bool Equals(object obj) => obj is ObscuredString o && Equals(o);
		public override int GetHashCode() => _value?.GetHashCode() ?? 0;
	}
}
