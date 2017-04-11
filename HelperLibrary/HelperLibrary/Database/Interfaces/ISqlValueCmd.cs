namespace HelperLibrary.Database.Interfaces
{
    public interface ISqlValueCmd
    {
        string CreateCmd(params object[] values);
    }
}