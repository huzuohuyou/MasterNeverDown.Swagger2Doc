using System.Text.RegularExpressions;

namespace OpenAPI2Word.CommunityToolkit.Extensions
{
    internal static class XWPFTableCellExtension
    {
        public static void SetJsonOrBrText(this XWPFTableCell cell, string content)
        {
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            var contents = content.Split("<br>");
            if (contents.Length <= 1)
            {
                contents = content.Split("\r\n");
            }

            var index = 0;
            foreach (var s in contents)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                XWPFParagraph p1;
                if (index == 0)
                {
                    p1 = cell.Paragraphs[0];
                }
                else
                {
                    p1 = cell.AddParagraph();
                }

                p1.Alignment = ParagraphAlignment.LEFT;
                p1.FirstLineIndent = 20;
                XWPFRun r1 = p1.CreateRun();
                r1.FontFamily = "microsoft yahei";
                r1.FontSize = 10;
                r1.IsBold = false;
                p1.IndentFromLeft = 50 * (s.LastIndexOf('·') + 1);
                r1.SetText(s.Contains('·') ? s.Replace("·", "") : s);
                index++;
            }
        }

        public static void SetDescription(this XWPFTableCell cell, string content)
        {
            if (content.Contains("ws://p"))
            {
                var a = 1;
            }
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            var contents = content.Split("<br>");

            var split2 = new List<string>() { };

            foreach (var c in contents)
            {
                foreach (var c1 in c.Split("\r\n"))
                {
                    if (!string.IsNullOrWhiteSpace(c1))
                    {
                        split2.Add(c1);
                    }

                }
            }

            if (Equals(split2,null))
            {
             return;   
            }
            var index = 0;
            foreach (var line in split2)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                var append = 0;
                var enWordLength = Regex.Matches(line.Length > 35 ? line.Substring(0, 35) : line, @"[a-z|/|,|.|:|-]{1}").Count;

                var takeWordCount = 35+enWordLength/2+5;
        
                for (int i = 0; i < line.Length; i += takeWordCount)
                {
                    XWPFParagraph p1;
                    if (index == 0)
                        p1 = cell.Paragraphs[0];
                    else
                        p1 = cell.AddParagraph();
                    p1.Alignment = ParagraphAlignment.LEFT;
                    p1.IsWordWrapped=true;
                    XWPFRun r1 = p1.CreateRun();
                    p1.VerticalAlignment = TextAlignment.CENTER;
                    p1.FirstLineIndent = 40;
                    r1.FontFamily = "microsoft yahei";
                    r1.FontSize = 10;
                    p1.IndentFromLeft = 50 * (line.LastIndexOf('·') + 1);
                    var s = line.Contains('·') ? line.Replace("·", "") : line;
                    r1.SetText(new string(s.Skip(i).Take(takeWordCount).ToArray()));
                    index++;
                }

                //p1.Alignment = ParagraphAlignment.LEFT;
                //p1.FirstLineIndent = 40;
                //XWPFRun r1 = p1.CreateRun();
                //r1.FontFamily = "microsoft yahei";
                //r1.FontSize = 10;
                //r1.IsBold = false;
                //p1.IndentFromLeft = 50 * (s.LastIndexOf('·') + 1);
                //r1.SetText(s.Contains('·') ? s.Replace("·", "") : s);
                
            }
        }

        public static void SetText3(this XWPFTableCell cell, string content)
        {
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            XWPFParagraph p1 = cell.Paragraphs[0];
            XWPFRun r1 = p1.CreateRun();
            p1.VerticalAlignment = TextAlignment.CENTER;
            p1.FirstLineIndent = 20;
            r1.FontFamily = "microsoft yahei";
            r1.FontSize = 10;
            r1.SetText(content);
        }

        public static void SetColumCellText(this XWPFTableCell cell, string content, int length = 12)
        {
            for (int i = 0; i < cell.Paragraphs.Count; i++)
            {
                var h = cell.GetTableRow().Height;
                if (i>0)
                {
                    cell.Paragraphs.RemoveAt(i);
                }
            }
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            for (int i = 0; i < content.Length; i += length)
            {
                XWPFParagraph p1;
                if (i == 0)
                    p1 = cell.Paragraphs[0];
                else
                    p1 = cell.AddParagraph();

                XWPFRun r1 = p1.CreateRun();
                p1.VerticalAlignment = TextAlignment.CENTER;
                p1.FirstLineIndent = 20;
                r1.FontFamily = "microsoft yahei";
                r1.FontSize = 10;
                //p1.IndentFromLeft = 50 * (content.LastIndexOf('·') + 1);
                var s = (content.Contains('·') ? content.Replace("·", "") : content).Trim();
                r1.SetText(new string(s.Skip(i).Take(length).ToArray()));
            }

        }



        public static void SetTableHeaderText(this XWPFTableCell cell, string content)
        {
            cell.SetVerticalAlignment(XWPFTableCell.XWPFVertAlign.CENTER);
            cell.GetTableRow().SetColor("696969");
            XWPFParagraph p1 = cell.Paragraphs[0];
            p1.SpacingBeforeLines = 20;
            p1.SpacingAfterLines = 20;
            p1.FirstLineIndent = 40;
            p1.VerticalAlignment = TextAlignment.CENTER;
            XWPFRun r1 = p1.CreateRun();
            r1.FontFamily = "microsoft yahei";
            r1.FontSize = 10;
            r1.IsBold = true;
            r1.SetColor("#ffffff");
            r1.SetText(content.Trim());
        }

        public static XWPFTableCell SetTableColumnWidth(this XWPFTableCell cell, int width)
        {
            //不好使
            //var tcpr = cell.GetCTTc().AddNewTcPr();

            //var cellw = tcpr.AddNewTcW();
            //cellw.type = ST_TblWidth.dxa;
            //cellw.w = width.ToString();

            //不好使
            CT_TcPr m_Pr = cell.GetCTTc().AddNewTcPr();
            m_Pr.tcW = new CT_TblWidth();
            m_Pr.tcW.w = width.ToString();
            m_Pr.tcW.type = ST_TblWidth.dxa;
            return cell;
        }
    }
}

