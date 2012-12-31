// EnsureCleanup - Windows RAII helper library
// By RaptorFactor.
// http://www.raptorfactor.com/ensurecleanup-gcc-support-move-semantics/
//
// EnsureRelease and EnsureVirtualFree added by _Mike, don't blame RaptorFactor if they break something.
 
#pragma once
 
// Windows API
#include <Windows.h>
#include <Objbase.h>
 
// C++ Standard Library
#include <string>
 
// Boost::noncopyable
#include "noncopyable.hpp"
 
// Notice: Modified version of EnsureCleanup library provided in the 'Windows
// via C/C++' sample code. Originally copyright Jeffrey Richter and
// Christophe Nasarre.
 
namespace Hades
{
  namespace Windows
  {
    // Windows RAII helper class template
    // HandleT = Handle type (e.g. 'HANDLE')
    // FuncT = Function prototype (e.g. 'BOOL (WINAPI*) (HANDLE)' )
    // CleanupFn = Cleanup function (e.g. 'CloseHandle')
    // Invalid = Invalid handle value (e.g. '0')
    // Note: Invalid is of type DWORD_PTR and not HandleT due to a GCC error
    template <typename HandleT, typename FuncT, FuncT CleanupFn,
      DWORD_PTR Invalid>
    class EnsureCleanup : private boost::noncopyable
    {
    public:
      // Ensure size of handle type is valid. Under Windows all handles are
      // the size of a pointer.
      static_assert(sizeof(HandleT) == sizeof(DWORD_PTR),
        "Size of handle type is invalid.");
 
      // Constructor
      EnsureCleanup(HandleT Handle = reinterpret_cast<HandleT>(Invalid))
        : m_Handle(Handle)
      { }
 
      // Move constructor
      EnsureCleanup(EnsureCleanup&& MyEnsureCleanup)
        : m_Handle(reinterpret_cast<HandleT>(Invalid))
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureCleanup& operator= (EnsureCleanup&& MyEnsureCleanup)
      {
        Cleanup();
 
        this->m_Handle = MyEnsureCleanup.m_Handle;
 
        MyEnsureCleanup.m_Handle = reinterpret_cast<HandleT>(Invalid);
 
        return *this;
      }
 
      // Assignment operator (for HandleT values)
      EnsureCleanup& operator= (HandleT Handle)
      {
        Cleanup();
 
        m_Handle = Handle;
 
        return *this;
      }
 
      // The destructor performs the cleanup.
      ~EnsureCleanup()
      {
        Cleanup();
      }
 
      // Whether object is valid
      BOOL IsValid() const
      {
        return m_Handle != reinterpret_cast<HandleT>(Invalid);
      }
 
      // Whether object is invalid
      BOOL IsInvalid() const
      {
        return !IsValid();
      }
 
      // Implicit conversion operator for HandleT
      operator HandleT() const
      {
        return m_Handle;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (IsValid())
        {
          // Close the object.
          CleanupFn(m_Handle);
 
          // We no longer represent a valid object.
          m_Handle = reinterpret_cast<HandleT>(Invalid);
        }
      }
 
    private:
      // Handle being managed
      HandleT m_Handle;
    };
 
    // Redefining 'INVALID_HANDLE_VALUE' due to a GCC error
    namespace
    {
      DWORD_PTR const INVALID_HANDLE_VALUE_WRAP = static_cast<DWORD_PTR>(-1);
    }
 
    // Instances of the template C++ class for common data types.
    typedef EnsureCleanup<HANDLE, BOOL(WINAPI*)(HANDLE), FindClose, 0>
      EnsureFindClose;
    typedef EnsureCleanup<HANDLE, BOOL(WINAPI*)(HANDLE), CloseHandle, 0>
      EnsureCloseHandle;
    typedef EnsureCleanup<HANDLE, BOOL(WINAPI*)(HANDLE), CloseHandle,
      INVALID_HANDLE_VALUE_WRAP> EnsureCloseSnap;
    typedef EnsureCleanup<HLOCAL, HLOCAL(WINAPI*)(HLOCAL), LocalFree, 0>
      EnsureLocalFree;
    typedef EnsureCleanup<HGLOBAL, HGLOBAL(WINAPI*)(HGLOBAL), GlobalFree, 0>
      EnsureGlobalFree;
    typedef EnsureCleanup<HGLOBAL, BOOL(WINAPI*)(HGLOBAL), GlobalUnlock, 0>
      EnsureGlobalUnlock;
    typedef EnsureCleanup<HKEY, LONG(WINAPI*)(HKEY), RegCloseKey, 0>
      EnsureRegCloseKey;
    typedef EnsureCleanup<SC_HANDLE, BOOL(WINAPI*)(SC_HANDLE),
      CloseServiceHandle, 0> EnsureCloseServiceHandle;
    typedef EnsureCleanup<HWINSTA, BOOL(WINAPI*)(HWINSTA), CloseWindowStation,
      0> EnsureCloseWindowStation;
    typedef EnsureCleanup<HDESK, BOOL(WINAPI*)(HDESK), CloseDesktop, 0>
      EnsureCloseDesktop;
    typedef EnsureCleanup<LPCVOID, BOOL(WINAPI*)(LPCVOID), UnmapViewOfFile, 0>
      EnsureUnmapViewOfFile;
    typedef EnsureCleanup<HMODULE, BOOL(WINAPI*)(HMODULE), FreeLibrary, 0>
      EnsureFreeLibrary;
    typedef EnsureCleanup<PVOID, ULONG(WINAPI*)(PVOID),
      RemoveVectoredExceptionHandler, 0> EnsureRemoveVEH;
    typedef EnsureCleanup<HANDLE, DWORD(WINAPI*)(HANDLE), ResumeThread, 0>
      EnsureResumeThread;
    typedef EnsureCleanup<HANDLE, BOOL(WINAPI*)(HANDLE), CloseHandle,
      INVALID_HANDLE_VALUE_WRAP> EnsureCloseFile;
    typedef EnsureCleanup<HHOOK, BOOL(WINAPI*)(HHOOK), UnhookWindowsHookEx, 0>
      EnsureUnhookWindowsHookEx;
    typedef EnsureCleanup<HWND, BOOL(WINAPI*)(HWND), DestroyWindow, 0>
      EnsureDestroyWindow;
    typedef EnsureCleanup<PSID, PVOID(WINAPI*)(PSID), FreeSid, 0>
      EnsureFreeSid;
    typedef EnsureCleanup<HGLOBAL, BOOL(WINAPI*)(HGLOBAL), FreeResource, 0>
      EnsureFreeResource;
    typedef EnsureCleanup<HDC, BOOL(WINAPI*)(HDC), DeleteDC, 0>
      EnsureDeleteDc;
    typedef EnsureCleanup<HBITMAP, BOOL(WINAPI*)(HGDIOBJ), DeleteObject, 0>
      EnsureDeleteObject;
    typedef EnsureCleanup<HICON, BOOL(WINAPI*)(HICON), DestroyIcon, 0>
      EnsureDestroyIcon;
    typedef EnsureCleanup<HMENU, BOOL(WINAPI*)(HMENU), DestroyMenu, 0>
      EnsureDestroyMenu;
 
    // Special class for ensuring COM is uninitialized
    class EnsureCoUninitialize : private boost::noncopyable
    {
    public:
      ~EnsureCoUninitialize()
      {
        CoUninitialize();
      }
    };
 
    // Special class for releasing a reserved region.
    class EnsureReleaseRegion : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureReleaseRegion(PVOID pv = NULL)
        : m_pv(pv)
      { }
 
      // Destructor
      ~EnsureReleaseRegion()
      {
        Cleanup();
      }
 
      // Move constructor
      EnsureReleaseRegion(EnsureReleaseRegion&& MyEnsureCleanup)
        : m_pv(NULL)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureReleaseRegion& operator= (EnsureReleaseRegion&& MyEnsureCleanup)
      {
        Cleanup();
 
        m_pv = MyEnsureCleanup.m_pv;
 
        MyEnsureCleanup.m_pv = 0;
 
        return *this;
      }
 
      // Assignment operator (for PVOID values)
      EnsureReleaseRegion& operator= (PVOID pv)
      {
        Cleanup();
 
        m_pv = pv;
 
        return *this;
      }
 
      // Implicit conversion operator for PVOID
      operator PVOID() const
      {
        return m_pv;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_pv != NULL)
        {
          VirtualFree(m_pv, 0, MEM_RELEASE);
 
          m_pv = NULL;
        }
      }
 
    private:
      // Handle being managed
      PVOID m_pv;
    };
 
    // Special class for releasing a reserved region.
    class EnsureEndUpdateResource : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureEndUpdateResource(HANDLE File = NULL)
        : m_File(File)
      { }
 
      // Move constructor
      EnsureEndUpdateResource(EnsureEndUpdateResource&& MyEnsureCleanup)
        : m_File(NULL)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureEndUpdateResource& operator= (EnsureEndUpdateResource&&
        MyEnsureCleanup)
      {
        Cleanup();
 
        m_File = MyEnsureCleanup.m_File;
 
        MyEnsureCleanup.m_File = NULL;
 
        return *this;
      }
 
      // Destructor
      ~EnsureEndUpdateResource()
      {
        Cleanup();
      }
 
      // Assignment operator (for HANDLE values)
      EnsureEndUpdateResource& operator= (HANDLE File)
      {
        Cleanup();
 
        m_File = File;
 
        return *this;
      }
 
      // Implicit conversion operator for HANDLE
      operator HANDLE() const
      {
        return m_File;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_File != NULL)
        {
          EndUpdateResource(m_File, FALSE);
 
          m_File = NULL;
        }
      }
 
    private:
      // Handle being managed
      HANDLE m_File;
    };
 
    // Special class for freeing a block from a heap
    class EnsureHeapFree : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureHeapFree(PVOID pv = NULL, HANDLE hHeap = GetProcessHeap())
        : m_pv(pv), m_hHeap(hHeap)
      { }
 
      // Move constructor
      EnsureHeapFree(EnsureHeapFree&& MyEnsureCleanup)
        : m_pv(NULL),
        m_hHeap(NULL)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureHeapFree& operator= (EnsureHeapFree&& MyEnsureCleanup)
      {
        Cleanup();
 
        m_pv = MyEnsureCleanup.m_pv;
        m_hHeap = MyEnsureCleanup.m_hHeap;
 
        MyEnsureCleanup.m_pv = NULL;
        MyEnsureCleanup.m_hHeap = NULL;
 
        return *this;
      }
 
      // Destructor
      ~EnsureHeapFree()
      {
        Cleanup();
      }
 
      // Assignment operator (for PVOID values)
      EnsureHeapFree& operator= (PVOID pv)
      {
        Cleanup();
 
        m_pv = pv;
 
        return *this;
      }
 
      // Implicit conversion operator for PVOID
      operator PVOID() const
      {
        return m_pv;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_pv != NULL)
        {
          HeapFree(m_hHeap, 0, m_pv);
 
          m_pv = NULL;
        }
      }
 
    private:
      // Handles being managed
      PVOID m_pv;
      HANDLE m_hHeap;
    };
 
    // Special class for releasing a remote reserved region
    class EnsureReleaseRegionEx : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureReleaseRegionEx(PVOID pv = NULL, HANDLE proc = NULL)
        : m_pv(pv),
        m_proc(proc)
      { }
 
      // Move constructor
      EnsureReleaseRegionEx(EnsureReleaseRegionEx&& MyEnsureCleanup)
        : m_pv(NULL),
        m_proc(NULL)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureReleaseRegionEx& operator= (EnsureReleaseRegionEx&&
        MyEnsureCleanup)
      {
        Cleanup();
 
        m_pv = MyEnsureCleanup.m_pv;
        m_proc = MyEnsureCleanup.m_proc;
 
        MyEnsureCleanup.m_pv = NULL;
        MyEnsureCleanup.m_proc = NULL;
 
        return *this;
      }
 
      // Destructor
      ~EnsureReleaseRegionEx()
      {
        Cleanup();
      }
 
      // Assignment operator (for PVOID values)
      EnsureReleaseRegionEx& operator= (PVOID pv)
      {
        Cleanup();
 
        m_pv = pv;
 
        return *this;
      }
 
      // Implicit conversion operator for PVOID
      operator PVOID() const
      {
        return m_pv;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_pv != NULL && m_proc != NULL)
        {
          VirtualFreeEx(m_proc, m_pv, 0, MEM_RELEASE);
 
          m_pv = NULL;
        }
      }
 
    private:
      // Handles being managed
      PVOID m_pv;
      HANDLE m_proc;
    };
 
    // Special class for closing the clipboard
    class EnsureCloseClipboard : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureCloseClipboard(BOOL Success)
        : m_Success(Success)
      { }
 
      // Move constructor
      EnsureCloseClipboard(EnsureCloseClipboard&& MyEnsureCleanup)
        : m_Success(FALSE)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureCloseClipboard& operator= (EnsureCloseClipboard&& MyEnsureCleanup)
      {
        Cleanup();
 
        m_Success = MyEnsureCleanup.m_Success;
 
        MyEnsureCleanup.m_Success = FALSE;
 
        return *this;
      }
 
      // Destructor
      ~EnsureCloseClipboard()
      {
        Cleanup();
      }
 
      // Assignment operator (for BOOL values)
      EnsureCloseClipboard& operator= (BOOL Success)
      {
        Cleanup();
 
        m_Success = Success;
 
        return *this;
      }
 
      // Implicit conversion operator for BOOL
      operator BOOL() const
      {
        return m_Success;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_Success)
        {
          CloseClipboard();
 
          m_Success = FALSE;
        }
      }
 
    private:
      // 'Handle' being managed
      BOOL m_Success;
    };
 
    // Special class for releasing a window class
    class EnsureUnregisterClassW : private boost::noncopyable
    {
    public:
      // Constructor
      EnsureUnregisterClassW(const std::wstring& ClassName, HINSTANCE Instance)
        : m_ClassName(ClassName),
        m_Instance(Instance)
      { }
 
      // Move constructor
      EnsureUnregisterClassW(EnsureUnregisterClassW&& MyEnsureCleanup)
        : m_ClassName(),
        m_Instance()
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureUnregisterClassW& operator= (EnsureUnregisterClassW&&
        MyEnsureCleanup)
      {
        Cleanup();
 
        m_ClassName = std::move(MyEnsureCleanup.m_ClassName);
        m_Instance = MyEnsureCleanup.m_Instance;
 
        MyEnsureCleanup.m_ClassName = std::wstring();
        MyEnsureCleanup.m_Instance = NULL;
 
        return *this;
      }
 
      // Destructor
      ~EnsureUnregisterClassW()
      {
        Cleanup();
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (!m_ClassName.empty() && m_Instance)
        {
          UnregisterClassW(m_ClassName.c_str(), m_Instance);
 
          m_ClassName.clear();
          m_Instance = 0;
        }
      }
 
    private:
      // 'Handles' being managed
      std::wstring m_ClassName;
      HINSTANCE m_Instance;
    };
 
    // Special class for releasing a DC
    class EnsureReleaseDc
    {
    public:
      // Constructor
      EnsureReleaseDc(HWND Wnd = NULL, HDC Dc = NULL)
        : m_Wnd(Wnd),
        m_Dc(Dc)
      { }
 
      // Move constructor
      EnsureReleaseDc(EnsureReleaseDc&& MyEnsureCleanup)
        : m_Wnd(NULL),
        m_Dc(NULL)
      {
        *this = std::move(MyEnsureCleanup);
      }
 
      // Move assignment operator
      EnsureReleaseDc& operator= (EnsureReleaseDc&& MyEnsureCleanup)
      {
        Cleanup();
 
        m_Wnd = MyEnsureCleanup.m_Wnd;
        m_Dc = MyEnsureCleanup.m_Dc;
 
        MyEnsureCleanup.m_Wnd = NULL;
        MyEnsureCleanup.m_Dc = NULL;
 
        return *this;
      }
 
      // Destructor
      ~EnsureReleaseDc()
      {
        Cleanup();
      }
 
      // Assignment operator (for HDC values)
      EnsureReleaseDc& operator= (HDC Dc)
      {
        Cleanup();
 
        m_Dc = Dc;
 
        return *this;
      }
 
      // Implicit conversion operator for BOOL
      operator HDC() const
      {
        return m_Dc;
      }
 
      // Cleanup the object if the value represents a valid object
      void Cleanup()
      {
        if (m_Wnd != NULL && m_Dc != NULL)
        {
          ReleaseDC(m_Wnd, m_Dc);
 
          m_Wnd = NULL;
          m_Dc = NULL;
        }
      }
 
    private:
      // Handles being managed
      HWND m_Wnd;
      HDC m_Dc;
    };

	// Special template class for objects which needs T->Release() at destruction
	template <typename T>
	class EnsureRelease : private boost::noncopyable
	{
	public:
		EnsureRelease(T* pv = NULL)
		: m_pv(pv)
		{ }
 
		~EnsureRelease()
		{
		Cleanup();
		}
 
		EnsureRelease(EnsureRelease&& MyEnsureCleanup)
		: m_pv(NULL)
		{
		*this = std::move(MyEnsureCleanup);
		}
 
		EnsureRelease& operator= (EnsureRelease&& MyEnsureCleanup)
		{
		Cleanup();
 
		m_pv = MyEnsureCleanup.m_pv;
 
		MyEnsureCleanup.m_pv = 0;
 
		return *this;
		}
 
		EnsureRelease& operator= (T* pv)
		{
		Cleanup();
 
		m_pv = pv;
 
		return *this;
		}
 
		operator T*() const
		{
		return m_pv;
		}

/*		EnsureRelease& operator ()(T** arg)
		{
			Cleanup();
			m_pv = *arg;
			*arg = 0;
			return *this;
		}
 */
		void Cleanup()
		{
			if (m_pv != NULL)
			{
				m_pv->Release();
 
				m_pv = NULL;
			}
		}

		T** operator & ()
		{
			return &m_pv;
		}

		T* operator -> ()
		{
			return m_pv;
		}
 
	private:
		// Handle being managed
		T* m_pv;
	};

	// Special class for memory allocated by VirtualAlloc(Ex)
	class EnsureVirtualFree : private boost::noncopyable
	{
	public:
		EnsureVirtualFree(LPVOID pv = NULL, HANDLE h=GetCurrentProcess())
		: m_pv(pv), m_h(h)
		{ }
 
		~EnsureVirtualFree()
		{
		Cleanup();
		}
 
		EnsureVirtualFree(EnsureVirtualFree&& MyEnsureCleanup)
		: m_pv(NULL), m_h(NULL)
		{
		*this = std::move(MyEnsureCleanup);
		}
 
		EnsureVirtualFree& operator= (EnsureVirtualFree&& MyEnsureCleanup)
		{
		Cleanup();
 
		m_pv = MyEnsureCleanup.m_pv;
		m_h = MyEnsureCleanup.m_h;
 
		MyEnsureCleanup.m_pv = 0;
		MyEnsureCleanup.m_h = 0;
 
		return *this;
		}
 
		EnsureVirtualFree& operator= (LPVOID pv)
		{
		Cleanup();
 
		m_pv = pv;
		if(!m_h) m_h = GetCurrentProcess();
 
		return *this;
		}
 
		operator LPVOID() const
		{
		return m_pv;
		}

		operator BYTE*() const
		{
			return (BYTE*)m_pv;
		}

		bool operator ! () const
		{
			return m_pv == NULL;
		}

		void Cleanup()
		{
			if (m_pv != NULL)
			{
				VirtualFreeEx(m_h, m_pv, 0, MEM_RELEASE);
 
				m_pv = NULL;
			}
		}

		LPVOID* operator & ()
		{
			return &m_pv;
		}
 
	private:
		// Handle being managed
		LPVOID m_pv;
		HANDLE m_h;
	};
  } // namespace Windows
} // namespace Hades