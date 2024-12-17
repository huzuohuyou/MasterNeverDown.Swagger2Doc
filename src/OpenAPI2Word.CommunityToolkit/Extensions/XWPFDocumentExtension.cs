namespace OpenAPI2Word.CommunityToolkit.Extensions
{
    public static class XWPFDocumentExtension
    {
        public static void AddHeader(this XWPFDocument doc, string header,string footer, int c = 0)
        {
            //doc.Document.body.sectPr = new CT_SectPr();
            //CT_SectPr m_SectPr = doc.Document.body.sectPr;
            ////创建页眉
            //CT_Hdr m_Hdr = new CT_Hdr();
            //m_Hdr.AddNewP().AddNewR().AddNewT().Value = header;//页眉内容
            ////创建页眉关系（headern.xml）
            //XWPFRelation Hrelation = XWPFRelation.HEADER;
            //XWPFHeader m_h = (XWPFHeader)doc.CreateRelationship(Hrelation, XWPFFactory.GetInstance(), doc.HeaderList.Count + c);
          
            ////设置页眉
            //m_h.SetHeaderFooter(m_Hdr);
            //CT_HdrFtrRef m_HdrFtr2 = m_SectPr.AddNewHeaderReference();
            //XWPFRun r1 = m_h.Paragraphs[0].CreateRun();
            //r1.SetUnderline(UnderlinePatterns.Dash);
            //m_HdrFtr2.type = ST_HdrFtr.@default;
            //m_HdrFtr2.id = m_h.GetPackageRelationship().Id;

            ////创建页脚
            //CT_Ftr m_ftr = new CT_Ftr();
            //m_ftr.AddNewP().AddNewR().AddNewT().Value = footer;//页脚内容

            ////创建页脚关系（footern.xml）
            //XWPFRelation Frelation = XWPFRelation.FOOTER;
            //XWPFFooter m_f = (XWPFFooter)doc.CreateRelationship(Frelation, XWPFFactory.GetInstance(), doc.FooterList.Count + c);
            ////设置页脚
            //m_f.SetHeaderFooter(m_ftr);
            //CT_HdrFtrRef m_HdrFtr1 = m_SectPr.AddNewFooterReference();
            //m_HdrFtr1.type = ST_HdrFtr.@default;
        }

    }
}
