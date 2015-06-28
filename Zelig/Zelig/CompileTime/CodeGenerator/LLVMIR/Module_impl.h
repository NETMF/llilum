#pragma once
#include "LLVMHeaders.h"

//--//

NS_MICROSOFT
NS_ZELIG
NS_LLVM

//--//

/*
 * Holds the LLVM builder, the module and the context, and also
 * the debug information 
 *
 */
class Module_impl
{
private:
    IRBuilder<>* _pB;
    LLVMContext* _pC;
    Module*	  _pM;
    unsigned _nativeIntSize;

    //
    // Global values store
    // 
    std::map<std::string, llvm::Value*>     globals;

    // 
    // Debu info store
    // 

    std::map<std::string, DIFile* >         dIFiles;
    std::map<std::string, DISubprogram* >   dISubprograms;

public:
    //
    // Contructors
    // 

    Module_impl( const char* assembly, unsigned nativeIntSize );

    //
    // Public methods 
    // 

    llvm::BasicBlock* InsertBlock( llvm::Function* fp, string name );

    //
    // Debug 
    // 
    void            SetCurrentDIFile( std::string fn );
    DISubprogram*   GetDISubprogram( std::string fn );
    void            SetDISubprogram( std::string fn, DISubprogram* disub );
    

    unsigned GetNativeIntSize( );
    unsigned GetPointerSize( );

    llvm::Module* GetLLVMObject( );
    bool Compile( );
    bool DumpToFile( string filename, bool text );

    //
    // Debug state 
    // 
    DIBuilder*      dib;
    DICompileUnit   dicu;
    DIFile*         curDIFile;
};

//--//

NS_END
NS_END
NS_END

