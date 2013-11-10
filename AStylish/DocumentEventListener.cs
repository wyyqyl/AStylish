using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Anonymous.AStylish
{
    public class OnBeforeSaveArgs : EventArgs
    {
        public uint DocCookie { get; set; }
        public OnBeforeSaveArgs(uint docCookie)
        {
            DocCookie = docCookie;
        }
    }

    public class DocumentEventListener: IVsRunningDocTableEvents3
    {
        private RunningDocumentTable table_;
        private uint cookie_;

        public delegate void OnBeforeSaveHandler(object sender, OnBeforeSaveArgs e);
        public event OnBeforeSaveHandler BeforeSave;

        public DocumentEventListener(IServiceProvider package)
        {
            table_ = new RunningDocumentTable(package);
            cookie_ = table_.Advise(this);
        }

        public void Dispose()
        {
            if (table_ != null)
            {
                table_.Unadvise(cookie_);
                table_ = null;
                cookie_ = 0;
            }
        }

        public string GetDocumentName(uint docCookie)
        {
            return table_.GetDocumentInfo(docCookie).Moniker;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            if (BeforeSave != null)
            {
                BeforeSave(this, new OnBeforeSaveArgs(docCookie));
            }
            return VSConstants.S_OK;
        }
    }
}
