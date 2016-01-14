using Llvm.NET.Types;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Llvm.NET.DebugInfo
{
    public class DebugStructType
        : DebugType<IStructType, DICompositeType>
        , IStructType
    {
        public DebugStructType( IStructType llvmType
                              , NativeModule module
                              , DIScope scope
                              , string name
                              , DIFile file
                              , uint line
                              , DebugInfoFlags debugFlags
                              , DIType derivedFrom
                              , IEnumerable<DIType> elements
                              )
            : base( llvmType
                  , module.VerifyArgNotNull( nameof( module ) )
                          .DIBuilder
                          .CreateStructType( scope
                                           , name
                                           , file
                                           , line
                                           , module.VerifyArgNotNull( nameof( module ) ).Layout.BitSizeOf( llvmType )
                                           , module.VerifyArgNotNull( nameof( module ) ).Layout.AbiBitAlignmentOf( llvmType )
                                           , debugFlags
                                           , derivedFrom
                                           , elements
                                           )
                  )
        {
        }

        public DebugStructType( IStructType llvmType
                              , NativeModule module
                              , DIScope scope
                              , string name
                              , DIFile file
                              , uint line
                              )
            : base( llvmType
                  , module.VerifyArgNotNull( nameof( module ) )
                          .DIBuilder
                          .CreateReplaceableCompositeType( Tag.StructureType
                                                         , name
                                                         , scope
                                                         , file
                                                         , line
                                                         )
                  )
        {
        }

        public DebugStructType( NativeModule module
                              , string nativeName
                              , DIScope scope
                              , string name
                              , DIFile file = null
                              , uint line = 0
                              )
            : this( module.VerifyArgNotNull( nameof( module ) ).Context.CreateStructType( nativeName ), module, scope, name, file, line )
        {
        }

        public bool IsOpaque => NativeType.IsOpaque;

        public bool IsPacked => NativeType.IsPacked;

        public IReadOnlyList<ITypeRef> Members => NativeType.Members;

        public string Name => NativeType.Name;

        public void SetBody( bool packed, params ITypeRef[ ] elements )
        {
            NativeType.SetBody( packed, elements );
        }

        public void SetBody( bool packed
                           , NativeModule module
                           , DIScope scope
                           , DIFile diFile
                           , uint line
                           , DebugInfoFlags debugFlags
                           , IEnumerable<DebugMemberInfo> debugElements
                           )
        {
            var debugMembersArray = debugElements as IList<DebugMemberInfo> ?? debugElements.ToList();
            var nativeElements = debugMembersArray.Select( e => e.DebugType.NativeType );
            SetBody( packed, module, scope, diFile, line, debugFlags, nativeElements, debugMembersArray );
        }

        public void SetBody( bool packed
                           , NativeModule module
                           , DIScope scope
                           , DIFile diFile
                           , uint line
                           , DebugInfoFlags debugFlags
                           , IEnumerable<ITypeRef> nativeElements
                           , IEnumerable<DebugMemberInfo> debugElements
                           , uint? bitSize = null
                           , uint? bitAlignment = null
                           )
        {
            DebugMembers = new ReadOnlyCollection<DebugMemberInfo>( debugElements as IList<DebugMemberInfo> ?? debugElements.ToList( ) );
            SetBody( packed, nativeElements.ToArray() );
            var memberTypes = from memberInfo in DebugMembers
                              select CreateMemberType( module, memberInfo );

            var concreteType = module.DIBuilder.CreateStructType( scope: scope
                                                                , name: DIType.Name
                                                                , file: diFile
                                                                , line: line
                                                                , bitSize: bitSize ?? module.Layout.BitSizeOf( NativeType )
                                                                , bitAlign: bitAlignment ?? module.Layout.AbiBitAlignmentOf( NativeType )
                                                                , debugFlags: debugFlags
                                                                , derivedFrom: null
                                                                , elements: memberTypes
                                                                );
            DIType = concreteType;
        }

        private DIDerivedType CreateMemberType( NativeModule module, DebugMemberInfo memberInfo )
        {
            ulong bitSize;
            ulong bitAlign;
            ulong bitOffset;

            // if explicit layout info provided, use it;
            // otherwise use module.Layout as the default
            if( memberInfo.ExplicitLayout != null )
            {
                bitSize = memberInfo.ExplicitLayout.BitSize;
                bitAlign = memberInfo.ExplicitLayout.BitAlignment;
                bitOffset = memberInfo.ExplicitLayout.BitOffset;
            }
            else
            {
                bitSize = module.Layout.BitSizeOf( memberInfo.DebugType.NativeType );
                bitAlign = module.Layout.AbiBitAlignmentOf( memberInfo.DebugType.NativeType );
                bitOffset = module.Layout.BitOffsetOfElement( NativeType, memberInfo.Index );
            }

            return module.DIBuilder.CreateMemberType( scope: DIType
                                                    , name: memberInfo.Name
                                                    , file: memberInfo.File
                                                    , line: memberInfo.Line
                                                    , bitSize: bitSize
                                                    , bitAlign: bitAlign
                                                    , bitOffset: bitOffset
                                                    , debugFlags: memberInfo.DebugInfoFlags
                                                    , type: memberInfo.DebugType.DIType
                                                    );
        }

        public IReadOnlyList<DebugMemberInfo> DebugMembers { get; private set; }
    }
}
