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
                              , Module module
                              , DIScope scope
                              , string name
                              , DIFile file
                              , uint line
                              , DebugInfoFlags flags
                              , DIType derivedFrom
                              , IEnumerable<DIType> elements
                              )
            : base( llvmType
                  , module.DIBuilder.CreateStructType( scope
                                                     , name
                                                     , file
                                                     , line
                                                     , module.Layout.BitSizeOf( llvmType )
                                                     , module.Layout.AbiBitAlignmentOf( llvmType )
                                                     , ( uint )flags
                                                     , derivedFrom
                                                     , elements
                                                     )
                  )
        {
        }

        public DebugStructType( IStructType llvmType
                              , Module module
                              , DIScope scope
                              , string name
                              , DIFile file
                              , uint line
                              )
            : base( llvmType
                  , module.DIBuilder.CreateReplaceableCompositeType( Tag.StructureType
                                                                   , name
                                                                   , scope
                                                                   , file
                                                                   , line
                                                                   )
                  )
        {
        }

        public DebugStructType( Module module
                              , string nativeName
                              , DIScope scope
                              , string name
                              , DIFile file = null
                              , uint line = 0
                              )
            : this( module.Context.CreateStructType( nativeName ), module, scope, name, file, line )
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
                           , Module module
                           , DIScope scope
                           , DIFile diFile
                           , uint line
                           , DebugInfoFlags flags
                           , IEnumerable<DebugMemberInfo> debugElements
                           )
        {
            var debugMembersArray = debugElements as IList<DebugMemberInfo> ?? debugElements.ToList();
            var nativeElements = debugMembersArray.Select( e => e.Type.NativeType );
            SetBody( packed, module, scope, diFile, line, flags, nativeElements, debugMembersArray );
        }

        public void SetBody( bool packed
                           , Module module
                           , DIScope scope
                           , DIFile diFile
                           , uint line
                           , DebugInfoFlags flags
                           , IEnumerable<ITypeRef> nativeElements
                           , IEnumerable<DebugMemberInfo> debugelements
                           , uint? bitSize = null
                           , uint? bitAlignment = null
                           )
        {
            DebugMembers = new ReadOnlyCollection<DebugMemberInfo>( debugelements as IList<DebugMemberInfo> ?? debugelements.ToList( ) );
            SetBody( packed, nativeElements.ToArray() );
            var memberTypes = from memberInfo in DebugMembers
                              select CreateMemberType( module, memberInfo );

            var concreteType = module.DIBuilder.CreateStructType( scope: scope
                                                                , name: DIType.Name
                                                                , file: diFile
                                                                , line: line
                                                                , bitSize: bitSize ?? module.Layout.BitSizeOf( NativeType )
                                                                , bitAlign: bitAlignment ?? module.Layout.AbiBitAlignmentOf( NativeType )
                                                                , flags: (uint)flags
                                                                , derivedFrom: null
                                                                , elements: memberTypes
                                                                );
            DIType = concreteType;
        }

        private DIDerivedType CreateMemberType( Module module, DebugMemberInfo memberInfo )
        {
            return module.DIBuilder.CreateMemberType( scope: DIType
                                                    , name: memberInfo.Name
                                                    , file: memberInfo.File
                                                    , line: memberInfo.Line
                                                    , bitSize: memberInfo.BitSize ?? module.Layout.BitSizeOf( memberInfo.Type.NativeType )
                                                    , bitAlign: memberInfo.BitAlignment ?? module.Layout.AbiBitAlignmentOf( memberInfo.Type.NativeType )
                                                    , bitOffset: memberInfo.BitOffset ?? module.Layout.BitOffsetOfElement( NativeType, memberInfo.Index )
                                                    , flags: (uint)memberInfo.Flags
                                                    , type: memberInfo.Type.DIType
                                                    );
        }

        public IReadOnlyList<DebugMemberInfo> DebugMembers { get; private set; }
    }
}
