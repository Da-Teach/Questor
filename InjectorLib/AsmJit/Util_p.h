// AsmJit - Complete JIT Assembler for C++ Language.

// Copyright (c) 2008-2010, Petr Kobalicek <kobalicek.petr@gmail.com>
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

// [Guard]
#ifndef _ASMJIT_UTIL_P_H
#define _ASMJIT_UTIL_P_H

// [Dependencies]
#include "Util.h"

#include <stdlib.h>
#include <string.h>

namespace AsmJit {

//! @addtogroup AsmJit_Util
//! @{

// ============================================================================
// [AsmJit::Util]
// ============================================================================

namespace Util
{
  // --------------------------------------------------------------------------
  // [AsmJit::isIntX]
  // --------------------------------------------------------------------------

  //! @brief Returns @c true if a given integer @a x is signed 8-bit integer
  inline bool isInt8(sysint_t x) ASMJIT_NOTHROW { return x >= -128 && x <= 127; }
  //! @brief Returns @c true if a given integer @a x is unsigned 8-bit integer
  inline bool isUInt8(sysint_t x) ASMJIT_NOTHROW { return x >= 0 && x <= 255; }

  //! @brief Returns @c true if a given integer @a x is signed 16-bit integer
  inline bool isInt16(sysint_t x) ASMJIT_NOTHROW { return x >= -32768 && x <= 32767; }
  //! @brief Returns @c true if a given integer @a x is unsigned 16-bit integer
  inline bool isUInt16(sysint_t x) ASMJIT_NOTHROW { return x >= 0 && x <= 65535; }

  //! @brief Returns @c true if a given integer @a x is signed 16-bit integer
  inline bool isInt32(sysint_t x) ASMJIT_NOTHROW
  {
#if defined(ASMJIT_X86)
    return true;
#else
    return x >= ASMJIT_INT64_C(-2147483648) && x <= ASMJIT_INT64_C(2147483647);
#endif
  }
  //! @brief Returns @c true if a given integer @a x is unsigned 16-bit integer
  inline bool isUInt32(sysint_t x) ASMJIT_NOTHROW
  {
#if defined(ASMJIT_X86)
    return x >= 0;
#else
    return x >= 0 && x <= ASMJIT_INT64_C(4294967295);
#endif
  }

  // --------------------------------------------------------------------------
  // [AsmJit::floatAsInt32, int32AsFloat]
  // --------------------------------------------------------------------------

  //! @internal
  //!
  //! @brief used to cast from float to 32-bit integer and vica versa.
  union I32FPUnion
  {
    //! @brief 32-bit signed integer value.
    int32_t i;
    //! @brief 32-bit SP-FP value.
    float f;
  };

  //! @internal
  //!
  //! @brief used to cast from double to 64-bit integer and vica versa.
  union I64FPUnion
  {
    //! @brief 64-bit signed integer value.
    int64_t i;
    //! @brief 64-bit DP-FP value.
    double f;
  };

  //! @brief Binary cast from 32-bit integer to SP-FP value (@c float).
  inline float int32AsFloat(int32_t i) ASMJIT_NOTHROW
  {
    I32FPUnion u;
    u.i = i;
    return u.f;
  }

  //! @brief Binary cast SP-FP value (@c float) to 32-bit integer.
  inline int32_t floatAsInt32(float f) ASMJIT_NOTHROW
  {
    I32FPUnion u;
    u.f = f;
    return u.i;
  }

  //! @brief Binary cast from 64-bit integer to DP-FP value (@c double).
  inline double int64AsDouble(int64_t i) ASMJIT_NOTHROW
  {
    I64FPUnion u;
    u.i = i;
    return u.f;
  }

  //! @brief Binary cast from DP-FP value (@c double) to 64-bit integer.
  inline int64_t doubleAsInt64(double f) ASMJIT_NOTHROW
  {
    I64FPUnion u;
    u.f = f;
    return u.i;
  }

  // --------------------------------------------------------------------------
  // [Str Utils]
  // --------------------------------------------------------------------------

  char* mycpy(char* dst, const char* src, sysuint_t len = (sysuint_t)-1) ASMJIT_NOTHROW;
  char* myfill(char* dst, const int c, sysuint_t len) ASMJIT_NOTHROW;
  char* myhex(char* dst, const uint8_t* src, sysuint_t len) ASMJIT_NOTHROW;
  char* myutoa(char* dst, sysuint_t i, sysuint_t base = 10) ASMJIT_NOTHROW;
  char* myitoa(char* dst, sysint_t i, sysuint_t base = 10) ASMJIT_NOTHROW;

  // --------------------------------------------------------------------------
  // [Mem Utils]
  // --------------------------------------------------------------------------

  inline void memset32(uint32_t* p, uint32_t c, sysuint_t len)
  {
    sysuint_t i;
    for (i = 0; i < len; i++) p[i] = c;
  }
} // Util namespace

//! @}

} // AsmJit namespace

#endif // _ASMJIT_UTIL_P_H
