namespace OpenAPI2Word.CommunityToolkit.Generators
{
    internal class ContactNameGenerator
    {
        public void Generate(XWPFDocument doc,string title)
        {
            var p0 = doc.CreateParagraph();
            p0.Alignment = ParagraphAlignment.CENTER;
            XWPFRun r0 = p0.CreateRun();
            r0.FontFamily = "microsoft yahei";
            r0.FontSize = 12;
            r0.IsBold = true;
            r0.SetText(title);
        }
    }
}
