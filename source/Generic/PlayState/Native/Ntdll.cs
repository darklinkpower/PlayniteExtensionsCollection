using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PlayState.Native
{
    public class Ntdll
    {
        private const string dllName = "ntdll.dll";

        [DllImport(dllName, PreserveSig = false)]
        public static extern void NtSuspendProcess(IntPtr processHandle);
        [DllImport(dllName, PreserveSig = false)]
        public static extern void NtResumeProcess(IntPtr processHandle);
    }
}