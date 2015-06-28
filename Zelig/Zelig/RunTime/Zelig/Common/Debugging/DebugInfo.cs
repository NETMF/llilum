//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugging
{
    using System;
    using System.Text;

    public sealed class DebugInfo
    {
        //
        // State
        //

        public String SrcFileName;
        public String MethodName;
        public int    BeginLineNumber;
        public int    BeginColumn;
        public int    EndLineNumber;
        public int    EndColumn;

        //
        // Constructor Methods
        //

        public DebugInfo( String srcFileName,
                  int beginLineNumber,
                  int beginColumn,
                  int endLineNumber,
                  int endColumn ) : 
            this( srcFileName, null, beginLineNumber,  beginColumn, endLineNumber, endColumn )
        {
        }

        public DebugInfo( String srcFileName     ,
                          String methodName      ,
                          int    beginLineNumber ,
                          int    beginColumn     ,
                          int    endLineNumber   ,
                          int    endColumn       )
        {
            this.SrcFileName      = srcFileName;
            this.MethodName       = methodName;
            this.BeginLineNumber  = beginLineNumber;
            this.BeginColumn      = beginColumn;
            this.EndLineNumber    = endLineNumber;
            this.EndColumn        = endColumn;
        }

        private DebugInfo( String srcFileName ,
                           String methodName  ,
                           int    lineNumber  )
        {
            SetMarkerForLine( srcFileName, methodName, lineNumber );
        }

        //
        // Equality Methods
        //

        public override bool Equals( object obj )
        {
            if(obj is DebugInfo)
            {
                DebugInfo other = (DebugInfo)obj;

                if(this.SrcFileName     == other.SrcFileName     &&
                   this.MethodName      == other.MethodName      &&
                   this.BeginLineNumber == other.BeginLineNumber &&
                   this.BeginColumn     == other.BeginColumn     &&
                   this.EndLineNumber   == other.EndLineNumber   &&
                   this.EndColumn       == other.EndColumn        )
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            Byte[] crcData = System.Text.UTF8Encoding.UTF8.GetBytes(SrcFileName);
            return (int)CRC32.Compute( crcData, (uint)this.BeginLineNumber );
        }

        //
        // Helper Methods
        //

        public static DebugInfo CreateMarkerForLine( string srcFileName ,
                                                     string methodName  ,
                                                     int    lineNumber  )
        {
            return new DebugInfo( srcFileName, methodName, lineNumber );
        }

        public void SetMarkerForLine( string srcFileName ,
                                      string methodName  ,
                                      int    lineNumber  )
        {
            this.SrcFileName     = srcFileName;
            this.MethodName      = methodName;
            this.BeginLineNumber = lineNumber;
            this.BeginColumn     = int.MinValue;
            this.EndLineNumber   = lineNumber;
            this.EndColumn       = int.MaxValue;
        }

        public DebugInfo ComputeIntersection( DebugInfo other )
        {
            if(other == null)
            {
                return null;
            }

            int compareBeginLine = this.BeginLineNumber.CompareTo( other.BeginLineNumber );

            int beginLineNumber;
            int beginColumn;

            if(compareBeginLine < 0)
            {
                beginLineNumber = other.BeginLineNumber;
                beginColumn     = other.BeginColumn;
            }
            else
            {
                beginLineNumber = this.BeginLineNumber;

                 if(compareBeginLine > 0)
                 {
                     beginColumn = this.BeginColumn;
                 }
                 else
                 {
                     beginColumn = Math.Max( this.BeginColumn, other.BeginColumn );
                 }
            }

            //--//

            int compareEndLine = this.EndLineNumber.CompareTo( other.EndLineNumber );

            int endLineNumber;
            int endColumn;

            if(compareEndLine > 0)
            {
                endLineNumber = other.EndLineNumber;
                endColumn     = other.EndColumn;
            }
            else
            {
                endLineNumber = this.EndLineNumber;

                 if(compareEndLine < 0)
                 {
                     endColumn = this.EndColumn;
                 }
                 else
                 {
                     endColumn = Math.Min( this.EndColumn, other.EndColumn );
                 }
            }

            if((beginLineNumber >  endLineNumber                            ) ||
               (beginLineNumber == endLineNumber && beginColumn >= endColumn)  )
            {
                return null;
            }

            return new DebugInfo( null, MethodName, beginLineNumber, beginColumn, endLineNumber, endColumn );
        }

        public bool IsContainedIn( DebugInfo other )
        {
            if(this.SrcFileName != other.SrcFileName)
            {
                return false;
            }

            int compareBeginLine = this.BeginLineNumber.CompareTo( other.BeginLineNumber );

            if(compareBeginLine < 0)
            {
                return false;
            }

            if(compareBeginLine == 0 && this.BeginColumn < other.BeginColumn)
            {
                return false;
            }

            //--//

            int compareEndLine = this.EndLineNumber.CompareTo( other.EndLineNumber );

            if(compareEndLine > 0)
            {
                return false;
            }

            if(compareEndLine == 0 && this.EndColumn > other.EndColumn)
            {
                return false;
            }

            return true;
        }

        //
        // Debug Methods
        //

        public override String ToString()
        {
            StringBuilder s = new StringBuilder();

            s.AppendFormat( "      ; [{0}:{1}-{2}:{3}] {4}", this.BeginLineNumber, this.BeginColumn, this.EndLineNumber, this.EndColumn, this.SrcFileName );

            return s.ToString();
        }
    }
}
