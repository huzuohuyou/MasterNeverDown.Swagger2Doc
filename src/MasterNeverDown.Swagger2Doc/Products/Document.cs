namespace OpenApi2Doc.CommunityToolkit.Products
{
    public abstract class Document
    {
        protected OpenApiDocument ApiDocument;
        private async Task CreateOpenApiDocument(string? requestUri, string savePath = "")
        {
            var client = new HttpClient();
            var stream = await client.GetStreamAsync(requestUri);
            ApiDocument = new OpenApiStreamReader().Read(stream, out _);
        }

        public abstract Title Title { get; set; }
        public abstract Toc Toc { get; set; }
        public abstract Info Info { get; set; }
        public abstract Services Services { get; set; }
        public abstract void SaveDocument();
    }
}
