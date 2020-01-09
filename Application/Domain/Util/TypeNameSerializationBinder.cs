#region

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

#endregion

namespace Application.Domain.Util
{
    public class TypeNameSerializationBinder : ISerializationBinder
    {
        private readonly IEnumerable<Type> _allTypes;

        public TypeNameSerializationBinder()
        {
            _allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            var type = _allTypes.FirstOrDefault(x => x.Name == typeName);
            return type;
        }
    }
}