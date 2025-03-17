namespace poplensFeedApi.Models.Common {
    public class PageResult<T> {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Result { get; set; }
    }
}
