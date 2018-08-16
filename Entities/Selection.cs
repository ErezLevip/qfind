namespace qfind.Entities
{
    public class Selection
    {
        public string SearchKey {get;set;}
        public int Count {get;set;}
         public Selection(int count, string key)
        {
            SearchKey = key;
            Count = count;
        }
    }
}