using OpenApi2Doc.CommunityToolkit.Builders;

namespace OpenApi2Doc.CommunityToolkit.Directors
{
    public class DocumentDirector
    {
        private  DocumentBuilder _builder;

        public DocumentDirector(DocumentBuilder builder)

        {
          _builder = builder;
        }

        public void ChangeBuilder(DocumentBuilder builder)
        {
            _builder = builder;
        }

        public void Make(string type)
        {
            if (Equals(type,"default"))
            {
                _builder.BuildTitle();
                _builder.BuildToc();
                _builder.BuildToc();
                _builder.BuildServices();
            }

        }
    }
}
