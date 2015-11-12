using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DD.CBU.CaasDeploy.Library.Utilities
{
    /// <summary>
    /// Supports properties to be specified either a s a single value or an array.
    /// </summary>
    /// <typeparam name="T">The type of the array element</typeparam>
    public class JsonSingleOrArrayConverter<T> : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>True if the object can be converted.</returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        /// <summary>
        /// Reads the JSON value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The deserialized value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }

            return new List<T> { token.ToObject<T>() };
        }

        /// <summary>
        /// Writes the JSON value.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null && typeof(IEnumerable<T>).IsAssignableFrom(value.GetType()))
            {
                writer.WriteStartArray();

                foreach (var item in (IEnumerable<T>)value)
                {
                    writer.WriteValue(item.ToString());
                }

                writer.WriteEndArray();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
