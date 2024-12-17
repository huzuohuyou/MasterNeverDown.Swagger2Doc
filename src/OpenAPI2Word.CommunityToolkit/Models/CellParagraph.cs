namespace OpenAPI2Word.CommunityToolkit.Models
{
    internal class CellParagraph
    {
        internal XWPFParagraph cellParagraph;
        public CellParagraph(XWPFTable table, string content)
        {

            var para = new CT_P();
            cellParagraph = new XWPFParagraph(para, table.Body);
            cellParagraph.Alignment = ParagraphAlignment.LEFT;  
            cellParagraph.VerticalAlignment = TextAlignment.CENTER; 

            var r0 = cellParagraph.CreateRun();
            r0.FontFamily = "microsoft yahei";
            r0.FontSize = 10;
            r0.IsBold = false;
            var contents = content.Split(" <br>");
            //foreach (var s in contents)
            //{
            //    r0.AppendText(s);
            //    r0.AddCarriageReturn();
            //}

           
            
        }
    }
}
