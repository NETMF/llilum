//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#include "mbed_helpers.h" 

//--//

extern "C"
{

    // Must match consts defined in ObjectHeader.cs
    #define REFERENCE_COUNT_MASK  0xFF000000
    #define REFERENCE_COUNT_SHIFT 24

    struct ObjectHeader
    {
        int32_t MultiUseWord;
        void* VTable;
    };

    // Helpers for starting / ending section of code that needs to be atomic
    __attribute__((always_inline)) __STATIC_INLINE void StartAtomicOperations(void)
    {
        __set_PRIMASK(1);
    }

    __attribute__((always_inline)) __STATIC_INLINE void EndAtomicOperations(void)
    {
        __set_PRIMASK(0);
    }

    __attribute__((always_inline)) __STATIC_INLINE void AddReferenceFast(ObjectHeader* target)
    {
        if (target != NULL && (target->MultiUseWord & REFERENCE_COUNT_MASK))
        {
            target->MultiUseWord += (1 << REFERENCE_COUNT_SHIFT);
        }
    }

    void AddReference(ObjectHeader* target)
    {
        if (target != NULL)
        {
            StartAtomicOperations();

            if (target->MultiUseWord & REFERENCE_COUNT_MASK)
            {
                target->MultiUseWord += (1 << REFERENCE_COUNT_SHIFT);
            }

            EndAtomicOperations();
        }
    }

    // Return zero when target is dead after the call
    int ReleaseReferenceNative(ObjectHeader* target)
    {
        int ret = 1;
        if (target != NULL)
        {
            StartAtomicOperations();

            int32_t value = target->MultiUseWord;
            if (value & REFERENCE_COUNT_MASK)
            {
                value -= (1 << REFERENCE_COUNT_SHIFT);
                target->MultiUseWord = value;
                ret = value & REFERENCE_COUNT_MASK;
            }

            EndAtomicOperations();
        }

        return ret;
    }

    ObjectHeader* LoadAndAddReferenceNative(ObjectHeader** target)
    {
        StartAtomicOperations();

        ObjectHeader* value = *target;
        AddReferenceFast(value);
        
        EndAtomicOperations();

        return value;
    }

    ObjectHeader* ReferenceCountingExchange(ObjectHeader** target, ObjectHeader* value)
    {
        StartAtomicOperations();

        ObjectHeader* oldValue = *target;
        *target = value;
        AddReferenceFast(value);

        EndAtomicOperations();

        return oldValue;
    }

    ObjectHeader* ReferenceCountingCompareExchange(ObjectHeader** target, ObjectHeader* value, ObjectHeader* comparand)
    {
        StartAtomicOperations();

        ObjectHeader* oldValue = *target;
        ObjectHeader* addRefTarget;
        if (oldValue == comparand)
        {
            *target = value;

            // Compare exchange succeeded, we need to add ref the new value
            // The old value's ref count will be passed back to the caller on return.
            addRefTarget = value;
        }
        else
        {
            // Target is not changed, we need to add ref the old value so it has
            // a ref count to pass back to caller on return
            addRefTarget = oldValue;
        }
        
        AddReferenceFast(addRefTarget);

        EndAtomicOperations();

        return oldValue;
    }
}
