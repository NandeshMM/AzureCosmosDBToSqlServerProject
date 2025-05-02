
using Newtonsoft.Json;

namespace DataStore.Implementation.DTO
{

    public class EscapeControlCharConverter : JsonConverter

    {

        public override bool CanConvert(Type objectType)

        {

            return objectType == typeof(string);

        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)

        {

            var raw = reader.Value?.ToString();

            if (string.IsNullOrEmpty(raw))

                return raw;

            var result = string.Concat(raw.Select(c =>

                char.IsControl(c) ? $"\\u{((int)c):x4}" : c.ToString()

            ));

            return result;

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)

        {

            writer.WriteValue(value?.ToString());

        }

    }



}
