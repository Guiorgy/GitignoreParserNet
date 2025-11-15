using System;
#if !NET7_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
#endif
using System.Text;

namespace GitignoreParserNet;

internal static class Extensions
{
    extension(StringBuilder builder)
    {
        /// <summary>
        /// Prepends the string representation of a specified System.Char object to this instance.
        /// </summary>
        /// <param name="value">The character to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(char value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends a specified number of copies of the string representation of a Unicode character to this instance.
        /// </summary>
        /// <param name="value">The character to prepend.</param>
        /// <param name="repeatCount">The number of times to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// repeatCount is less than zero,
        /// or enlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(char value, int repeatCount) => builder.Insert(0, new string(value, repeatCount));

        /// <summary>
        /// Prepends the string representation of the Unicode characters in a specified array to this instance.
        /// </summary>
        /// <param name="value">The array of characters to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(char[] value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends a copy of the specified string to this instance.
        /// </summary>
        /// <param name="builder">The instance to prepend.</param>        /// <param name="value">The string to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(string value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 64-bit unsigned integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(ulong value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 64-bit signed integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(long value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 32-bit unsigned integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(uint value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 32-bit signed integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(int value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 16-bit unsigned integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(ushort value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 16-bit signed integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(short value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 8-bit unsigned integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(byte value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified 8-bit signed integer to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(sbyte value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified decimal number to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(decimal value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified double-precision floating-point number to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(double value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified single-precision floating-point number to this instance.
        /// </summary>
        /// <param name="value">The value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(float value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified boolean value to this instance.
        /// </summary>
        /// <param name="value">The boolean value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(bool value) => builder.Insert(0, value);

        /// <summary>
        /// Prepends the string representation of a specified object to this instance.
        /// </summary>
        /// <param name="value">The object value to prepend.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Eenlarging the value of this instance would exceed <see cref="StringBuilder.MaxCapacity"/>.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        internal StringBuilder Prepend(object value) => builder.Insert(0, value);
    }

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_0_OR_GREATER
    extension(string @string)
    {
        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified character.
        /// </summary>
        /// <param name="value">The character to find.</param>
        /// <returns><see langword="true"/> if value matches the beginning of this string; otherwise, <see langword="false"/>.</returns>
        internal bool StartsWith(char value) => @string.Length >= 1 && @string[0] == value;

        /// <summary>
        /// Determines whether the end of this string instance matches the specified character.
        /// </summary>
        /// <param name="value">The character to find.</param>
        /// <returns><see langword="true"/> if value matches the end of this instance; otherwise, <see langword="false"/>.</returns>
        internal bool EndsWith(char value) => @string.Length >= 1 && @string[@string.Length - 1] == value;
    }
#endif

#if !NET7_0_OR_GREATER
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Sorts the elements of a sequence in ascending order.
        /// </summary>
        /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method is implemented by using deferred execution. The immediate return value is an object
        /// that stores all the information that is required to perform the action.
        /// The query represented by this method is not executed until the object is enumerated by calling
        /// its <see cref="IEnumerable{T}.GetEnumerator"/> method.
        ///
        /// This method compares elements by using the default comparer <see cref="Comparer{T}.Default"/>.
        /// </para>
        /// </remarks>
        internal IOrderedEnumerable<T> Order() => source.OrderBy(static x => x);
    }
#endif
}
