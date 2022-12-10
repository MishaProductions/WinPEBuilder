namespace WinPEBuilder.Core
{
    public delegate void BuilderEvent(bool error, int progress, string message);
    public delegate void BuilderLogEvent(string message);
}
