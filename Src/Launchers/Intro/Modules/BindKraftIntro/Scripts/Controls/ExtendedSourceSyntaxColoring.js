function ExtendedSourceSyntaxColoring() {
    SourceSyntaxColoring.apply(this, arguments);
}

ExtendedSourceSyntaxColoring.Inherit(SourceSyntaxColoring, "ExtendedSourceSyntaxColoring");

ExtendedSourceSyntaxColoring.ImplementProperty("test", new InitializeStringParameter("test", ""));
