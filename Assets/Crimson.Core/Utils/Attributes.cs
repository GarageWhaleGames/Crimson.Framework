namespace Crimson.Core.Utils
{
    
    public class CastToUI : System.Attribute
    {
        private string _fieldId;

        public string FieldId => _fieldId;

        public CastToUI(string fieldId)
        {
            _fieldId = fieldId;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field |
                           System.AttributeTargets.Property)]
    public class LevelableValue : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Class |
                           System.AttributeTargets.Struct)]
    public class NetworkSimObject : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Field |
                           System.AttributeTargets.Property)]
    public class NetworkSimData : System.Attribute
    {
    }
}