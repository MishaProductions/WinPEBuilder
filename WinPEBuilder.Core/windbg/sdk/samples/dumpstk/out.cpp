//----------------------------------------------------------------------------
//
// stdio-based output callbacks class.
//
// Copyright (C) Microsoft Corporation, 2000.
//
//----------------------------------------------------------------------------

#include <stdio.h>
#include <windows.h>
#include <dbgeng.h>

#include "out.hpp"

StdioOutputCallbacks g_OutputCb;

STDMETHODIMP
StdioOutputCallbacks::QueryInterface(
    THIS_
    _In_ REFIID InterfaceId,
    _Out_ PVOID* Interface
    )
{
    *Interface = NULL;

    if (IsEqualIID(InterfaceId, __uuidof(IUnknown)) ||
        IsEqualIID(InterfaceId, __uuidof(IDebugOutputCallbacks)))
    {
        *Interface = (IDebugOutputCallbacks *)this;
        AddRef();
        return S_OK;
    }
    else
    {
        return E_NOINTERFACE;
    }
}

STDMETHODIMP_(ULONG)
StdioOutputCallbacks::AddRef(
    THIS
    )
{
    // This class is designed to be static so
    // there's no true refcount.
    return 1;
}

STDMETHODIMP_(ULONG)
StdioOutputCallbacks::Release(
    THIS
    )
{
    // This class is designed to be static so
    // there's no true refcount.
    return 0;
}

STDMETHODIMP
StdioOutputCallbacks::Output(
    THIS_
    _In_ ULONG Mask,
    _In_ PCSTR Text
    )
{
    UNREFERENCED_PARAMETER(Mask);
    fputs(Text, stdout);
    return S_OK;
}
