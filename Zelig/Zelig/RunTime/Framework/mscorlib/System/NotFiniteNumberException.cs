// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System
{

    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class NotFiniteNumberException : ArithmeticException
    {
        private double _offendingNumber;

        public NotFiniteNumberException() : base( Environment.GetResourceString( "Arg_NotFiniteNumberException" ) )
        {
            _offendingNumber = 0;
        }

        public NotFiniteNumberException( double offendingNumber ) : base()
        {
            _offendingNumber = offendingNumber;
        }

        public NotFiniteNumberException( String message ) : base( message )
        {
            _offendingNumber = 0;
        }

        public NotFiniteNumberException( String message, double offendingNumber ) : base( message )
        {
            _offendingNumber = offendingNumber;
        }

        public NotFiniteNumberException( String message, Exception innerException ) : base( message, innerException )
        {
        }

        public NotFiniteNumberException( String message, double offendingNumber, Exception innerException ) : base( message, innerException )
        {
            _offendingNumber = offendingNumber;
        }

////    protected NotFiniteNumberException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////        _offendingNumber = info.GetInt32( "OffendingNumber" );
////    }

        public double OffendingNumber
        {
            get
            {
                return _offendingNumber;
            }
        }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public override void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////        base.GetObjectData( info, context );
////        info.AddValue( "OffendingNumber", _offendingNumber, typeof( Int32 ) );
////    }
    }
}
