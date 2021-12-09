using System;
using System.Runtime.InteropServices;

namespace andywiecko.BurstCollections
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public readonly struct Id<T> : IEquatable<Id<T>>, IComparable<Id<T>>, IIndexer
    {
        public static readonly Id<T> Invalid = new Id<T>(-1);
        public static readonly Id<T> Zero = new Id<T>(0);

        public readonly int Value { get; }

        public bool IsValid => Value != -1;

        public Id(int val) => Value = val;

        public bool Equals(Id<T> other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Id<T> id && Equals(id);
        public int CompareTo(Id<T> other) => Value.CompareTo(other.Value);
        public override int GetHashCode() => Value;
        public override string ToString() => $"({typeof(T).Name}){Value}";

        public static explicit operator int(Id<T> id) => id.Value;
        public static explicit operator Id<T>(int val) => new Id<T>(val);

        public static bool operator ==(Id<T> left, Id<T> right) => left.Equals(right);
        public static bool operator !=(Id<T> left, Id<T> right) => !left.Equals(right);
        public static bool operator <(Id<T> left, Id<T> right) => left.Value < right.Value;
        public static bool operator <=(Id<T> left, Id<T> right) => left.Value <= right.Value;
        public static bool operator >(Id<T> left, Id<T> right) => left.Value > right.Value;
        public static bool operator >=(Id<T> left, Id<T> right) => left.Value >= right.Value;

        public static Id<T> operator +(Id<T> left, Id<T> right) => new Id<T>(left.Value + right.Value);
        public static Id<T> operator -(Id<T> left, Id<T> right) => new Id<T>(left.Value - right.Value);
        public static Id<T> operator +(Id<T> left, int right) => new Id<T>(left.Value + right);
        public static Id<T> operator -(Id<T> left, int right) => new Id<T>(left.Value - right);
        public static Id<T> operator +(int left, Id<T> right) => new Id<T>(left + right.Value);
        public static Id<T> operator -(int left, Id<T> right) => new Id<T>(left - right.Value);
        public static Id<T> operator ++(Id<T> id) => new Id<T>(id.Value + 1);
        public static Id<T> operator --(Id<T> id) => new Id<T>(id.Value - 1);
    }
}