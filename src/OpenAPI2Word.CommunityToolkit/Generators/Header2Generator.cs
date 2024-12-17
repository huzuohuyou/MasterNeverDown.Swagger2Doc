namespace OpenAPI2Word.CommunityToolkit.Generators
{
    internal class Header2Generator
    {
        public void Generate(XWPFDocument doc,string title)
        {
            var p0 = doc.CreateParagraph();
            p0.Alignment = ParagraphAlignment.LEFT;
            XWPFRun r0 = p0.CreateRun();
            r0.FontFamily = "microsoft yahei";
            r0.FontSize = 16;
            r0.IsBold = true;
            r0.SetColor("#009B3E");
            r0.SetText(title);
        }
    }
}
