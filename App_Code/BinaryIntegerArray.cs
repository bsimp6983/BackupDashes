using System;
using System.Collections.Generic;
using System.Text;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// Manages an array of integer values, which can be extracted in binary form.
    /// </summary>
    public class BinaryIntegerArray<T> : List<T> where T : struct
    {
        #region Methods

        private byte[] GetBytes( TypeCode typeCode, T value )
        {
            byte[] bytes = null;

            switch ( typeCode )
            {
                case TypeCode.Int16:
                {
                    bytes = BitConverter.GetBytes( Convert.ToInt16( value ) );
                    break;
                }
                case TypeCode.Int32:
                {
                    bytes = BitConverter.GetBytes( Convert.ToInt32( value ) );
                    break;
                }
                case TypeCode.Int64:
                {
                    bytes = BitConverter.GetBytes( Convert.ToInt64( value ) );
                    break;
                }
                case TypeCode.UInt16:
                {
                    bytes = BitConverter.GetBytes( Convert.ToUInt16( value ) );
                    break;
                }

                case TypeCode.UInt32:
                {
                    bytes = BitConverter.GetBytes( Convert.ToUInt32( value ) );
                    break;
                }
                case TypeCode.UInt64:
                {
                    bytes = BitConverter.GetBytes( Convert.ToUInt64( value ) );
                    break;
                }
            }
            
            // NOTE: oddly BitConverter.IsLittleEndian is true even though Windows is big endian
            
            // reverse byte order
            Array.Reverse( bytes );

            return bytes;
        }

        /// <summary>
        /// Returns the contents of the collection in binary form.
        /// </summary>
        /// <returns>An array of type <see cref="Byte"/>.</returns>
        public byte[] ToBinary()
        {
            // exit if there is nothing to do
            if ( Count == 0 )
                return new byte[0];

            List<byte> binary = new List<byte>();
            TypeCode typeCode = Type.GetTypeCode( typeof( T ) );

            // append each value to the binary array
            foreach ( T value in this )
                binary.AddRange( GetBytes( typeCode, value ) );

            return binary.ToArray();
        }

        /// <summary>
        /// Returns the contexts of the collection in hexadecimal form.
        /// </summary>
        /// <returns>A hexadecimal <see cref="String"/>.</returns>
        public string ToHexadecimal()
        {
            // exit if there is nothing to do
            if ( Count == 0 )
                return string.Empty;

            TypeCode typeCode = Type.GetTypeCode( typeof( T ) );
            int size = 0;

            // determine size
            switch ( typeCode )
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                {
                    size = 2;
                    break;
                }
                case TypeCode.Int32:
                case TypeCode.UInt32:
                {
                    size = 4;
                    break;
                }
                case TypeCode.Int64:
                case TypeCode.UInt64:
                {
                    size = 8;
                    break;
                }
            }

            // size string builder accordingly
            int capacity = ( ( size * Count ) + 2 );
            StringBuilder hex = new StringBuilder( "0x", capacity );

            // append each value as hex
            foreach ( T value in this )
                foreach ( byte b in GetBytes( typeCode, value ) )
                    hex.Append( b.ToString( "X2" ) );

            return hex.ToString();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the <see cref="BinaryIntegerArray{T}"/> class.
        /// </summary>
        internal BinaryIntegerArray()
        {
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="BinaryIntegerArray{T}"/> class.
        /// </summary>
        /// <param name="values">An array of initial values to add.</param>
        internal BinaryIntegerArray( T[] values )
        {
            if ( values != null && values.Length > 0 )
                AddRange( values );
        }

        #endregion
    }
}
