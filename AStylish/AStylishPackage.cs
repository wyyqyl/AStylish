using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Anonymous.AStylish
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "#114", IconResourceID = 400)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    [Guid(GuidList.guidAStylishPkgString)]
    public sealed class AStylishPackage : Package
    {
        private DTE dte_;
        private DocumentEventListener docEventListener_;
        private AStyleInterface aStyle_;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public AStylishPackage()
        {
            aStyle_ = new AStyleInterface();
            aStyle_.ErrorRaised += OnAStyleErrorRaised;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            dte_ = (DTE)GetService(typeof(DTE));
            docEventListener_ = new DocumentEventListener(this);
            docEventListener_.BeforeSave += OnBeforeDocumentSave;
        }

        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                if (disposing)
                {
                    if (docEventListener_ != null)
                    {
                        docEventListener_.BeforeSave -= OnBeforeDocumentSave;
                        docEventListener_.Dispose();
                        docEventListener_ = null;
                    }
                    GC.SuppressFinalize(this);
                }
                base.Dispose(disposing);
            }
        }
        #endregion

        private TextDocument GetTextDocument(Document doc)
        {
            if (doc == null || doc.ReadOnly)
            {
                return null;
            }

            return doc.Object("TextDocument") as TextDocument;
        }

        private Language GetLanguage(Document doc)
        {
            Language language = Language.NA;

            string lang = doc.Language.ToLower();
            if (lang == "gcc" || lang == "avrgcc" || lang == "c/c++")
            {
                language = Language.Cpp;
            }
            else if (lang == "csharp")
            {
                language = Language.CSharp;
            }

            return language;
        }

        private void OnBeforeDocumentSave(object source, OnBeforeSaveArgs args)
        {
            Document doc = null;
            string docName = docEventListener_.GetDocumentName(args.DocCookie);
            foreach (Document x in dte_.Documents)
            {
                if (x.FullName == docName)
                {
                    doc = x;
                    break;
                }
            }

            Language language = GetLanguage(doc);
            if (language != Language.NA)
            {
                FormatDocument(GetTextDocument(doc), language);
            }
        }

        private void FormatDocument(TextDocument textDoc, Language language)
        {
            if (textDoc == null || language == Language.NA)
            {
                return;
            }

            EditPoint sp = textDoc.StartPoint.CreateEditPoint();
            EditPoint ep = textDoc.EndPoint.CreateEditPoint();
            string text = sp.GetText(ep);

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            string formattedText = Format(text, language);
            if (!String.IsNullOrEmpty(formattedText))
            {
                sp.ReplaceText(ep, formattedText, (int)vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
            }
        }

        private string Format(string text, Language language)
        {
            string options = null;

            if (language == Language.CSharp)
            {
                options = null;
            }
            else if (language == Language.Cpp)
            {
                options = "--style=google --indent=spaces=2 --indent-modifiers --indent-switches --indent-cases --indent-preproc-define --indent-col1-comments --min-conditional-indent=0 --pad-oper --pad-header --align-pointer=type --align-reference=type --add-brackets --close-templates --max-code-length=80 --break-after-logical";
            }

            if (String.IsNullOrEmpty(options))
            {
                return null;
            }

            return aStyle_.FormatSource(text, options);
        }

        private void OnAStyleErrorRaised(object source, AStyleErrorArgs args)
        {
            MessageBox.Show(args.Message, "AStyle Formatter Error");
        }
    }
}
