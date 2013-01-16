#region License/Copyright

// WhiteMagic - Injected .NET Helper Library
//     Copyright (C) 2009 Apoc
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using WhiteMagic.Internals;
using WhiteMagic.Native;

#if X64
using ADDR = System.UInt64;
#else
using ADDR = System.UInt32;
#endif

namespace WhiteMagic
{
    /*
     Notes on this project:
     * 
     * I am not using Marshal.Read/Write for good reasons. The main one, being the required security
     * contract permissions to actually have it work right. This may sometimes fail, regardless of whether
     * the end user has administrator permissions or not. Instead, I opted to use ReadProcessMemory and WriteProcessMemory
     * (contained in the Native.Win32 class) to do the actual reading/writing. There is no obvious overhead, so it
     * shouldn't be an issue either way. This class handles opening a proc handle, and closing it as well.
     * 
     * The Win32 API is much faster than the Marshal API, so there's another reason. (Granted; not a good one, but hey, this is
     * written mainly for those hacking games from C#)
     * 
     * This library DOES NOT HANDLE INJECTING INTO PROCESSES! You will need to write that functionality yourself,
     * as I'm far too lazy to write the injection library. (It's not that hard anyway...)
     * 
     * Major credit to Shynd, who's name I pretty much stole (from his BlackMagic lib, aimed towards out of proc editors)
     * 
     * Other credits:
     * 
     * kynox; being an awesome ausfag
     * Cypher: being a portability nazi kiwi (yes, this lib is mainly for x86, no x64 support is implied)
     * jjaa; a good bit of his code is used in this library, however, I've changed/rewritten a good chunk of it.
     * Shynd; the BlackMagic library that I used as a basis of.. well... even writing this one...
     * Everyone at GD; just plain awesome people.
     * MMOwned; 95% retarded dimwits, but some of you are awesome.     
     */

    /// <summary>
    /// The main memory library class. Just instantiate this class, and everything else you need is contained within.
    /// Alternatively, you can use the Magic.Instance property, to grab a static instance of this class (singleton)
    /// 
    /// I highly suggest tracking your own instances for fairly obvious reasons.
    /// </summary>
    public sealed class Magic : IDisposable
    {
        private static Magic _instance;
        private readonly DetourManager _detours;
        private readonly PatchManager _patches;
        private readonly PatternManager _patterns;
        private readonly Win32 _win32;

        /// <summary>
        /// Creates a new instance of the <see cref="Magic"/> class.
        /// </summary>
        public Magic()
        {
            Process.EnterDebugMode();
            _win32 = new Win32(IntPtr.Zero);
            _detours = new DetourManager(_win32);
            _patches = new PatchManager(_win32);
            _patterns = new PatternManager(_win32);
        }

        /// <summary>
        /// Retrieves a static instance of the <see cref="Magic"/> class. (A singleton)
        /// </summary>
        public static Magic Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Magic();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Provides access to the DetourManager class, that allows you to create and remove
        /// detours and hooks for functions. (Or any other use you may find...)
        /// </summary>
        public DetourManager Detours { get { return _detours; } }

        /// <summary>
        /// Provides access to the PatchManager class, which allows you to apply and remove patches.
        /// </summary>
        public PatchManager Patches { get { return _patches; } }

        /// <summary>
        /// Provides access to the PatternsManager class, which allows you to load, and search for patterns in memory.
        /// </summary>
        public PatternManager Patterns { get { return _patterns; } }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Process.LeaveDebugMode();
            _win32.Dispose();
            
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Read/Write

        private T ReadInternal<T>(IntPtr address)
        {
            object ret;

            // Handle types that don't have a real typecode
            // and/or can be done without the ReadByte bullshit
            if (typeof (T) == typeof (IntPtr))
            {
                ret = Marshal.ReadIntPtr(address);
                return (T) ret;
            }

            if (typeof (T) == typeof (string))
            {
                ret = Marshal.PtrToStringAnsi(address);
                return (T) ret;
            }

            int size = Marshal.SizeOf(typeof (T));
            byte[] ba = _win32.ReadBytes(address, size);

            switch (Type.GetTypeCode(typeof (T)))
            {
                case TypeCode.Boolean:
                    ret = BitConverter.ToBoolean(ba, 0);
                    break;
                case TypeCode.Char:
                    ret = BitConverter.ToChar(ba, 0);
                    break;
                case TypeCode.Byte:
                    ret = ba[0];
                    break;
                case TypeCode.Int16:
                    ret = BitConverter.ToInt16(ba, 0);
                    break;
                case TypeCode.UInt16:
                    ret = BitConverter.ToUInt16(ba, 0);
                    break;
                case TypeCode.Int32:
                    ret = BitConverter.ToInt32(ba, 0);
                    break;
                case TypeCode.UInt32:
                    ret = BitConverter.ToUInt32(ba, 0);
                    break;
                case TypeCode.Int64:
                    ret = BitConverter.ToInt64(ba, 0);
                    break;
                case TypeCode.UInt64:
                    ret = BitConverter.ToUInt64(ba, 0);
                    break;
                case TypeCode.Single:
                    ret = BitConverter.ToSingle(ba, 0);
                    break;
                case TypeCode.Double:
                    ret = BitConverter.ToDouble(ba, 0);
                    break;
                default:
                    throw new NotSupportedException(typeof (T).FullName + " is not currently supported by Read<T>");
            }
            return (T) ret;
        }

        /// <summary>
        /// Reads a specific number of bytes from memory.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(IntPtr address, int count)
        {
            return _win32.ReadBytes(address, count);
        }

        /// <summary>
        /// Reads a specific number of bytes from memory.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(ADDR address, int count)
        {
            return ReadBytes((IntPtr) address, count);
        }


        /// <summary>
        /// Reads a "T" from memory, using consecutive reading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public T Read<T>(params IntPtr[] addresses)
        {
            var tmp = new List<ADDR>();
            foreach (IntPtr ptr in addresses)
            {
                tmp.Add((ADDR) ptr.ToInt32());
            }
            return Read<T>(tmp.ToArray());
        }

        /// <summary>
        /// Reads a "T" from memory, using consecutive reading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public T Read<T>(params ADDR[] addresses)
        {
            if (addresses.Length == 0)
            {
                return default(T);
            }
            if (addresses.Length == 1)
            {
                return ReadInternal<T>((IntPtr) addresses[0]);
            }

            ADDR last = 0;
            for (int i = 0; i < addresses.Length; i++)
            {
                if (i == addresses.Length - 1)
                {
                    return ReadInternal<T>((IntPtr) (addresses[i] + last));
                }
                last = ReadInternal<ADDR>(new IntPtr(last + addresses[i]));
            }

            // Should never hit this.
            // The compiler just bitches.
            return default(T);
        }

        /// <summary>
        /// Reads a struct from memory. The struct must be attributed properly!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public T ReadStruct<T>(IntPtr address) where T : struct
        {
#if !NOEXCEPTIONS
            if (!Utilities.HasAttrib<StructLayoutAttribute>(typeof(T)))
                throw new MissingAttributeException("StructLayoutAttribute is missing.");
#endif

            return (T) Marshal.PtrToStructure(address, typeof (T));
        }

        /// <summary>
        /// Writes a set of bytes to memory.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool WriteBytes(IntPtr address, byte[] bytes)
        {
            return _win32.WriteBytes(address, bytes) == bytes.Length;
        }

        /// <summary>
        /// Writes a set of bytes to memory.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool WriteBytes(ADDR address, byte[] bytes)
        {
            return WriteBytes((IntPtr) address, bytes);
        }

        /// <summary>
        /// Writes a generic datatype to memory. (Note; only base datatypes are supported [int,float,uint,byte,sbyte,double,etc])
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write<T>(IntPtr address, T value)
        {
            // We can't handle strings yet...
            // Maybe in the future. Since strings require special treatment
            // depending on the context. (Can we just write the string bytes? Do we need to write
            // them in a cave somewhere, and write the pointer to the cave somewhere else?
            // What type of string should be written? Etc.)

            // Just handle writing strings yourselves!
            if (typeof (T) == typeof (string))
            {
                throw new ArgumentException("Writing strings are not currently supported!", "value");
            }

            try
            {
                object val = value;

                byte[] bytes;

                // Handle types that don't have a real typecode
                // and/or can be done without the ReadByte bullshit
                if (typeof (T) == typeof (IntPtr))
                {
                    // Since we're already here...... we might as well do some stuffs.
                    Marshal.WriteIntPtr(address, (IntPtr) val);
                    return true;
                }

                // Make sure we're handling passing in stuff as a byte array.
                if (typeof (T) == typeof (byte[]))
                {
                    bytes = (byte[]) val;
                    return _win32.WriteBytes(address, bytes) == bytes.Length;
                }
                switch (Type.GetTypeCode(typeof (T)))
                {
                    case TypeCode.Boolean:
                        bytes = BitConverter.GetBytes((bool) val);
                        break;
                    case TypeCode.Char:
                        bytes = BitConverter.GetBytes((char) val);
                        break;
                    case TypeCode.Byte:
                        bytes = new[] {(byte) val};
                        break;
                    case TypeCode.Int16:
                        bytes = BitConverter.GetBytes((short) val);
                        break;
                    case TypeCode.UInt16:
                        bytes = BitConverter.GetBytes((ushort) val);
                        break;
                    case TypeCode.Int32:
                        bytes = BitConverter.GetBytes((int) val);
                        break;
                    case TypeCode.UInt32:
                        bytes = BitConverter.GetBytes((uint) val);
                        break;
                    case TypeCode.Int64:
                        bytes = BitConverter.GetBytes((long) val);
                        break;
                    case TypeCode.UInt64:
                        bytes = BitConverter.GetBytes((ulong) val);
                        break;
                    case TypeCode.Single:
                        bytes = BitConverter.GetBytes((float) val);
                        break;
                    case TypeCode.Double:
                        bytes = BitConverter.GetBytes((double) val);
                        break;
                    default:
                        throw new NotSupportedException(typeof (T).FullName + " is not currently supported by Write<T>");
                }
                return _win32.WriteBytes(address, bytes) == bytes.Length;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Writes a generic datatype to memory. (Note; only base datatypes are supported [int,float,uint,byte,sbyte,double,etc])
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Write<T>(ADDR address, T value)
        {
            return Write((IntPtr) address, value);
        }

        /// <summary>
        /// Writes a struct to memory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStruct<T>(IntPtr address, T value) where T : struct
        {
#if !NOEXCEPTIONS
            if (!Utilities.HasAttrib<StructLayoutAttribute>(typeof(T)))
                throw new MissingAttributeException("StructLayoutAttribute is missing.");
#endif

            try
            {
                Marshal.StructureToPtr(value, address, true);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a struct to memory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteStruct<T>(ADDR address, T value) where T : struct
        {
            return WriteStruct((IntPtr) address, value);
        }

        #endregion

        #region VTable Stuff

        /// <summary>
        /// Retrieves a function pointer, to an objects virtual function table.
        /// </summary>
        /// <param name="objectBase">The pointer to the object.</param>
        /// <param name="functionIndex">The nth function to retrieve.</param>
        /// <returns></returns>
        public IntPtr GetObjectVtableFunction(IntPtr objectBase, uint functionIndex)
        {
            // [[objBase]+4*vfuncIdx]
            return Read<IntPtr>((ADDR) objectBase, functionIndex * 4);
        }

        #endregion

        #region Delegate/Function Registration

        /// <summary>
        /// Registers a function into a delegate. Note: The delegate must provide a proper function signature!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public T RegisterDelegate<T>(ADDR address) where T : class
        {
            return RegisterDelegate<T>((IntPtr) address);
        }

        /// <summary>
        /// Registers a function into a delegate. Note: The delegate must provide a proper function signature!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public T RegisterDelegate<T>(IntPtr address) where T : class
        {
#if !NOEXCEPTIONS
            // Make sure delegates are attributed properly!
            if (!Utilities.HasUFPAttribute(typeof (T)))
            {
                throw new ArgumentException("The delegate does not have proper attributes! It must be adorned with" +
                                            " the System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute, with proper calling conventions" +
                                            " to work properly!");
            }
#endif
            return Marshal.GetDelegateForFunctionPointer(address, typeof (T)) as T;
        }

        #endregion

        #region LoadLib/GetProcAddr/FreeLib

        /// <summary>
        /// Win32 LoadLibrary
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IntPtr LoadLibrary(string fileName)
        {
            return Win32.LoadLibrary(fileName);
        }

        /// <summary>
        /// Win32 FreeLibrary
        /// </summary>
        /// <param name="hLib"></param>
        /// <returns></returns>
        public bool FreeLibrary(IntPtr hLib)
        {
            return Win32.FreeLibrary(hLib);
        }

        /// <summary>
        /// Win32 GetProcAddress
        /// </summary>
        /// <param name="hModule"></param>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public IntPtr GetProcAddress(IntPtr hModule, string procedureName)
        {
            return Win32.GetProcAddress(hModule, procedureName);
        }

        /// <summary>
        /// Loads a library, and gets a procedure address.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="procedureName"></param>
        /// <returns></returns>
        public IntPtr GetProcAddress(string fileName, string procedureName)
        {
            return GetProcAddress(LoadLibrary(fileName), procedureName);
        }

        #endregion
    }
}