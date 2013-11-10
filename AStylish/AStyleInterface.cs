using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Anonymous.AStylish
{
    public enum Language
    {
        NA,
        Cpp,
        CSharp,
        Java
    }

    public class AStyleErrorArgs : EventArgs
    {
        public string Message { get; set; }

        public AStyleErrorArgs(string message)
        {
            Message = message;
        }
    }

    public class AStyleInterface
    {
        public delegate void AStyleErrorHandler(object sender, AStyleErrorArgs e);
        public event AStyleErrorHandler ErrorRaised;

        /// AStyleMainUtf16 Delegates.
        private SafeNativeMethods.AStyleMemAllocDelgate AStyleMemAlloc;
        private SafeNativeMethods.AStyleErrorDelgate AStyleError;

        /// Declare callback functions.
        public AStyleInterface()
        {
            AStyleMemAlloc = new SafeNativeMethods.AStyleMemAllocDelgate(OnAStyleMemAlloc);
            AStyleError = new SafeNativeMethods.AStyleErrorDelgate(OnAStyleError);
        }

        /// Call the AStyleMainUtf16 function in Artistic Style.
        /// An empty string is returned on error.
        public String FormatSource(String textIn, String options)
        {   // Return the allocated string
            // Memory space is allocated by OnAStyleMemAlloc, a callback function
            String sTextOut = String.Empty;
            try
            {
                IntPtr pText = SafeNativeMethods.AStyleMainUtf16(textIn, options, AStyleError, AStyleMemAlloc);
                if (pText != IntPtr.Zero)
                {
                    sTextOut = Marshal.PtrToStringUni(pText);
                    Marshal.FreeHGlobal(pText);
                }
            }
            catch (Exception e)
            {
                OnAStyleError(this, new AStyleErrorArgs(e.ToString()));
            }
            return sTextOut;
        }

        /// Get the Artistic Style version number.
        /// Does not need to terminate on error.
        /// But the exception must be handled when a function is called.
        public String GetVersion()
        {
            String sVersion = String.Empty;
            try
            {
                IntPtr pVersion = SafeNativeMethods.AStyleGetVersion();
                if (pVersion != IntPtr.Zero)
                {
                    sVersion = Marshal.PtrToStringAnsi(pVersion);
                }
            }
            catch (Exception e)
            {
                OnAStyleError(this, new AStyleErrorArgs(e.ToString()));
            }
            return sVersion;
        }

        /// Allocate the memory for the Artistic Style return string.
        private IntPtr OnAStyleMemAlloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        /// Display errors from Artistic Style .
        private void OnAStyleError(object source, AStyleErrorArgs args)
        {
            if (ErrorRaised != null)
            {
                ErrorRaised(source, args);
            }
        }

        private void OnAStyleError(int errorNumber, String errorMessage)
        {
            OnAStyleError(this, new AStyleErrorArgs(errorNumber + ": " + errorMessage));
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    internal static class SafeNativeMethods
    {
        private const String dllName = "Resources/AStyle";

        /// AStyleGetVersion DllImport.
        /// Cannot use String as a return value because Mono runtime will attempt to
        /// free the returned pointer resulting in a runtime crash.
        /// NOTE: CharSet.Unicode is NOT used here.
        [DllImport(dllName)]
        internal static extern IntPtr AStyleGetVersion();

        /// AStyleMainUtf16 DllImport.
        /// Cannot use String as a return value because Mono runtime will attempt to
        /// free the returned pointer resulting in a runtime crash.
        /// NOTE: CharSet.Unicode and wide strings are used here.
        [DllImport(dllName, CharSet = CharSet.Unicode)]
        internal static extern IntPtr AStyleMainUtf16(
            [MarshalAs(UnmanagedType.LPWStr)] String sIn,
            [MarshalAs(UnmanagedType.LPWStr)] String sOptions,
            AStyleErrorDelgate errorFunc,
            AStyleMemAllocDelgate memAllocFunc
        );

        /// AStyleMainUtf16 callbacks.
        /// NOTE: Wide strings are NOT used here.
        internal delegate IntPtr AStyleMemAllocDelgate(int size);
        internal delegate void AStyleErrorDelgate(
            int errorNum,
            [MarshalAs(UnmanagedType.LPStr)] String error
        );
    }
}
