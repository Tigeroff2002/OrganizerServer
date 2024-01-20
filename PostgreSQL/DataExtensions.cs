using System.Reflection;

namespace PostgreSQL;

public static class DataExtensions
{
    public static TDATA Map<TDATA>(this object oldObject) where TDATA : new()
    {
        TDATA newObject = new TDATA();

        try
        {
            if (oldObject == null)
            {
                return newObject;
            }

            Type newObjType = typeof(TDATA);
            Type oldObjType = oldObject.GetType();

            var propertyList = newObjType.GetProperties();

            if (propertyList.Length > 0)
            {
                foreach (var newObjProp in propertyList)
                {
                    var oldProp = oldObjType.GetProperty(
                        newObjProp.Name,
                        BindingFlags.Public | BindingFlags.NonPublic
                        | BindingFlags.Instance | BindingFlags.IgnoreCase
                        | BindingFlags.ExactBinding);

                    if (oldProp != null 
                        && oldProp.CanRead 
                        && newObjProp.CanWrite)
                    {
                        var oldPropertyType = oldProp.PropertyType;
                        var newPropertyType = newObjProp.PropertyType;

                        if (oldPropertyType.IsGenericType 
                            && oldPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            oldPropertyType = oldPropertyType.GetGenericArguments()[0];
                        }

                        if (newPropertyType.IsGenericType 
                            && newPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            newPropertyType = newPropertyType.GetGenericArguments()[0];
                        }

                        if (newPropertyType == oldPropertyType)
                        {
                            var value = oldProp.GetValue(oldObject);
                            newObjProp.SetValue(newObject, value);
                        }
                    }
                }
            }
        }

        catch (Exception)
        {
            // If there is an exception, log it
        }

        return newObject;
    }
}
