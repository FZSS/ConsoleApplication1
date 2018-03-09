namespace SneakerIcon.Classes.DAO
{
    public class RequestParam
    {
        public RequestParam(TypeParamEnum typeParam, object param)
        {
            TypeParam = typeParam;
            Param = param;
        }

        public TypeParamEnum TypeParam { get; set; }
        public object Param { get; set; }
    }
}
