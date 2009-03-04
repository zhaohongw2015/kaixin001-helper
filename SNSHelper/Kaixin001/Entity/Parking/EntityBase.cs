
namespace SNSHelper.Kaixin001.Entity.Parking
{
    /// <summary>
    /// 实体类的基类
    /// </summary>
    public class EntityBase
    {
        protected Newtonsoft.Json.JavaScriptObject jsobj;

        public EntityBase()
        {
        }

        public EntityBase(object obj)
        {
            jsobj = obj as Newtonsoft.Json.JavaScriptObject;
        }
    }
}
