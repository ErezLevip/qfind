namespace qfind.Entities
{
    public class FindOptions
    {
        public bool ExplicitValue {get;set;}
        public bool FileNameOnly {get;set;}
        public int MaxResults {get;set;}
        public bool DisplayResultInTerminal {get;set;}
        public FindOptions(string explicitValue,int maxResults,string displayResultInTerminal,string fileNameOnly = "true"){
            ExplicitValue = string.IsNullOrEmpty(explicitValue) ? false : bool.Parse(explicitValue);
            FileNameOnly = string.IsNullOrEmpty(fileNameOnly) ? false : bool.Parse(fileNameOnly);
            MaxResults = maxResults;
            DisplayResultInTerminal = string.IsNullOrEmpty(displayResultInTerminal) ? false : bool.Parse(displayResultInTerminal);




        }
    }
}