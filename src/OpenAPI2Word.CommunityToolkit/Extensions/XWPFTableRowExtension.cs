namespace OpenAPI2Word.CommunityToolkit.Extensions
{
    public static class XWPFTableRowExtension
    {
        public static void SetColor(this XWPFTableRow row, string rgbStr)
        {
            row.GetTableCells().ForEach(c =>
            {
                c.SetColor(rgbStr);
            });
            
        }

        public static void SetBorderBottom(this XWPFTableRow row)
        {
            row.GetTableCells().ForEach(c =>
            {
                c.SetBorderBottom(XWPFTable.XWPFBorderType.NONE,0,0,"#ffffff");
            });

        }

        public static void MergeCells2(this XWPFTableRow row,int startIndex,int endIndex ,int width=650)
        {
            var p0 = row.GetTable();
            p0.SetColumnWidth(1, 650); /* 设置列宽 */
            p0.SetColumnWidth(2, 650); /* 设置列宽 */
            p0.SetColumnWidth(3, 650); /* 设置列宽 */
            p0.SetColumnWidth(4, 650); /* 设置列宽 */
            p0.SetColumnWidth(5, 650); /* 设置列宽 */
            row.MergeCells(startIndex,endIndex);
            var wlong = (endIndex - startIndex) * width;
            //row.GetTable().SetColumnWidth(startIndex, 1000);
            row.GetCell(startIndex).SetTableColumnWidth(wlong);
        }

        
    }
}
