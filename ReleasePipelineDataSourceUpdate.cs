foreach(var ds in Model.DataSources.OfType<StructuredDataSource>())
{
    var evName = ds.Name.Replace(" ", "");
    var evValue = Environment.GetEnvironmentVariable(evName);
    if (evValue != null)
        ds.Server = evValue;
}