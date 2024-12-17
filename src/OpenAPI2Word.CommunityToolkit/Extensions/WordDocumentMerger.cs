using Microsoft.Office.Interop.Word;
using System.Reflection;
using Document = Microsoft.Office.Interop.Word.Document;

namespace OpenAPI2Word.CommunityToolkit.Extensions
{
    ///
    /// Word文档合并类
    ///
    public class WordDocumentMerger
    {
        private ApplicationClass objApp = null;
        private Document objDocLast = null;
        private Document objDocBeforeLast = null;
        public WordDocumentMerger()
        {
            objApp = new ApplicationClass();
        }
        #region 打开文件
        private void Open(string tempDoc)
        {
            object objTempDoc = tempDoc;
            object objMissing = System.Reflection.Missing.Value;

            objDocLast = objApp.Documents.Open(
                 ref objTempDoc,    //FileName
                 ref objMissing,   //ConfirmVersions
                 ref objMissing,   //ReadOnly
                 ref objMissing,   //AddToRecentFiles
                 ref objMissing,   //PasswordDocument
                 ref objMissing,   //PasswordTemplate
                 ref objMissing,   //Revert
                 ref objMissing,   //WritePasswordDocument
                 ref objMissing,   //WritePasswordTemplate
                 ref objMissing,   //Format
                 ref objMissing,   //Enconding
                 ref objMissing,   //Visible
                 ref objMissing,   //OpenAndRepair
                 ref objMissing,   //DocumentDirection
                 ref objMissing,   //NoEncodingDialog
                 ref objMissing    //XMLTransform
                 );

            objDocLast.Activate();
        }
        #endregion

        #region 保存文件到输出模板
        private void SaveAs(string outDoc)
        {
            object objMissing = System.Reflection.Missing.Value;
            object objOutDoc = outDoc;
            objDocLast.SaveAs(
              ref objOutDoc,      //FileName
              ref objMissing,     //FileFormat
              ref objMissing,     //LockComments
              ref objMissing,     //PassWord    
              ref objMissing,     //AddToRecentFiles
              ref objMissing,     //WritePassword
              ref objMissing,     //ReadOnlyRecommended
              ref objMissing,     //EmbedTrueTypeFonts
              ref objMissing,     //SaveNativePictureFormat
              ref objMissing,     //SaveFormsData
              ref objMissing,     //SaveAsAOCELetter,
              ref objMissing,     //Encoding
              ref objMissing,     //InsertLineBreaks
              ref objMissing,     //AllowSubstitutions
              ref objMissing,     //LineEnding
              ref objMissing      //AddBiDiMarks
              );
        }
        #endregion

        #region 循环合并多个文件（复制合并重复的文件）
        ///
        /// 循环合并多个文件（复制合并重复的文件）
        ///
        /// 模板文件
        /// 需要合并的文件
        /// 合并后的输出文件
        public void CopyMerge(string tempDoc, string[] arrCopies, string outDoc)
        {
            object objMissing = Missing.Value;
            object objFalse = false;
            object objTarget = WdMergeTarget.wdMergeTargetSelected;
            object objUseFormatFrom = WdUseFormattingFrom.wdFormattingFromSelected;
            try
            {
                //打开模板文件
                Open(tempDoc);
                foreach (string strCopy in arrCopies)
                {
                    objDocLast.Merge(
                      strCopy,                //FileName   
                      ref objTarget,          //MergeTarget
                      ref objMissing,         //DetectFormatChanges
                      ref objUseFormatFrom,   //UseFormattingFrom
                      ref objMissing          //AddToRecentFiles
                      );
                    objDocBeforeLast = objDocLast;
                    objDocLast = objApp.ActiveDocument;
                    if (objDocBeforeLast != null)
                    {
                        objDocBeforeLast.Close(
                          ref objFalse,     //SaveChanges
                          ref objMissing,   //OriginalFormat
                          ref objMissing    //RouteDocument
                          );
                    }
                }
                //保存到输出文件
                SaveAs(outDoc);
                foreach (Document objDocument in objApp.Documents)
                {
                    objDocument.Close(
                      ref objFalse,     //SaveChanges
                      ref objMissing,   //OriginalFormat
                      ref objMissing    //RouteDocument
                      );
                }
            }
            finally
            {
                objApp.Quit(
                  ref objMissing,     //SaveChanges
                  ref objMissing,     //OriginalFormat
                  ref objMissing      //RoutDocument
                  );
                objApp = null;
            }
        }
        ///
        /// 循环合并多个文件（复制合并重复的文件）
        ///
        /// 模板文件
        /// 需要合并的文件
        /// 合并后的输出文件
        public void CopyMerge(string tempDoc, string strCopyFolder, string outDoc)
        {
            string[] arrFiles = Directory.GetFiles(strCopyFolder);
            CopyMerge(tempDoc, arrFiles, outDoc);
        }
        #endregion

        #region 循环合并多个文件（插入合并文件）
        ///
        /// 循环合并多个文件（插入合并文件）
        ///
        /// 模板文件
        /// 需要合并的文件
        /// 合并后的输出文件
        public void InsertMerge(string tempDoc, string[] arrCopies, string outDoc)
        {
            object objMissing = Missing.Value;
            object objFalse = false;
            object confirmConversion = false;
            object link = false;
            object attachment = false;
            try
            {
                //打开模板文件
                Open(tempDoc);
                foreach (string strCopy in arrCopies)
                {
                    objApp.Selection.InsertFile(
                        strCopy,
                        ref objMissing,
                        ref confirmConversion,
                        ref link,
                        ref attachment
                        );
                }
                //保存到输出文件
                SaveAs(outDoc);
                foreach (Document objDocument in objApp.Documents)
                {
                    objDocument.Close(
                      ref objFalse,     //SaveChanges
                      ref objMissing,   //OriginalFormat
                      ref objMissing    //RouteDocument
                      );
                }
            }
            finally
            {
                objApp.Quit(
                  ref objMissing,     //SaveChanges
                  ref objMissing,     //OriginalFormat
                  ref objMissing      //RoutDocument
                  );
                objApp = null;
            }
        }
        ///
        /// 循环合并多个文件（插入合并文件）
        ///
        /// 模板文件
        /// 需要合并的文件
        /// 合并后的输出文件
        public void InsertMerge(string tempDoc, string strCopyFolder, string outDoc)
        {
            string[] arrFiles = Directory.GetFiles(strCopyFolder);
            InsertMerge(tempDoc, arrFiles, outDoc);
        }
        #endregion
    }
}
