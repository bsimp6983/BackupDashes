using System;
using System.Collections.Generic;
using System.Text;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// Represents a factory for binary integer array objects.
    /// <seealso cref="BinaryIntegerArray{T}"/>
    /// </summary>
    public static class BinaryIntegerArrayFactory
    {
        #region Methods

        /// <summary>
        /// Returns an array for signed 16-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<short> CreateInt16()
        {
            return new BinaryIntegerArray<short>();
        }

        /// <summary>
        /// Returns an array for signed 16-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<short> CreateInt16( params short[] values )
        {
            return new BinaryIntegerArray<short>( values );
        }

        /// <summary>
        /// Returns an array for unsigned 16-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<ushort> CreateUInt16()
        {
            return new BinaryIntegerArray<ushort>();
        }

        /// <summary>
        /// Returns an array for unsigned 16-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<ushort> CreateUInt16( params ushort[] values )
        {
            return new BinaryIntegerArray<ushort>( values );
        }

        /// <summary>
        /// Returns an array for signed 32-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<int> CreateInt32()
        {
            return new BinaryIntegerArray<int>();
        }

        /// <summary>
        /// Returns an array for signed 32-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<int> CreateInt32( params int[] values )
        {
            return new BinaryIntegerArray<int>( values );
        }

        /// <summary>
        /// Returns an array for unsigned 32-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<uint> CreateUInt32()
        {
            return new BinaryIntegerArray<uint>();
        }

        /// <summary>
        /// Returns an array for unsigned 32-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<uint> CreateUInt32( params uint[] values )
        {
            return new BinaryIntegerArray<uint>( values );
        }

        /// <summary>
        /// Returns an array for signed 64-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<long> CreateInt64()
        {
            return new BinaryIntegerArray<long>();
        }

        /// <summary>
        /// Returns an array for signed 64-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<long> CreateInt64( params long[] values )
        {
            return new BinaryIntegerArray<long>( values );
        }

        /// <summary>
        /// Returns an array for unsigned 64-bit integers.
        /// </summary>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<ulong> CreateUInt64()
        {
            return new BinaryIntegerArray<ulong>();
        }

        /// <summary>
        /// Returns an array for unsigned 64-bit integers.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        /// <returns>A <see cref="BinaryIntegerArray{T}"/> object.</returns>
        public static BinaryIntegerArray<ulong> CreateUInt64( params ulong[] values )
        {
            return new BinaryIntegerArray<ulong>( values );
        }

        #endregion
    }
}
