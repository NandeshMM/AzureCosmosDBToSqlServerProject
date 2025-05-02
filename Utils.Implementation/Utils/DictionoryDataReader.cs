using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Utils.Implementation.Utils
{
    public class DictionaryDataReader : IDataReader
    {
        private readonly List<IDictionary<string, object>> _records;
        private readonly List<string> _columns;
        private int _currentIndex = -1;

        public DictionaryDataReader(List<IDictionary<string, object>> records, List<string> columns)
        {
            _records = records ?? throw new ArgumentNullException(nameof(records));
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public bool Read()
        {
            _currentIndex++;
            return _currentIndex < _records.Count;
        }

        public int FieldCount => _columns.Count;

        public object GetValue(int i)
        {
            var key = _columns[i];

            if (_records[_currentIndex].TryGetValue(key, out var value))
            {
                return ConvertToSqlCompatibleValue(value);
            }

            var variations = new[] { key.ToLower(), char.ToLower(key[0]) + key.Substring(1) };
            foreach (var variant in variations)
            {
                if (_records[_currentIndex].TryGetValue(variant, out var altValue))
                {
                    return ConvertToSqlCompatibleValue(altValue);
                }
            }

            return DBNull.Value;
        }

        public string GetName(int i) => _columns[i];

        public int GetOrdinal(string name) => _columns.IndexOf(name);

        public Type GetFieldType(int i) => typeof(object);

        public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));

        #region Not Implemented (safe defaults)

        public void Dispose() { }
        public string GetDataTypeName(int i) => "object";
        public bool NextResult() => false;
        public int Depth => 0;
        public bool IsClosed => false;
        public int RecordsAffected => -1;

        public bool GetBoolean(int i) => (bool)GetValue(i);
        public byte GetByte(int i) => (byte)GetValue(i);
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public char GetChar(int i) => (char)GetValue(i);
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public IDataReader GetData(int i) => throw new NotSupportedException();
        public DateTime GetDateTime(int i) => (DateTime)GetValue(i);
        public decimal GetDecimal(int i) => (decimal)GetValue(i);
        public double GetDouble(int i) => (double)GetValue(i);
        public float GetFloat(int i) => (float)GetValue(i);
        public Guid GetGuid(int i) => (Guid)GetValue(i);
        public short GetInt16(int i) => (short)GetValue(i);
        public int GetInt32(int i) => (int)GetValue(i);
        public long GetInt64(int i) => (long)GetValue(i);
        public string GetString(int i) => GetValue(i)?.ToString();

        #endregion

        public void Close() { }

        DataTable? IDataReader.GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        int IDataRecord.GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        private static object ConvertToSqlCompatibleValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            return value switch
            {
                JObject jObj => jObj.ToString(Formatting.None),
                JValue jValue => jValue.Value ?? DBNull.Value,
                JToken jToken => jToken.ToString(Formatting.None),
                string str => str,
                _ => value
            };
        }
    }
}
